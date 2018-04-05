using GraphQl.NetStandard.Client;
using Newtonsoft.Json.Linq;
using Octokit;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class GitHubApiRepositorySourceRepository : IRepositorySourceRepository
    {
        private IGitHubClient gitHubClient;
        private ITreesClient treesClient;
        private IGraphQLClient graphQLClient;

        public GitHubApiRepositorySourceRepository(IGitHubClient gitHubClient, ITreesClient treesClient, IGraphQLClient graphQLClient)
        {
            this.gitHubClient = gitHubClient;
            this.treesClient = treesClient;
            this.graphQLClient = graphQLClient;
        }

        public async Task<string> ReadFileContentAsync(string owner, string name, string fullFilePath)
        {
            var repositoryContent = await gitHubClient.Repository.Content.GetAllContents(owner, name, fullFilePath);

            if (repositoryContent != null && repositoryContent.Any())
            {
                return repositoryContent.First().Content;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<(string fullFilePath, string fileContent)>> GetMultipleFileContentsAsync(string repositoryOwner, string repositoryName, string branch, List<string> fullFilePaths)
        {
            var tupleList = new List<(string fullFilePath, string fileContent)>();


            var fileContentRequestBuilder = new StringBuilder();
            // Build up the list of file content requests.  This is needed because GraphQl does not allow
            // the returning of multiple nodes of the same name.  This loop builds up the file request json
            // aliases on these nodes
            for (int i = 0; i < fullFilePaths.Count; i++)
            {
                var fileContentRequestJson = GetFileContentRequestJson(i + 1, branch, fullFilePaths[i]);
                fileContentRequestBuilder.Append(fileContentRequestJson);
            }

            var query = $@"
            query ($repoName:String!, $repoOwner:String!){{
              repository(name:$repoName,  owner: $repoOwner) {{
                    {fileContentRequestBuilder.ToString()}
                }}
            }}
            ";

            var variables = new { repoOwner = repositoryOwner, repoName = repositoryName };

            var responseBodyString = await graphQLClient.QueryAsync(query, variables).ConfigureAwait(false);

            var jObject = JObject.Parse(responseBodyString);

            // Parse the aliased file
            for (int i = 0; i < fullFilePaths.Count; i++)
            {
                var fileContent = jObject["data"]["repository"][$"file{i + 1}"]["text"].Value<string>();

                tupleList.Add((fullFilePaths[i], fileContent));
            }

            return tupleList;

            string GetFileContentRequestJson(int index, string branchName, string fullFilePath)
            {
                return $@"
                file{index}: object(expression: ""{branch}:{fullFilePath}"") {{
                 ...on Blob {{
                     text
                    }}
                }}
            ";
            }
        }

        public async Task<List<RepositoryFile>> ReadFilesAsync(string owner, string name, string branch)
        {
            var treeResponse = await treesClient.GetRecursive(owner, name, branch);
            var treeItems = treeResponse.Tree;

            var repoFiles = new List<RepositoryFile>();

            if (treeItems != null && treeItems.Any())
            {
                foreach (var treeItem in treeItems)
                {
                    var codeRepoFile = new RepositoryFile();
                    codeRepoFile.FullPath = treeItem.Path;
                    codeRepoFile.Name = Path.GetFileName(codeRepoFile.FullPath);

                    repoFiles.Add(codeRepoFile);
                }
            }

            return repoFiles;
        }

        public async Task<ServiceModel.Repository> ReadRepositoryAsync(string repositoryOwner, string repositoryName)
        {
            var query = @"
            query ($repoName:String!, $repoOwner:String!){
              repository(owner: $repoOwner, name: $repoName){
	            name,
                url,
                pushedAt,
                createdAt,
                defaultBranchRef{
                  name
                },
                projects{
                  totalCount
                },
    		    issues{
                  totalCount
                },
                pullRequests{
                  totalCount
                }
                repositoryTopics(first:50){
                  nodes{
                    topic{
                      name
                    }
                  }
                }
              }
            }
            ";

            var variables = new { repoOwner = repositoryOwner, repoName = repositoryName };

            var responseBodyString = await graphQLClient.QueryAsync(query, variables).ConfigureAwait(false);

            var repository = MapFromGraphQlGitHubRepoBodyString(responseBodyString);

            return repository;

        }

        public async Task<CursorPagedResults<RepositorySourceRepository>> ReadRepositoriesAsync(string organization, string user, int take, string endCursor)
        {
            string loginType = null;
            string login = null;
            string endCursorQuerySegment = string.Empty;

            if (!string.IsNullOrWhiteSpace(user))
            {
                loginType = "user";
                login = user;
            }
            else
            {
                loginType = "organization";
                login = organization;
            }

            if (!string.IsNullOrWhiteSpace(endCursor))
            {
                endCursorQuerySegment = $", after: \"{endCursor}\"";
            }


            var query = $@"
            query ($login: String!, $take: Int) {{
              {loginType}(login:$login){{
                repositories(first: $take, orderBy: {{field:PUSHED_AT, direction:DESC}}{endCursorQuerySegment}){{
                  edges{{
                    node{{
                      url,
                      createdAt,
                      pushedAt
                    }}
                  }},
                pageInfo{{
                  hasNextPage,
                  endCursor
                }}
              }}
             }}
            }}
            ";

            var variables = new { login = login, take = take };

            var responseBodyString = await graphQLClient.QueryAsync(query, variables).ConfigureAwait(false);

            var jObject = JObject.Parse(responseBodyString);
            dynamic repositories = jObject["data"][loginType]["repositories"];

            var cursorPagedResults = new CursorPagedResults<RepositorySourceRepository>();
            cursorPagedResults.EndCursor = repositories.pageInfo.endCursor;
            cursorPagedResults.MoreToRead = repositories.pageInfo.hasNextPage;

            var results = new List<RepositorySourceRepository>();

            foreach (var edge in repositories.edges)
            {
                var repositorySourceRepository = new RepositorySourceRepository();
                repositorySourceRepository.CreatedAt = edge.node.createdAt;
                repositorySourceRepository.UpdatedAt = edge.node.pushedAt;
                repositorySourceRepository.Url = edge.node.url;

                results.Add(repositorySourceRepository);
            }

            cursorPagedResults.Results = results;

            return cursorPagedResults;
        }


        public async Task<Dictionary<string, List<string>>> ReadTeamToRepositoriesMaps(string organization)
        {
            var teamToRespositoriesMap = new Dictionary<string, List<string>>();

            var endCursorQuerySegment = "";
            var moreTeamsToRead = true;

            while (moreTeamsToRead)
            {
                var allTeamsRepositoriesQuery = $@"
                query ($login: String!) {{
                  organization(login: $login) {{
                    teams(first: 100, {endCursorQuerySegment} orderBy:{{field:NAME, direction:ASC}}) {{
                      nodes {{
                        name
                        repositories(first: 100) {{
                          nodes {{
                            name
                          }}
                          pageInfo {{
                            endCursor
                            hasNextPage
                          }}
                        }}
                      }}
                      pageInfo {{
                        endCursor
                        hasNextPage
                      }}
                    }}
                  }}
                }}
               ";

                var allTeamsRepositoriesVariables = new { login = organization };

                var allTeamsRepositoriesResponseBodyString = await graphQLClient.QueryAsync(allTeamsRepositoriesQuery, allTeamsRepositoriesVariables);

                dynamic jObject = JObject.Parse(allTeamsRepositoriesResponseBodyString);

                var numberOfTeams = jObject.data.organization.teams.nodes.Count;

                for (int i = 0; i < numberOfTeams; i++)
                {
                    var teamNode = jObject.data.organization.teams.nodes[i];
                    var teamName = teamNode.name.Value;

                    var numberOfTeamRepositories = teamNode.repositories.nodes.Count;

                    var teamRepositoryNames = new List<string>();

                    for (int j = 0; j < numberOfTeamRepositories; j++)
                    {
                        var repositoryName = teamNode.repositories.nodes[j].name.Value;
                        teamRepositoryNames.Add(repositoryName);
                    }

                    // Now get the additional pages of repositories if we need to
                    bool moreTeamRepositoriesToRead = teamNode.repositories.pageInfo.hasNextPage.Value;
                    var afterCursor = teamNode.repositories.pageInfo.endCursor.Value;

                    while (moreTeamRepositoriesToRead)
                    {

                        var result = await GetAdditionalTeamRepositoriesAsync(teamName, afterCursor);

                        teamRepositoryNames.AddRange(result.TeamNames);

                        if (result.AfterCursor != null)
                        {
                            moreTeamRepositoriesToRead = true;
                            afterCursor = result.AfterCursor;
                        }
                        else
                        {
                            moreTeamRepositoriesToRead = false;
                        }
                    }

                    teamToRespositoriesMap.Add(teamName, teamRepositoryNames);
                }

                moreTeamsToRead = jObject.data.organization.teams.pageInfo.hasNextPage.Value;

                if (moreTeamsToRead)
                {
                    var endCursor = jObject.data.organization.teams.pageInfo.endCursor.Value;
                    endCursorQuerySegment = $"after: \"{endCursor}\" ,";
                }

            }

            return teamToRespositoriesMap;

            async Task<(List<string> TeamNames, string AfterCursor)> GetAdditionalTeamRepositoriesAsync(string teamName, string afterCursor)
            {
                var teamRepositoriesQuery = @"
                    query ($login: String!, $teamName: String!, $repositoriesAfter: String) {
                      organization(login: $login) {
                        teams(first: 1, query: $teamName) {
                          nodes {
                            name
                            repositories(first: 100, after: $repositoriesAfter) {
                              nodes {
                                name
                              }
                              pageInfo {
                                endCursor
                                hasNextPage
                              }
                            }
                          }
                        }
                      }
                    }
                ";

                var teamRepositoriesVariables = new { login = organization, teamName = teamName, repositoriesAfter = afterCursor };

                var responseBodyString = await graphQLClient.QueryAsync(teamRepositoriesQuery, teamRepositoriesVariables).ConfigureAwait(false);

                dynamic jObject2 = JObject.Parse(responseBodyString);

                var teamNode = jObject2.data.organization.teams.nodes[0];

                var numberofRepositories = teamNode.repositories.nodes.Count;

                var repositoryNames = new List<string>();
                string nextAfterCursor = null;

                for (int i = 0; i < numberofRepositories; i++)
                {
                    var repositoryName = teamNode.repositories.nodes[i].name.Value;

                    repositoryNames.Add(repositoryName);
                }

                bool moreRepositoriesToRead = teamNode.repositories.pageInfo.hasNextPage.Value;

                if (moreRepositoriesToRead)
                {
                    nextAfterCursor = teamNode.repositories.pageInfo.endCursor.Value;
                }

                return (repositoryNames, nextAfterCursor);
            }
        }



        private ServiceModel.Repository MapFromGraphQlGitHubRepoBodyString(string responseBodyString)
        {
            var codeRepository = new ServiceModel.Repository();

            dynamic jObject = JObject.Parse(responseBodyString);

            codeRepository.Id = jObject.data.repository.url;
            codeRepository.Name = jObject.data.repository.name;
            codeRepository.CreatedOn = jObject.data.repository.createdAt;
            codeRepository.LastUpdatedOn = jObject.data.repository.pushedAt;

            if (jObject.data.repository.defaultBranchRef != null)
            {
                codeRepository.DefaultBranch = jObject.data.repository.defaultBranchRef.name;
            }

            var projectCount = jObject.data.repository.projects.totalCount;
            codeRepository.HasProjects = projectCount > 0;

            var issueCount = jObject.data.repository.issues.totalCount;
            codeRepository.HasIssues = issueCount > 0;

            var pullRequestCount = jObject.data.repository.pullRequests.totalCount;
            codeRepository.HasPullRequests = pullRequestCount > 0;

            var numberOfTopics = jObject.data.repository.repositoryTopics.nodes.Count;

            if (numberOfTopics > 0)
            {
                var topics = new List<string>();

                for (int i = 0; i < numberOfTopics; i++)
                {
                    var topicName = jObject.data.repository.repositoryTopics.nodes[i].topic.name.Value;
                    topics.Add(topicName);
                }

                codeRepository.Topics = topics;
            }

            return codeRepository;
        }
    }
}

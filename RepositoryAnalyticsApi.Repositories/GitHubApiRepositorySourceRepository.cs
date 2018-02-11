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

        public async Task<string> ReadFileContentAsync(string owner,string name, string fullFilePath)
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

        public List<(string fullFilePath, string fileContent)> GetMultipleFileContents(string repositoryOwner, string repositoryName, string branch, List<string> fullFilePaths)
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

            var variables = new { repoOwner = repositoryOwner, repoName = repositoryName};

            var responseBodyString = graphQLClient.QueryAsync(query, variables).Result;

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

        public async Task<List<RepositoryFile>> ReadFilesAsync(string owner, string name,  string branch)
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

            var variables = new { repoOwner = repositoryOwner, repoName = repositoryName};

            var responseBodyString = await graphQLClient.QueryAsync(query, variables).ConfigureAwait(false);

            var repository = MapFromGraphQlGitHubRepoBodyString(responseBodyString);

            return repository;
        }

        private ServiceModel.Repository MapFromGraphQlGitHubRepoBodyString(string responseBodyString)
        {
            var codeRepository = new ServiceModel.Repository();

            dynamic jObject = JObject.Parse(responseBodyString);

            codeRepository.Id = jObject.data.repository.url;
            codeRepository.Name = jObject.data.repository.name;
            codeRepository.CreatedOn = jObject.data.repository.createdAt;
            codeRepository.LastUpdatedOn = jObject.data.repository.pushedAt;
            codeRepository.DefaultBranch = jObject.data.repository.defaultBranchRef.name;

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

        private ServiceModel.Repository MapFromGitHubRepo(Octokit.Repository gitHubRepository)
        {
            var codeRepository = new ServiceModel.Repository();
            codeRepository.CreatedOn = gitHubRepository.CreatedAt.LocalDateTime;
            codeRepository.LastUpdatedOn = gitHubRepository.UpdatedAt.LocalDateTime;
            codeRepository.Id = gitHubRepository.Id.ToString();
            codeRepository.Name = gitHubRepository.Name;
            codeRepository.DefaultBranch = gitHubRepository.DefaultBranch;

            return codeRepository;
        }
    }
}

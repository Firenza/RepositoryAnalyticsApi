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

        public string GetFileContent(string repositoryId, string fullFilePath)
        {
            var gitHubRepoId = Convert.ToInt64(repositoryId);

            var fileContent = gitHubClient.Repository.Content.GetAllContents(gitHubRepoId, fullFilePath).Result.First().Content;

            return fileContent;
        }

        public List<(string fullFilePath, string fileContent)> GetMultipleFileContents(string repositoryName, string repositoryOwner, string branch, List<string> fullFilePaths)
        {
            var tupleList = new List<(string fullFilePath, string fileContent)>();

            // Build up the list of file content requests
            var fileContentRequestBuilder = new StringBuilder();

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

            var variables = new { repoName = repositoryName, repoOwner = repositoryOwner };

            var responseBodyString = graphQLClient.QueryAsync(query, variables).Result;

            var jObject = JObject.Parse(responseBodyString);

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

        public List<RepositoryFile> ReadFiles(string repositoryId, string branch)
        {
            var gitHubRepoId = Convert.ToInt64(repositoryId);

            var treeItems = this.treesClient.GetRecursive(gitHubRepoId, branch).Result.Tree;

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

        public List<ServiceModel.Repository> ReadRepositories(string group, int pageCount, int pageSize, int startPage)
        {
            var gitHubRepos = gitHubClient.Repository.GetAllForOrg(group, new ApiOptions { PageSize = pageSize, PageCount = pageCount, StartPage = startPage }).Result;

            var codeRepositories = new List<ServiceModel.Repository>();

            if (gitHubRepos != null && gitHubRepos.Any())
            {
                foreach (var gitHubRepo in gitHubRepos)
                {
                    var codeRepository = MapFromGitHubRepo(gitHubRepo);

                    codeRepositories.Add(codeRepository);
                }
            }

            return codeRepositories;
        }

        public ServiceModel.Repository ReadRepository(string repositoryId)
        {
            var gitHubRepoId = Convert.ToInt64(repositoryId);

            var gitHubRepo = gitHubClient.Repository.Get(gitHubRepoId).Result;

            var codeRepository = MapFromGitHubRepo(gitHubRepo);

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

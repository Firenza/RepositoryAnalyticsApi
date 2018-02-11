using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MongoDependencyRepository : IDependencyRepository
    {
        private IMongoDatabase mongoDatabase;

        public MongoDependencyRepository(IMongoDatabase mongoDatabase)
        {
            this.mongoDatabase = mongoDatabase;
        }

        public async Task<List<RepositoryDependencySearchResult>> SearchAsync(string name)
        {
            var searchResults = new List<RepositoryDependencySearchResult>();

            var query = $@"
            {{
                aggregate: ""repository"",
                pipeline:
                [
                    {{ $match: {{ ""Dependencies.Name"" : ""{name}""}}}},
		            {{ $unwind: {{ path: ""$Dependencies""}}}},
		            {{ $match: {{ ""Dependencies.Name"": ""{name}""}}}},
                    {{ $group: {{ _id: {{ Name: ""$Dependencies.Name"", Version: ""$Dependencies.Version""}}, count : {{$sum: 1}}}}}},
		            {{ $sort: {{ count: -1 }}}},
	            ]
            }}        
            ";

            var command = new JsonCommand<BsonDocument>(query);
            var result = await mongoDatabase.RunCommandAsync(command);

            var jObject = JObject.Parse(result.ToJson());

            var numberOfSearchResults = jObject["result"].Count();

            if (numberOfSearchResults > 0)
            {
                for (int i = 0; i < numberOfSearchResults; i++)
                {
                    var searchResult = new RepositoryDependencySearchResult
                    {
                      Count = jObject["result"][0]["count"].Value<int>(),
                      RepositoryDependency = new RepositoryDependency
                      {
                          Name = jObject["result"][0]["_id"]["Name"].Value<string>(),
                          Version = jObject["result"][0]["_id"]["Version"].Value<string>()
                      }
                    };

                    searchResults.Add(searchResult);
                }
            }

            return searchResults;
        }


        public async Task<List<string>> SearchNamesAsync(string name)
        {
            var query = $@"
            {{
                aggregate: ""repository"",
                pipeline:
                [
                    {{ $match: {{ ""Dependencies.Name"" : /{name}/i}}}},
		            {{ $unwind: {{ path: ""$Dependencies""}}}},
		            {{ $match: {{ ""Dependencies.Name"": /{name}/i}}}},
		            {{ $group: {{ _id: {{ Name: ""$Dependencies.Name""}}}}}},
		            {{ $sort: {{ _id: 1 }}}},
	            ]
            }}        
            ";

            var command = new JsonCommand<BsonDocument>(query);
            var result = await mongoDatabase.RunCommandAsync(command);

            var jObject = JObject.Parse(result.ToJson());

            var numberOfNamesFound = jObject["result"].Count();

            if (numberOfNamesFound > 0)
            {
                return jObject["result"].Select(token => token["_id"]["Name"].Value<string>()).ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
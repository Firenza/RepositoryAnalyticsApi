using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.Repositories.Model.MySql
{
    public class Topic
    {
        public static List<Topic> MapFrom(RepositoryAnalyticsApi.ServiceModel.RepositoryCurrentState repositoryCurrentState, int newRepositoryCurrentStateId)
        {
            var newTopics = new List<Topic>();

            if (repositoryCurrentState.Topics != null)
            {
                foreach (var topic in repositoryCurrentState.Topics)
                {
                    var newTopic = new Topic
                    {
                        RepositoryCurrentStateId = newRepositoryCurrentStateId,
                        Name = topic
                    };

                    newTopics.Add(newTopic);
                }
            }

            return newTopics;
        }

        public int RepositoryCurrentStateId { get; set; }
        public string Name { get; set; }
    }
}

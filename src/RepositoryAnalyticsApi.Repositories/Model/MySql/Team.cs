﻿using System.Collections.Generic;

namespace RepositoryAnalyticsApi.Repositories.Model.MySql
{
    public class Team
    {
        public static List<Team> MapFrom(RepositoryAnalyticsApi.ServiceModel.RepositoryCurrentState repositoryCurrentState, int newRepositoryCurrentStateId)
        {
            var newTeams = new List<Team>();

            if (repositoryCurrentState.Teams != null)
            {
                foreach (var team in repositoryCurrentState.Teams)
                {
                    var newTeam = new Team
                    {
                        RepositoryCurrentStateId = newRepositoryCurrentStateId,
                        Name = team.Name,
                        Permission = team.Permission
                    };

                    newTeams.Add(newTeam);
                }
            }

            return newTeams;
        }


        public int RepositoryCurrentStateId { get; set; }
        public string Name { get; set; }
        public string Permission { get; set; }
    }
}

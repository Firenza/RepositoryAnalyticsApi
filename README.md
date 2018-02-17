# RepositoryAnaltyicsApi

An ASP.NET Core API to provide analyitical data about GitHub code repositories.  



#### Running locally

The API uses the following environment variables to configure how to connect to GitHub.

```
GITHUB_V3_API_URL
GITHUB_GRAPHQL_API_URL
GITHUB_ACCESS_TOKEN
```

To provide these to the API a run time you can do one of the following

1. Create a `GitHubConfiguration.env` file (in the same directory as the docker-compose.yml file) and put the variables / values in there.  This file has been added to the `.gitignore` file so you don't have to worry about commiting it by accident.

2. Provide the environment varibles in the `docker-compose` command
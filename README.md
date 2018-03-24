# RepositoryAnaltyicsApi

An ASP.NET Core API to provide analyitical data about GitHub code repositories.  



#### Running locally

The API uses the following environment variables to configure how to connect to GitHub.

```
GITHUB_ACCESS_TOKEN=YOUR_ACCESS_TOKEN
GITHUB_V3_API_URL=https://api.github.com
GITHUB_GRAPHQL_API_URL=https://api.github.com/graphql
MONGO_DB_CONNECTION=mongodb://localhost:27017
MONGO_DB_DATABASE=local
```

To provide these to the API a run time you can do one of the following

1. Create a `configuration.env` file (in the same directory as the docker-compose.yml file) and put the variables / values in there.  This file has been added to the `.gitignore` file so you don't have to worry about commiting it by accident.

2. Provide the environment varibles in the `docker-compose` command

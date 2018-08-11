# RepositoryAnaltyicsApi

An ASP.NET Core API to provide analyitical data about GitHub code repositories.  Paired with [orchestration wrapper application](https://github.com/Firenza/RepositoryAnaltyicsOrchestrator) to provide easy ingestion of GitHub data.

#### Purpose

Provide the ability to answer the questions like the following with respect to a group of GitHub repositories.

* How many NodeJS APIs do we currently have vs ASP.NET APIs?
* Our goal was to get 80% of our Web UI's switched to React.  Where are we at now with that and how much progress have we made in the last month?
* Our shared logging package has an critical security vulnerablity for all versions below 1.7.9.  How many repositories are still referencing one of these versions?
* Is anyone using dependency X with an application of type Y?

#### Tech Used

* ASP.NET Core Web API
* MongoDB (Likely to soon also support MySQL)

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

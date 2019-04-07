# RepositoryAnaltyicsApi

An ASP.NET Core API to provide analyitical data about GitHub code repositories.  Paired with [orchestration wrapper application](https://github.com/Firenza/RepositoryAnaltyicsOrchestrator) to provide easy ingestion of GitHub data.

#### Purpose

Provide the ability to answer the questions like the following with respect to a group of GitHub repositories.

* How many NodeJS APIs do we currently have vs ASP.NET APIs?
* Our goal was to get 80% of our Web UI's switched to React.  Where are we at now with that and how much progress have we made in the last month?
* Our shared logging package has an critical security vulnerablity for all versions below 1.7.9.  How many repositories are still referencing one of these versions?
* Is anyone using dependency X with an application of type Y?

The API must also provide the ability to be exended so that people can define thier own application types/implementations along with their level of DevOps integration. This is currently provided via [MEF plugin assemblies](https://github.com/Firenza/RepositoryAnaltyicsApiExampleExtension).

#### Tech Used

* ASP.NET Core Web API
* Swagger UI
* Entity Framwork Core
* PostgreSQL or SQL Server
* MEF
* Visual Studio 

#### Running Locally

The API uses the following environment variables to configure how to connect to GitHub.

```
GITHUB_ACCESS_TOKEN=YOUR_ACCESS_TOKEN
GITHUB_V3_API_URL=https://api.github.com
GITHUB_GRAPHQL_API_URL=https://api.github.com/graphql
DB_TYPE=postgresql
DB_CONNECTION_STRING=Server=localhost;Database=repository_analytics;UserID=bob;Password=bobiscool;
```

To provide these to the API a run time you can do one of the following

1. Create a `configuration.env` file (in the same directory as the docker-compose.yml file) and put the variables / values in there.  This file has been added to the `.gitignore` file so you don't have to worry about commiting it by accident.

2. Provide the environment varibles in the `docker-compose` command

##### Flushing Redis Cashe

Execute `docker exec -it repositoryAnalyticsApi_redis bash -c "redis-cli FLUSHALL"` to clear all the data

#### Integrating Extensions

1. Build the extension assembly
2. Copy all of the assemblies to the `./src/RepositoryAnalyticsApi/Plugins/` folder
3. Start the API and you check the output to see if the types in your plugin were loaded. You should see something like the following:

![](https://user-images.githubusercontent.com/9145108/43986475-e54f8664-9cd6-11e8-9135-2b6998cb853a.png)

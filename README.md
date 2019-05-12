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

#####  Configure the application

1. Update the default values in the `appsettings.Development.json` file (if desired)
1. Run `docker-compose -f dependencies-docker-compose.yml up -d` to start up the dependent apps.  This will also happen when Rebuilding the solution in Visual Studio
1. Set the `GithubAccessToken` and `DatabasePassword` values in the [.NET Core Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2&tabs=windows#secret-manager) 

##### Build and run the application

###### Via command line

1. Run `dotnet build`
1. Run `dotnet run`

###### Via Visual Studio

1. Set the startup project to be `RepositoryAnaltyicsApi` (I think this is done by default)
1. Set the startup method to just run the api directly (vs through IIS Express)
1. Click the green start button :)

##### Test out the API

You can naviage to http://localhost:33283/swagger/ to interact with the API


##### Flushing Redis Cashe

Execute `docker exec -it repositoryAnalyticsApi_redis bash -c "redis-cli FLUSHALL"` to clear all the data

#### Integrating Extensions

1. Build the extension assembly
2. Copy all of the assemblies to the `./src/RepositoryAnalyticsApi/Plugins/` folder
3. Start the API and you check the output to see if the types in your plugin were loaded. You should see something like the following:

![](https://user-images.githubusercontent.com/9145108/43986475-e54f8664-9cd6-11e8-9135-2b6998cb853a.png)

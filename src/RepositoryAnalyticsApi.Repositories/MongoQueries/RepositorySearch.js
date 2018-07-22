// Stages that have been excluded from the aggregation pipeline query
__3tsoftwarelabs_disabled_aggregation_stages = [

	{
		// Stage 3 - excluded
		stage: 3,  source: {
			$match: {
			   "RepositoryCurrentState.DevOpsIntegrations.ContinuousDelivery": true
			}
		}
	},
]

db.getCollection("repositorySnapshot").aggregate(

	// Pipeline
	[
		// Stage 1
		{
			$match: {
			   "$and" : 
			    [
			        { "TypesAndImplementations": { $elemMatch: {"TypeName": "API" } } }, 
			        { "TypesAndImplementations.Implementations": { $elemMatch: {"Name": "ASP.NET Core" } } },
			        { "Dependencies": { $elemMatch: {"Name": "Newtonsoft.Json", "Version": { $gte: "11.0.1" } } } },
			        { "WindowEndsOn": {$lte : ""}}
			    ]
			}
		},

		// Stage 2
		{
			$lookup: // Equality Match
			{
			    from: "repositoryCurrentState",
			    localField: "RepositoryCurrentStateId",
			    foreignField: "_id",
			    as: "RepositoryCurrentState"
			}
			
			// Uncorrelated Subqueries
			// (supported as of MongoDB 3.6)
			// {
			//    from: "<collection to join>",
			//    let: { <var_1>: <expression>, …, <var_n>: <expression> },
			//    pipeline: [ <pipeline to execute on the collection to join> ],
			//    as: "<output array field>"
			// }
		},

		// Stage 4
		{
			$project: {
			    "RepositoryCurrentState.Name": 1, "_id": 0
			}
		},

	]

	// Created with Studio 3T, the IDE for MongoDB - https://studio3t.com/

);

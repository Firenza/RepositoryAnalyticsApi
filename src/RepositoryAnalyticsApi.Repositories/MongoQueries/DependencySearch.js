db.getCollection("repositorySnapshot").aggregate(

	// Pipeline
	[
		// Stage 1
		{
			$match: {
			   "$and" : 
			    [
			         { "Dependencies.Name" : ".NET Standard" },
			        // { "WindowStartsOn": { $lte: "asOf" }},
			         //{ "WindowEndsOn": { $gte: "asOf" }}
			    ]
			}
		},

		// Stage 2
		{
			$unwind: {
			    path : "$Dependencies"
			}
		},

		// Stage 3
		{
			$match: {
			    "$and" : 
			    [
			         { "Dependencies.Name" : ".NET Standard"},
			         //{ "WindowStartsOn": { $lte: "asOf" }},
			         //{ "WindowEndsOn": { $gte: "asOf" }}
			    ]
			}
		},

		// Stage 4
		{
			$group: {
			     "_id" : { Name : "$Dependencies.Name", Version: "$Dependencies.Version" }, "count" : {"$sum": 1}
			}
		},

		// Stage 5
		{
			$sort: {
			    "_id.Name":1
			}
		},

	]

	// Created with Studio 3T, the IDE for MongoDB - https://studio3t.com/

);

db.getCollection("repositorySnapshot").aggregate(

	// Pipeline
	[
		// Stage 1
		{
			$match: {
			    "$and" : 
			    [
			         { "TypesAndImplementations.TypeName" : "API" },
			         { "WindowStartsOn": { $lte: "IntervalStartTime" }},
			         { "WindowEndsOn": { $gte: "IntervalEndtime" }}
			    ]
			}
		},

		// Stage 2
		{
			$unwind: {
			    path : "$TypesAndImplementations"
			}
		},

		// Stage 3
		{
			$project: {
			     "TypeAndImplementations" : "$TypesAndImplementations"
			}
		},

		// Stage 4
		{
			$group: {
			     "_id" : { "Implementation" : "$TypeAndImplementations.Implementations.Name" }, "count" : { "$sum" : 1.0 } 
			}
		},

		// Stage 5
		{
			$sort: {
			    _id:1
			}
		},

	]

	// Created with Studio 3T, the IDE for MongoDB - https://studio3t.com/

);

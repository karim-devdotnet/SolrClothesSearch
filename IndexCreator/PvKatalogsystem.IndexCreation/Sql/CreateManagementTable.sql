CREATE TABLE [{{TableName}}]
(
	[ObjectId] INT NOT NULL,
	[DetermineTimestamp] DATETIME NOT NULL,
	[ChunkTimestamp] DATETIME NULL,
	[CommitTimestamp] DATETIME NULL,
	CONSTRAINT [PK_Solr_{{TableName}}] PRIMARY KEY CLUSTERED 
	(
		[ObjectId] ASC
	)
)
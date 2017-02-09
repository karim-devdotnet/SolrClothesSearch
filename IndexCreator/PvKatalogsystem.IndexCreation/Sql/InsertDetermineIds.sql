
TRUNCATE TABLE [{{TableName}}]

INSERT INTO [{{TableName}}] (ObjectId, DetermineTimestamp) 
SELECT Id, CURRENT_TIMESTAMP FROM @ids

-- JobCandidate

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'JobCandidate' AND
            schemas.name = 'HumanResources'
    )
    CREATE TABLE [HumanResources].[JobCandidate]
    (
        [JobCandidateID] INT IDENTITY (1, 1) NOT NULL,
        [BusinessEntityID] INT NULL,
        [Resume] XML NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

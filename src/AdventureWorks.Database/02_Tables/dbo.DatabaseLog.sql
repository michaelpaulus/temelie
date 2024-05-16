-- DatabaseLog

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'DatabaseLog' AND
            schemas.name = 'dbo'
    )
    CREATE TABLE [dbo].[DatabaseLog]
    (
        [DatabaseLogID] INT IDENTITY (1, 1) NOT NULL,
        [PostTime] DATETIME NOT NULL,
        [DatabaseUser] SYSNAME NOT NULL,
        [Event] SYSNAME NOT NULL,
        [Schema] SYSNAME NULL,
        [Object] SYSNAME NULL,
        [TSQL] NVARCHAR(MAX) NOT NULL,
        [XmlEvent] XML NOT NULL
    )
GO

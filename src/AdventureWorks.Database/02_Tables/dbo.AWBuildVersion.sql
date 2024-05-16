-- AWBuildVersion

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'AWBuildVersion' AND
            schemas.name = 'dbo'
    )
    CREATE TABLE [dbo].[AWBuildVersion]
    (
        [SystemInformationID] TINYINT IDENTITY (1, 1) NOT NULL,
        [Database Version] NVARCHAR(25) NOT NULL,
        [VersionDate] DATETIME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

-- ErrorLog

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ErrorLog' AND
            schemas.name = 'dbo'
    )
    CREATE TABLE [dbo].[ErrorLog]
    (
        [ErrorLogID] INT IDENTITY (1, 1) NOT NULL,
        [ErrorTime] DATETIME NOT NULL DEFAULT (GETDATE()),
        [UserName] SYSNAME NOT NULL,
        [ErrorNumber] INT NOT NULL,
        [ErrorSeverity] INT NULL,
        [ErrorState] INT NULL,
        [ErrorProcedure] NVARCHAR(126) NULL,
        [ErrorLine] INT NULL,
        [ErrorMessage] NVARCHAR(4000) NOT NULL
    )
GO

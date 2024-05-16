-- ProductPhoto

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductPhoto' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductPhoto]
    (
        [ProductPhotoID] INT IDENTITY (1, 1) NOT NULL,
        [ThumbNailPhoto] VARBINARY(MAX) NULL,
        [ThumbnailPhotoFileName] NVARCHAR(50) NULL,
        [LargePhoto] VARBINARY(MAX) NULL,
        [LargePhotoFileName] NVARCHAR(50) NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

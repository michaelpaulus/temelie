-- Address

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Address' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[Address]
    (
        [AddressID] INT IDENTITY (1, 1) NOT NULL,
        [AddressLine1] NVARCHAR(60) NOT NULL,
        [AddressLine2] NVARCHAR(60) NULL,
        [City] NVARCHAR(30) NOT NULL,
        [StateProvinceID] INT NOT NULL,
        [PostalCode] NVARCHAR(15) NOT NULL,
        [SpatialLocation] GEOGRAPHY NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE()),
        [CreatedBy] NVARCHAR(250) NOT NULL,
        [ModifiedBy] NVARCHAR(250) NOT NULL
    )
GO

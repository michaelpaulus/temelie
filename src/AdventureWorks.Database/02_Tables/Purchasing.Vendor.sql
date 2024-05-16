-- Vendor

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Vendor' AND
            schemas.name = 'Purchasing'
    )
    CREATE TABLE [Purchasing].[Vendor]
    (
        [BusinessEntityID] INT NOT NULL,
        [AccountNumber] ACCOUNTNUMBER NOT NULL,
        [Name] NAME NOT NULL,
        [CreditRating] TINYINT NOT NULL,
        [PreferredVendorStatus] FLAG NOT NULL DEFAULT (1),
        [ActiveFlag] FLAG NOT NULL DEFAULT (1),
        [PurchasingWebServiceURL] NVARCHAR(1024) NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

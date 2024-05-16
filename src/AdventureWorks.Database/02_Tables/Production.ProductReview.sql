-- ProductReview

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductReview' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductReview]
    (
        [ProductReviewID] INT IDENTITY (1, 1) NOT NULL,
        [ProductID] INT NOT NULL,
        [ReviewerName] NAME NOT NULL,
        [ReviewDate] DATETIME NOT NULL DEFAULT (GETDATE()),
        [EmailAddress] NVARCHAR(50) NOT NULL,
        [Rating] INT NOT NULL,
        [Comments] NVARCHAR(3850) NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

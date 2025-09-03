CREATE VIEW [Production].[vProductAndDescription] AS
SELECT
    p.[ProductID],
    p.[Name],
    pm.[Name] AS [ProductModel],
    pmx.[CultureID],
    pd.[Description]
FROM  
    [Production].[Product] AS p INNER JOIN
    [Production].[ProductModel] AS pm ON
         p.[ProductModelID] = pm.[ProductModelID] INNER JOIN
    [Production].[ProductModelProductDescriptionCulture] AS pmx ON
         pm.[ProductModelID] = pmx.[ProductModelID] INNER JOIN
    [Production].[ProductDescription] AS pd ON
         pmx.[ProductDescriptionID] = pd.[ProductDescriptionID];
GO

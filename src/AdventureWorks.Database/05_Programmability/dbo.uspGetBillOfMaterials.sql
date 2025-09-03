﻿CREATE PROCEDURE [dbo].[uspGetBillOfMaterials]
@StartProductID INT, @CheckDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    WITH   [BOM_cte] ([ProductAssemblyID], [ComponentID], [ComponentDesc], [PerAssemblyQty], [StandardCost], [ListPrice], [BOMLevel], [RecursionLevel])
    AS     (SELECT
                b.[ProductAssemblyID],
                b.[ComponentID],
                p.[Name],
                b.[PerAssemblyQty],
                p.[StandardCost],
                p.[ListPrice],
                b.[BOMLevel],
                0
            FROM  
                [Production].[BillOfMaterials] AS b INNER JOIN
                [Production].[Product] AS p ON
                     b.[ComponentID] = p.[ProductID] WHERE
                                                          b.[ProductAssemblyID] = @StartProductID AND @CheckDate >= b.[StartDate] AND @CheckDate <= ISNULL(b.[EndDate], @CheckDate)
            UNION ALL
            SELECT
                b.[ProductAssemblyID],
                b.[ComponentID],
                p.[Name],
                b.[PerAssemblyQty],
                p.[StandardCost],
                p.[ListPrice],
                b.[BOMLevel],
                [RecursionLevel] + 1
            FROM  
                [BOM_cte] AS cte INNER JOIN
                [Production].[BillOfMaterials] AS b ON
                     b.[ProductAssemblyID] = cte.[ComponentID] INNER JOIN
                [Production].[Product] AS p ON
                     b.[ComponentID] = p.[ProductID] WHERE
                                                          @CheckDate >= b.[StartDate] AND @CheckDate <= ISNULL(b.[EndDate], @CheckDate))
    SELECT
        b.[ProductAssemblyID],
        b.[ComponentID],
        b.[ComponentDesc],
        SUM(b.[PerAssemblyQty]) AS [TotalQuantity],
        b.[StandardCost],
        b.[ListPrice],
        b.[BOMLevel],
        b.[RecursionLevel]
    FROM  
        [BOM_cte] AS b GROUP BY
                            b.[ComponentID], b.[ComponentDesc], b.[ProductAssemblyID], b.[BOMLevel], b.[RecursionLevel], b.[StandardCost], b.[ListPrice] ORDER BY
                                                                                                                                                              b.[BOMLevel], b.[ProductAssemblyID], b.[ComponentID]
    OPTION (MAXRECURSION 25);
END
GO
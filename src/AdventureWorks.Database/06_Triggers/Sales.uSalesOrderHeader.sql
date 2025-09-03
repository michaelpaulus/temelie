CREATE TRIGGER [Sales].[uSalesOrderHeader]
    ON [Sales].[SalesOrderHeader]
    AFTER UPDATE
    NOT FOR REPLICATION
    AS BEGIN
           DECLARE @Count AS INT;
           SET @Count = @@ROWCOUNT;
           IF @Count = 0
               RETURN;
           SET NOCOUNT ON;
           BEGIN TRY
               IF NOT UPDATE ([Status])
                   BEGIN
                       UPDATE [Sales].[SalesOrderHeader]
                       SET    [Sales].[SalesOrderHeader].[RevisionNumber] = [Sales].[SalesOrderHeader].[RevisionNumber] + 1 WHERE
                                                                                                                                 [Sales].[SalesOrderHeader].[SalesOrderID] IN (SELECT
                                                                                                                                                                                   inserted.[SalesOrderID]
                                                                                                                                                                               FROM  
                                                                                                                                                                                   inserted);
                   END
               IF UPDATE ([SubTotal])
                   BEGIN
                       DECLARE @StartDate AS DATETIME, @EndDate AS DATETIME;
                       SET @StartDate = [dbo].[ufnGetAccountingStartDate]();
                       SET @EndDate = [dbo].[ufnGetAccountingEndDate]();
                       UPDATE [Sales].[SalesPerson]
                       SET    [Sales].[SalesPerson].[SalesYTD] = (SELECT
                                                                      SUM([Sales].[SalesOrderHeader].[SubTotal])
                                                                  FROM  
                                                                      [Sales].[SalesOrderHeader] WHERE
                                                                                                      [Sales].[SalesPerson].[BusinessEntityID] = [Sales].[SalesOrderHeader].[SalesPersonID] AND ([Sales].[SalesOrderHeader].[Status] = 5) AND [Sales].[SalesOrderHeader].[OrderDate] BETWEEN @StartDate AND @EndDate) WHERE
                                                                                                                                                                                                                                                                                                                           [Sales].[SalesPerson].[BusinessEntityID] IN (SELECT DISTINCT
                                                                                                                                                                                                                                                                                                                                                                            inserted.[SalesPersonID]
                                                                                                                                                                                                                                                                                                                                                                        FROM  
                                                                                                                                                                                                                                                                                                                                                                            inserted WHERE
                                                                                                                                                                                                                                                                                                                                                                                          inserted.[OrderDate] BETWEEN @StartDate AND @EndDate);
                       UPDATE [Sales].[SalesTerritory]
                       SET    [Sales].[SalesTerritory].[SalesYTD] = (SELECT
                                                                         SUM([Sales].[SalesOrderHeader].[SubTotal])
                                                                     FROM  
                                                                         [Sales].[SalesOrderHeader] WHERE
                                                                                                         [Sales].[SalesTerritory].[TerritoryID] = [Sales].[SalesOrderHeader].[TerritoryID] AND ([Sales].[SalesOrderHeader].[Status] = 5) AND [Sales].[SalesOrderHeader].[OrderDate] BETWEEN @StartDate AND @EndDate) WHERE
                                                                                                                                                                                                                                                                                                                          [Sales].[SalesTerritory].[TerritoryID] IN (SELECT DISTINCT
                                                                                                                                                                                                                                                                                                                                                                         inserted.[TerritoryID]
                                                                                                                                                                                                                                                                                                                                                                     FROM  
                                                                                                                                                                                                                                                                                                                                                                         inserted WHERE
                                                                                                                                                                                                                                                                                                                                                                                       inserted.[OrderDate] BETWEEN @StartDate AND @EndDate);
                   END
           END TRY
           BEGIN CATCH
               EXECUTE [dbo].[uspPrintError] ;
               IF @@TRANCOUNT > 0
                   BEGIN
                       ROLLBACK;
                   END
               EXECUTE [dbo].[uspLogError] ;
           END CATCH
       END
GO
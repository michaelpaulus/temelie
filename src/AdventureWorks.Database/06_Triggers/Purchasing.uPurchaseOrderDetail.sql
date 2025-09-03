CREATE TRIGGER [Purchasing].[uPurchaseOrderDetail]
    ON [Purchasing].[PurchaseOrderDetail]
    AFTER UPDATE
    AS BEGIN
           DECLARE @Count AS INT;
           SET @Count = @@ROWCOUNT;
           IF @Count = 0
               RETURN;
           SET NOCOUNT ON;
           BEGIN TRY
               IF UPDATE ([ProductID]) OR UPDATE ([OrderQty]) OR UPDATE ([UnitPrice])
                   BEGIN
                       INSERT INTO [Production].[TransactionHistory] ([ProductID], [ReferenceOrderID], [ReferenceOrderLineID], [TransactionType], [TransactionDate], [Quantity], [ActualCost])
                       SELECT
                           inserted.[ProductID],
                           inserted.[PurchaseOrderID],
                           inserted.[PurchaseOrderDetailID],
                           'P',
                           GETDATE(),
                           inserted.[OrderQty],
                           inserted.[UnitPrice]
                       FROM  
                           inserted INNER JOIN
                           [Purchasing].[PurchaseOrderDetail] ON
                                inserted.[PurchaseOrderID] = [Purchasing].[PurchaseOrderDetail].[PurchaseOrderID];
                       UPDATE [Purchasing].[PurchaseOrderHeader]
                       SET    [Purchasing].[PurchaseOrderHeader].[SubTotal] = (SELECT
                                                                                   SUM([Purchasing].[PurchaseOrderDetail].[LineTotal])
                                                                               FROM  
                                                                                   [Purchasing].[PurchaseOrderDetail] WHERE
                                                                                                                           [Purchasing].[PurchaseOrderHeader].[PurchaseOrderID] = [Purchasing].[PurchaseOrderDetail].[PurchaseOrderID]) WHERE
                                                                                                                                                                                                                                             [Purchasing].[PurchaseOrderHeader].[PurchaseOrderID] IN (SELECT
                                                                                                                                                                                                                                                                                                          inserted.[PurchaseOrderID]
                                                                                                                                                                                                                                                                                                      FROM  
                                                                                                                                                                                                                                                                                                          inserted);
                       UPDATE [Purchasing].[PurchaseOrderDetail]
                       SET    [Purchasing].[PurchaseOrderDetail].[ModifiedDate] = GETDATE()
                       FROM  
                           inserted WHERE
                                         inserted.[PurchaseOrderID] = [Purchasing].[PurchaseOrderDetail].[PurchaseOrderID] AND inserted.[PurchaseOrderDetailID] = [Purchasing].[PurchaseOrderDetail].[PurchaseOrderDetailID];
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
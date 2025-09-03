CREATE TRIGGER [Production].[iWorkOrder]
    ON [Production].[WorkOrder]
    AFTER INSERT
    AS BEGIN
           DECLARE @Count AS INT;
           SET @Count = @@ROWCOUNT;
           IF @Count = 0
               RETURN;
           SET NOCOUNT ON;
           BEGIN TRY
               INSERT INTO [Production].[TransactionHistory] ([ProductID], [ReferenceOrderID], [TransactionType], [TransactionDate], [Quantity], [ActualCost])
               SELECT
                   inserted.[ProductID],
                   inserted.[WorkOrderID],
                   'W',
                   GETDATE(),
                   inserted.[OrderQty],
                   0
               FROM  
                   inserted;
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
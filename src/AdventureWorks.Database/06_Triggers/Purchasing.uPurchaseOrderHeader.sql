CREATE TRIGGER [Purchasing].[uPurchaseOrderHeader]
    ON [Purchasing].[PurchaseOrderHeader]
    AFTER UPDATE
    AS BEGIN
           DECLARE @Count AS INT;
           SET @Count = @@ROWCOUNT;
           IF @Count = 0
               RETURN;
           SET NOCOUNT ON;
           BEGIN TRY
               IF NOT UPDATE ([Status])
                   BEGIN
                       UPDATE [Purchasing].[PurchaseOrderHeader]
                       SET    [Purchasing].[PurchaseOrderHeader].[RevisionNumber] = [Purchasing].[PurchaseOrderHeader].[RevisionNumber] + 1 WHERE
                                                                                                                                                 [Purchasing].[PurchaseOrderHeader].[PurchaseOrderID] IN (SELECT
                                                                                                                                                                                                              inserted.[PurchaseOrderID]
                                                                                                                                                                                                          FROM  
                                                                                                                                                                                                              inserted);
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
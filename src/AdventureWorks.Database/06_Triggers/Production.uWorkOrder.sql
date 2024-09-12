﻿DROP TRIGGER IF EXISTS [Production].[uWorkOrder]
GO

CREATE TRIGGER [Production].[uWorkOrder]
    ON [Production].[WorkOrder]
    AFTER UPDATE
    AS BEGIN
           DECLARE @Count AS INT;
           SET @Count = @@ROWCOUNT;
           IF @Count = 0
               RETURN;
           SET NOCOUNT ON;
           BEGIN TRY
               IF UPDATE ([ProductID]) OR UPDATE ([OrderQty])
                   BEGIN
                       INSERT INTO [Production].[TransactionHistory] ([ProductID], [ReferenceOrderID], [TransactionType], [TransactionDate], [Quantity])
                       SELECT
                           inserted.[ProductID],
                           inserted.[WorkOrderID],
                           'W',
                           GETDATE(),
                           inserted.[OrderQty]
                       FROM  
                           inserted;
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

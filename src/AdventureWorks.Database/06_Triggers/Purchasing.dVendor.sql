DROP TRIGGER IF EXISTS [Purchasing].[dVendor]
GO

CREATE TRIGGER [Purchasing].[dVendor]
    ON [Purchasing].[Vendor]
    INSTEAD OF DELETE
    NOT FOR REPLICATION
    AS BEGIN
           DECLARE @Count AS INT;
           SET @Count = @@ROWCOUNT;
           IF @Count = 0
               RETURN;
           SET NOCOUNT ON;
           BEGIN TRY
               DECLARE @DeleteCount AS INT;
               SELECT
                   @DeleteCount = COUNT(*)
               FROM  
                   deleted;
               IF @DeleteCount > 0
                   BEGIN
                       RAISERROR (N'Vendors cannot be deleted. They can only be marked as not active.', 10, 1);
                       IF @@TRANCOUNT > 0
                           BEGIN
                               ROLLBACK;
                           END
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

DROP TRIGGER IF EXISTS [HumanResources].[dEmployee]
GO

CREATE TRIGGER [HumanResources].[dEmployee]
    ON [HumanResources].[Employee]
    INSTEAD OF DELETE
    NOT FOR REPLICATION
    AS BEGIN
           DECLARE @Count AS INT;
           SET @Count = @@ROWCOUNT;
           IF @Count = 0
               RETURN;
           SET NOCOUNT ON;
           BEGIN
               RAISERROR (N'Employees cannot be deleted. They can only be marked as not current.', 10, 1);
               IF @@TRANCOUNT > 0
                   BEGIN
                       ROLLBACK;
                   END
           END
       END
GO

﻿-- uspLogError
DROP PROCEDURE IF EXISTS [dbo].[uspLogError]
GO

CREATE PROCEDURE [dbo].[uspLogError]
@ErrorLogID INT=0 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @ErrorLogID = 0;
    BEGIN TRY
        IF ERROR_NUMBER() IS NULL
            RETURN;
        IF XACT_STATE() = -1
            BEGIN
                PRINT 'Cannot log error since the current transaction is in an uncommittable state. ' + 'Rollback the transaction before executing uspLogError in order to successfully log error information.';
                RETURN;
            END
        INSERT  [dbo].[ErrorLog] ([UserName], [ErrorNumber], [ErrorSeverity], [ErrorState], [ErrorProcedure], [ErrorLine], [ErrorMessage])
        VALUES                  (CONVERT (sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE());
        SET @ErrorLogID = @@IDENTITY;
    END TRY
    BEGIN CATCH
        PRINT 'An error occurred in stored procedure uspLogError: ';
        EXECUTE [dbo].[uspPrintError] ;
        RETURN -1;
    END CATCH
END
GO

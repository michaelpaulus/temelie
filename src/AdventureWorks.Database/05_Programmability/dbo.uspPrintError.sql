﻿-- uspPrintError
DROP PROCEDURE IF EXISTS [dbo].[uspPrintError]
GO

CREATE PROCEDURE [dbo].[uspPrintError]
AS
BEGIN
    SET NOCOUNT ON;
    PRINT 'Error ' + CONVERT (VARCHAR (50), ERROR_NUMBER()) + ', Severity ' + CONVERT (VARCHAR (5), ERROR_SEVERITY()) + ', State ' + CONVERT (VARCHAR (5), ERROR_STATE()) + ', Procedure ' + ISNULL(ERROR_PROCEDURE(), '-') + ', Line ' + CONVERT (VARCHAR (5), ERROR_LINE());
    PRINT ERROR_MESSAGE();
END
GO

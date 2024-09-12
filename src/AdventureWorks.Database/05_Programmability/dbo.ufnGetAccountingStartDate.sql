-- ufnGetAccountingStartDate
DROP FUNCTION IF EXISTS [dbo].[ufnGetAccountingStartDate]
GO

CREATE FUNCTION [dbo].[ufnGetAccountingStartDate]
( )
RETURNS DATETIME
AS
BEGIN
    RETURN CONVERT (DATETIME, '20030701', 112);
END
GO

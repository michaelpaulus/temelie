-- ufnGetAccountingEndDate
DROP FUNCTION IF EXISTS [dbo].[ufnGetAccountingEndDate]
GO

CREATE FUNCTION [dbo].[ufnGetAccountingEndDate]
( )
RETURNS DATETIME
AS
BEGIN
    RETURN DATEADD(millisecond, -2, CONVERT (DATETIME, '20040701', 112));
END
GO

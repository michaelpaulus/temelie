-- ufnGetAccountingEndDate
DROP FUNCTION IF EXISTS [dbo].[ufnGetAccountingEndDate]
GO

CREATE FUNCTION [dbo].[ufnGetAccountingEndDate]()
RETURNS [datetime] 
AS 
BEGIN
    RETURN DATEADD(millisecond, -2, CONVERT(datetime, '20040701', 112));
END;
GO

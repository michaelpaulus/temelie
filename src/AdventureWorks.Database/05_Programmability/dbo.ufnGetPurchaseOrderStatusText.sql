-- ufnGetPurchaseOrderStatusText
DROP FUNCTION IF EXISTS [dbo].[ufnGetPurchaseOrderStatusText]
GO

CREATE FUNCTION [dbo].[ufnGetPurchaseOrderStatusText]
(@Status TINYINT)
RETURNS NVARCHAR (15)
AS
BEGIN
    DECLARE @ret AS NVARCHAR (15);
    SET @ret = CASE @Status WHEN 1 THEN 'Pending' WHEN 2 THEN 'Approved' WHEN 3 THEN 'Rejected' WHEN 4 THEN 'Complete' ELSE '** Invalid **' END;
    RETURN @ret;
END
GO

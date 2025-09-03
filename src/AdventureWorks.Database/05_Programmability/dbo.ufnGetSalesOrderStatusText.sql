CREATE FUNCTION [dbo].[ufnGetSalesOrderStatusText]
(@Status TINYINT)
RETURNS NVARCHAR (15)
AS
BEGIN
    DECLARE @ret AS NVARCHAR (15);
    SET @ret = CASE @Status WHEN 1 THEN 'In process' WHEN 2 THEN 'Approved' WHEN 3 THEN 'Backordered' WHEN 4 THEN 'Rejected' WHEN 5 THEN 'Shipped' WHEN 6 THEN 'Cancelled' ELSE '** Invalid **' END;
    RETURN @ret;
END
GO
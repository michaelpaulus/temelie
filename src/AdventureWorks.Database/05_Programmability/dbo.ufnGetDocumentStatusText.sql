-- ufnGetDocumentStatusText
DROP FUNCTION IF EXISTS [dbo].[ufnGetDocumentStatusText]
GO

CREATE FUNCTION [dbo].[ufnGetDocumentStatusText]
(@Status TINYINT)
RETURNS NVARCHAR (16)
AS
BEGIN
    DECLARE @ret AS NVARCHAR (16);
    SET @ret = CASE @Status WHEN 1 THEN N'Pending approval' WHEN 2 THEN N'Approved' WHEN 3 THEN N'Obsolete' ELSE N'** Invalid **' END;
    RETURN @ret;
END
GO

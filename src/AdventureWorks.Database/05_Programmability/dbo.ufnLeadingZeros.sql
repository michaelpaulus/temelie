CREATE FUNCTION [dbo].[ufnLeadingZeros]
(@Value INT)
RETURNS VARCHAR (8)
WITH SCHEMABINDING
AS
BEGIN
    DECLARE @ReturnValue AS VARCHAR (8);
    SET @ReturnValue = CONVERT (VARCHAR (8), @Value);
    SET @ReturnValue = REPLICATE('0', 8 - DATALENGTH(@ReturnValue)) + @ReturnValue;
    RETURN (@ReturnValue);
END
GO
CREATE PROCEDURE [HumanResources].[uspUpdateEmployeePersonalInfo]
@BusinessEntityID INT, @NationalIDNumber NVARCHAR (15), @BirthDate DATETIME, @MaritalStatus NCHAR (1), @Gender NCHAR (1)
WITH EXECUTE AS CALLER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE [HumanResources].[Employee]
        SET    [NationalIDNumber] = @NationalIDNumber,
               [BirthDate] = @BirthDate,
               [MaritalStatus] = @MaritalStatus,
               [Gender] = @Gender WHERE
                                       [BusinessEntityID] = @BusinessEntityID;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspLogError] ;
    END CATCH
END
GO
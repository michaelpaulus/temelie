-- vStateProvinceCountryRegion
DROP VIEW IF EXISTS [Person].[vStateProvinceCountryRegion]
GO

CREATE VIEW [Person].[vStateProvinceCountryRegion] 
WITH SCHEMABINDING 
AS 
SELECT 
    sp.[StateProvinceID] 
    ,sp.[StateProvinceCode] 
    ,sp.[IsOnlyStateProvinceFlag] 
    ,sp.[Name] AS [StateProvinceName] 
    ,sp.[TerritoryID] 
    ,cr.[CountryRegionCode] 
    ,cr.[Name] AS [CountryRegionName]
FROM [Person].[StateProvince] sp 
    INNER JOIN [Person].[CountryRegion] cr 
    ON sp.[CountryRegionCode] = cr.[CountryRegionCode];
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'view', 'vStateProvinceCountryRegion', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'view',
                                     @level1name = 'vStateProvinceCountryRegion';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Joins StateProvince table with CountryRegion table.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'view',
                                @level1name = 'vStateProvinceCountryRegion';
GO


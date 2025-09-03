CREATE VIEW [Person].[vStateProvinceCountryRegion]
WITH SCHEMABINDING AS
SELECT
    sp.[StateProvinceID],
    sp.[StateProvinceCode],
    sp.[IsOnlyStateProvinceFlag],
    sp.[Name] AS [StateProvinceName],
    sp.[TerritoryID],
    cr.[CountryRegionCode],
    cr.[Name] AS [CountryRegionName]
FROM  
    [Person].[StateProvince] AS sp INNER JOIN
    [Person].[CountryRegion] AS cr ON
         sp.[CountryRegionCode] = cr.[CountryRegionCode];
GO
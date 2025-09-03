CREATE PROCEDURE [dbo].[uspSearchCandidateResumes]
@searchString NVARCHAR (1000), @useInflectional BIT=0, @useThesaurus BIT=0, @language INT=0
WITH EXECUTE AS CALLER
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @string AS NVARCHAR (1050);
    IF @language = NULL OR @language = 0
        BEGIN
            SELECT
                @language = CONVERT (INT, serverproperty('lcid'));
        END
    IF @useThesaurus = 1 AND @useInflectional = 1
        BEGIN
            SELECT
                FT_TBL.[JobCandidateID],
                KEY_TBL.[RANK]
            FROM  
                [HumanResources].[JobCandidate] AS FT_TBL INNER JOIN
                FREETEXTTABLE ([HumanResources].[JobCandidate], *, @searchString, LANGUAGE @language) AS KEY_TBL ON
                     FT_TBL.[JobCandidateID] = KEY_TBL.[KEY];
        END
    ELSE
        IF @useThesaurus = 1
            BEGIN
                SELECT
                    @string = 'FORMSOF(THESAURUS,"' + @searchString + '"' + ')';
                SELECT
                    FT_TBL.[JobCandidateID],
                    KEY_TBL.[RANK]
                FROM  
                    [HumanResources].[JobCandidate] AS FT_TBL INNER JOIN
                    CONTAINSTABLE ([HumanResources].[JobCandidate], *, @string, LANGUAGE @language) AS KEY_TBL ON
                         FT_TBL.[JobCandidateID] = KEY_TBL.[KEY];
            END
        ELSE
            IF @useInflectional = 1
                BEGIN
                    SELECT
                        @string = 'FORMSOF(INFLECTIONAL,"' + @searchString + '"' + ')';
                    SELECT
                        FT_TBL.[JobCandidateID],
                        KEY_TBL.[RANK]
                    FROM  
                        [HumanResources].[JobCandidate] AS FT_TBL INNER JOIN
                        CONTAINSTABLE ([HumanResources].[JobCandidate], *, @string, LANGUAGE @language) AS KEY_TBL ON
                             FT_TBL.[JobCandidateID] = KEY_TBL.[KEY];
                END
            ELSE
                BEGIN
                    SELECT
                        @string = '"' + @searchString + '"';
                    SELECT
                        FT_TBL.[JobCandidateID],
                        KEY_TBL.[RANK]
                    FROM  
                        [HumanResources].[JobCandidate] AS FT_TBL INNER JOIN
                        CONTAINSTABLE ([HumanResources].[JobCandidate], *, @string, LANGUAGE @language) AS KEY_TBL ON
                             FT_TBL.[JobCandidateID] = KEY_TBL.[KEY];
                END
END
GO
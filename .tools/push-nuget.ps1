param( 
    [Parameter(Mandatory)]    
    $RepoDirectory,
    [Parameter(Mandatory)]    
    $ApiKey
)

$Files = Get-ChildItem -Path $RepoDirectory -Filter "*.nupkg" -Recurse

foreach ($File in $Files) {
    Write-Host "pushing $($File.Name)"
    &dotnet nuget push $File.FullName -k $ApiKey -s https://api.nuget.org/v3/index.json --no-symbols 
}
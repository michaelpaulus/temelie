param( 
    [Parameter(Mandatory)]    
    $RepoDirectory
)

$Files = Get-ChildItem -Path $RepoDirectory -Filter "*.nupkg" -Recurse

$Directory = "$([System.IO.Path]::Combine($env:USERPROFILE, ".nuget", "local"))"

if (![System.IO.Directory]::Exists($Directory)) {
    [System.IO.Directory]::CreateDirectory($Directory)
}

foreach ($File in $Files) {
    &nuget add $File.FullName -source "$Directory"
}
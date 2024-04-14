param( 
    [Parameter(Mandatory)]    
    $RepoDirectory,
    [Parameter(Mandatory)]    
    $Version,
    [Parameter(Mandatory)]    
    $Hash
)

$FilePath = [System.IO.Path]::Combine($RepoDirectory, "Directory.Build.props")

(Get-Content $FilePath).Replace("<Version>1.0.0.0</Version>", "<Version>$Version+$($Hash.Substring(0, 7))</Version>") | Set-Content $FilePath

(Get-Content $FilePath).Replace("<FileVersion>1.0.0.0</FileVersion>", "<FileVersion>$Version</FileVersion>") | Set-Content $FilePath
param(
    [Parameter(Mandatory=$true)][string]$AssetZip,
    [switch]$Clean
)
$script = Join-Path $PSScriptRoot 'import-post-apocalypse-assets.py'
$argsList = @($script, $AssetZip)
if ($Clean) { $argsList += '--clean' }
python @argsList

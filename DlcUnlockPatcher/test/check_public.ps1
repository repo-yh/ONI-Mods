param([string]$Dll)

if (-not $Dll) {
    $Dll = Join-Path $PSScriptRoot '..\..\lib\Assembly-CSharp-firstpass_public.dll'
}
$Dll = (Resolve-Path $Dll).Path

$libDir = Split-Path $Dll
$sharedDirs = @(Get-ChildItem 'C:\Program Files\dotnet\shared\Microsoft.NETCore.App' -Directory -ErrorAction SilentlyContinue | Sort-Object Name -Descending)
$null = [AppDomain]::CurrentDomain.add_ReflectionOnlyAssemblyResolve({
    param($s, $e)
    $name = (New-Object System.Reflection.AssemblyName($e.Name)).Name
    $p = Join-Path $libDir "$name.dll"
    if (Test-Path $p) { return [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($p) }
    foreach ($d in $sharedDirs) {
        $p2 = Join-Path $d.FullName "$name.dll"
        if (Test-Path $p2) { return [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($p2) }
    }
    return $null
})

$a = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($Dll)
Write-Host "AssemblyName = $($a.GetName().Name)"
$t = $a.GetType('DlcManager')
if (-not $t) { Write-Host 'DlcManager NOT FOUND in this dll'; exit 1 }

$bf = [System.Reflection.BindingFlags]'NonPublic,Public,Static,Instance'
foreach ($n in 'CheckForDLCFileInstallation','IsContentSettingEnabled') {
    $m = $t.GetMethod($n, $bf)
    if ($m) { Write-Host "$n : IsPublic=$($m.IsPublic)" } else { Write-Host "$n : method not found" }
}
foreach ($n in 'dlcSubscribedCache','dlcPurchasedCache') {
    $f = $t.GetField($n, $bf)
    if ($f) { Write-Host "$n : IsPublic=$($f.IsPublic)" } else { Write-Host "$n : field not found" }
}

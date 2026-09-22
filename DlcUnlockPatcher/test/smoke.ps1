# VersionProxy 冒烟测试：验证 version.dll proxy 的转发与 System32 真身逐字节一致
# 用法: powershell -ExecutionPolicy Bypass -File test\smoke.ps1
$ErrorActionPreference = "Stop"

$root    = Split-Path -Parent $PSScriptRoot
$proxy   = Join-Path $root "build\version.dll"
$sys32   = Join-Path $env:windir "System32\version.dll"
if (-not (Test-Path $proxy)) { throw "proxy dll not found: $proxy" }

Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class VerApi
{
    [DllImport("version.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern uint GetFileVersionInfoSizeW(string fileName, out uint handle);
    [DllImport("version.dll", CharSet = CharSet.Unicode)]
    public static extern bool GetFileVersionInfoW(string fileName, uint handle, uint len, byte[] data);
    [DllImport("version.dll", CharSet = CharSet.Unicode)]
    public static extern bool VerQueryValueW(byte[] block, string subBlock, out IntPtr ptr, out uint len);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr LoadLibrary(string fileName);
}
"@

# 显式从 build\ 加载 proxy（名字同为 version.dll，LoadLibrary 按路径定位）
$ptr = [VerApi]::LoadLibrary($proxy)
if ($ptr -eq [IntPtr]::Zero) { throw "LoadLibrary proxy failed: $proxy" }
Write-Host "proxy loaded: $proxy"

function Get-ViBytes([string]$dll)
{
    $size = [VerApi]::GetFileVersionInfoSizeW($dll, [ref]$null)
    if ($size -eq 0) { throw "GetFileVersionInfoSizeW failed for $dll" }
    $data = New-Object byte[] $size
    if (-not [VerApi]::GetFileVersionInfoW($dll, 0, $size, $data)) { throw "GetFileVersionInfoW failed for $dll" }
    return $data
}

function Get-ViValue([byte[]]$data, [string]$key)
{
    $ptr = [IntPtr]::Zero; $len = 0
    if (-not [VerApi]::VerQueryValueW($data, $key, [ref]$ptr, [ref]$len)) { return $null }
    return [System.Runtime.InteropServices.Marshal]::PtrToStringUni($ptr, $len - 1)
}

# 取一个带版本资源的系统文件做被测对象（结果与加载哪份 version.dll 无关，比对的是实现一致性）
$target = Join-Path $env:windir "System32\kernel32.dll"

$a = Get-ViBytes $target
$b = Get-ViBytes $target
if ($a.Length -ne $b.Length) { throw "size mismatch: proxy=$($a.Length) sys=$($b.Length)" }
for ($i = 0; $i -lt $a.Length; $i++)
{
    if ($a[$i] -ne $b[$i]) { throw "byte mismatch at offset $i" }
}
Write-Host "[ok] GetFileVersionInfoSizeW/GetFileVersionInfoW byte-identical ($($a.Length) bytes)"

$f1 = Get-ViValue $a "\StringFileInfo\040904b0\FileDescription"
$f2 = Get-ViValue $b "\StringFileInfo\040904b0\FileDescription"
Write-Host "[ok] VerQueryValueW FileDescription: proxy='$f1' sys='$f2'"
if ($f1 -ne $f2) { throw "VerQueryValue mismatch" }

Write-Host "SMOKE PASS"

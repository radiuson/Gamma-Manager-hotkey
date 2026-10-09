# Builds dist\Gamma Manager.exe without Visual Studio.
# Step 1: .NET Framework MSBuild generates the .resources files (it fails at compile, which is expected: old csc is C# 5).
# Step 2: Roslyn from the .NET SDK compiles against the .NET Framework 4 assemblies.
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$proj = Join-Path $root 'Gamma Manager'
$fw   = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
$tmp  = Join-Path $env:TEMP 'gamma-manager-build'
$obj  = Join-Path $tmp 'obj\x64\Release'
$dist = Join-Path $root 'dist'

$sdk = (& dotnet --list-sdks | Select-Object -Last 1) -replace '^(\S+) \[(.+)\]$', '$2\$1'
$csc = Join-Path $sdk 'Roslyn\bincore\csc.dll'

& "$fw\MSBuild.exe" "$proj\Gamma Manager.csproj" /t:PrepareResources /p:Configuration=Release /p:Platform=x64 `
    /p:FrameworkPathOverride=$fw "/p:OutputPath=$tmp\" "/p:BaseIntermediateOutputPath=$tmp\obj\" /v:q /nologo
if ($LASTEXITCODE -ne 0) { throw 'Resource generation failed' }

New-Item -ItemType Directory -Force $dist | Out-Null
$src  = Get-ChildItem $proj -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\(obj|bin)\\' } | ForEach-Object FullName
$refs = 'mscorlib','System','System.Core','System.Configuration','System.Drawing','System.Management',
        'System.Web.Extensions','System.Windows.Forms','System.Xml' | ForEach-Object { "/r:$fw\$_.dll" }

& dotnet $csc /nologo /noconfig /nostdlib+ /target:winexe /platform:x64 /optimize+ /langversion:7.3 /define:TRACE `
    "/win32icon:$proj\TrayIcon.ico" "/win32manifest:$proj\Properties\app.manifest" "/out:$dist\Gamma Manager.exe" `
    "/resource:$obj\Gamma_Manager.Properties.Resources.resources" "/resource:$obj\Gamma_Manager.Window.resources" `
    @refs @src
if ($LASTEXITCODE -ne 0) { throw 'Compile failed' }

Copy-Item "$proj\App.config" "$dist\Gamma Manager.exe.config"
Write-Host "Built $dist\Gamma Manager.exe"

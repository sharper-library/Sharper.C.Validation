$TestsRegex = '\.Tests$'

function AllProjects() {
    Get-ChildItem */project.json
}

function PackageProjects() {
    AllProjects | Where {$_.Directory.Name -notmatch $TestsRegex}
}

function TestProjects() {
    AllProjects | Where {$_.Directory.Name -match $TestsRegex}
}

function GlobalSdk($path) {
    (ConvertFrom-Json ((Get-Content $path) -join "`n")).sdk
}

function CleanCmd() {
    AllProjects | %{$_.Directory} | %{
        if (Test-Path $_/bin) {Remove-Item -Recurse $_/bin}
        if (Test-Path $_/obj) {Remove-Item -Recurse $_/obj}
    }
    if (Test-Path artifacts) {Remove-Item -Recurse artifacts}
}

function RestoreCmd() {
    dnu restore
}

function InstallCmd() {
    nuget sources add -Name Sharper.C -Source $env:SHARPER_C_FEED
    $sdk = GlobalSdk 'global.json'
    dnvm install $sdk.version -r $sdk.runtime -arch $sdk.architecture
    dnu restore
}

function BuildCmd() {
    if ($env:APPVEYOR_BUILD_NUMBER) {
      $env:DNX_BUILD_VERSION =
          'build-{0}' -f (([string]$env:APPVEYOR_BUILD_NUMBER).PadLeft(5, '0'))
    }
    else {
      $env:DNX_BUILD_VERSION = 'z'
    }
    dnu pack --configuration Release (PackageProjects)
}

function TestCmd() {
    $codes = (TestProjects) | %{dnx -p $_ test | Write-Host; $LASTEXITCODE}
    $code = ($codes | Measure-Object -Sum).Sum
    exit $code
}

function RegisterCmd() {
    PackageProjects | %{
        Get-ChildItem -Recurse *.nupkg | %{dnu packages add $_}
    }
}

function RunCommand($name) {
    switch ($name) {
        clean {CleanCmd}
        restore {RestoreCmd}
        install {InstallCmd}
        build {BuildCmd}
        test {TestCmd}
        register {RegisterCmd}
    }
}

$args | %{RunCommand $_}

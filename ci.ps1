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

function EnsureDnvm() {
    $sdk = GlobalSdk 'global.json'
    dnvm install -Alias ci_build $sdk.version -r $sdk.runtime -arch $sdk.architecture
}

function InstallCmd() {
    dnvm exec ci_build dnu restore
}

function BuildCmd() {
    Write-Host "Building projects:"
    PackageProjects | %{Write-Host "   $_"}
    if ($env:BUILD_BUILDNUMBER) {
      $env:DNX_BUILD_VERSION = $env:BUILD_BUILDNUMBER
    }
    else {
      $env:DNX_BUILD_VERSION = 'z'
    }
    PackageProjects | %{
      dnvm exec ci_build dnu pack --configuration Release $_.Directory
    }
}

function TestCmd() {
    $codes = (TestProjects) | %{dnx -p $_ test | Write-Host; $LASTEXITCODE}
    $code = ($codes | Measure-Object -Sum).Sum
    exit $code
}

function RegisterCmd() {
    PackageProjects | %{
        Get-ChildItem -Recurse *.nupkg | %{dnvm exec ci_build dnu packages add $_}
    }
}

function RunCommand($name) {
    EnsureDnvm
    switch ($name) {
        clean {CleanCmd}
        install {InstallCmd}
        build {BuildCmd}
        test {TestCmd}
        register {RegisterCmd}
        all {CleanCmd; RestoreCmd; BuildCmd; RegisterCmd}
    }
}

$args | %{RunCommand $_}

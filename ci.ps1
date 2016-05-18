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
    $sdk = GlobalSdk 'global.json'
    dnvm install $sdk.version -r $sdk.runtime -arch $sdk.architecture
    dnu restore
}

function BuildCmd() {
    if ($env:BUILD_BUILDNUMBER) {
      $env:DNX_BUILD_VERSION = $env:BUILD_BUILDNUMBER
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

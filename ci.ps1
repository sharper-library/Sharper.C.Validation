Param([string]$phase)

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

function InstallPhase() {
    dnvm install latest
    dnu restore
}

function BuildPhase() {
    dnu pack --configuration Release (PackageProjects)
}

function TestPhase() {
    $codes = (TestProjects) | %{dnx $_ test | Write-Host; $LASTEXITCODE}
    $code = ($codes | Measure-Object -Sum).Sum
    exit $code
}

switch ($phase) {
    install {InstallPhase}
    build {BuildPhase}
    test {TestPhase}
}

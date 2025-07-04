# restore and builds all projects as release.
# creates NuGet package at \artifacts
dotnet --version

$versionPrefix = "6.0.2"
$versionSuffix = ""
$versionFile = $versionPrefix + "." + ${env:APPVEYOR_BUILD_NUMBER}
if ($env:APPVEYOR_PULL_REQUEST_NUMBER) {
    $versionPrefix = $versionFile
    $versionSuffix = "PR" + $env:APPVEYOR_PULL_REQUEST_NUMBER
}

msbuild /t:restore,pack /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /maxcpucount /p:PackageOutputPath=..\artifacts /p:VersionPrefix=$versionPrefix /p:VersionSuffix=$versionSuffix /p:FileVersion=$versionFile /p:ContinuousIntegrationBuild=true /p:EmbedUntrackedSources=true /verbosity:minimal
if (-Not $LastExitCode -eq 0) {
    exit $LastExitCode 
}

exit $LastExitCode

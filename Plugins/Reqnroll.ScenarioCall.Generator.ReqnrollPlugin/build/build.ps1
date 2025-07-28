param($version, $configuration)

# This build script is used by the NuGet package generation to copy the plugin DLL to the correct location
# The NuGet package will include the plugin DLL in the tools/generator folder
Write-Host "Building Reqnroll ScenarioCall Generator Plugin version $version in $configuration configuration"

# The actual build is handled by MSBuild, this script is mainly for NuGet packaging support
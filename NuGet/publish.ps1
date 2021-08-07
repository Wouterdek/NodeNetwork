$version = "6.0.0"

if (![System.IO.File]::Exists("nuget.exe")) {
    Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile "nuget.exe"
}

Write-Output ("Push NodeNetwork " + $version + " to NuGet? (Didn't forget version bump?)")
Read-Host
./nuget.exe push ("../NodeNetwork/bin/Release/NodeNetwork." + $version + ".nupkg") $args[0] -Source https://api.nuget.org/v3/index.json
./nuget.exe push ("../NodeNetworkToolkit/bin/Release/NodeNetworkToolkit." + $version + ".nupkg") $args[0] -Source https://api.nuget.org/v3/index.json
./nuget.exe push ("../NodeNetwork/bin/Release/NodeNetwork." + $version + ".symbols.nupkg") $args[0] -source https://nuget.smbsrc.net/
./nuget.exe push ("../NodeNetworkToolkit/bin/Release/NodeNetworkToolkit." + $version + ".symbols.nupkg") $args[0] -source https://nuget.smbsrc.net/

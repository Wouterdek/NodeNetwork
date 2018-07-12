@echo off
echo Push NodeNetwork to NuGet? (Didn't forget version bump?)
pause
nuget push NodeNetwork.3.0.1.nupkg %1 -Source https://api.nuget.org/v3/index.json
nuget push NodeNetworkToolkit.3.0.1.nupkg %1 -Source https://api.nuget.org/v3/index.json
nuget push NodeNetwork.3.0.1.symbols.nupkg %1 -source https://nuget.smbsrc.net/
nuget push NodeNetworkToolkit.3.0.1.symbols.nupkg %1 -source https://nuget.smbsrc.net/
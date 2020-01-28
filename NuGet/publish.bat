@echo off
echo Push NodeNetwork to NuGet? (Didn't forget version bump?)
pause
nuget push ../NodeNetwork/bin/Release/NodeNetwork.4.1.2.nupkg %1 -Source https://api.nuget.org/v3/index.json
nuget push ../NodeNetworkToolkit/bin/Release/NodeNetworkToolkit.4.1.2.nupkg %1 -Source https://api.nuget.org/v3/index.json
nuget push ../NodeNetwork/bin/Release/NodeNetwork.4.1.2.symbols.nupkg %1 -source https://nuget.smbsrc.net/
nuget push ../NodeNetworkToolkit/bin/Release/NodeNetworkToolkit.4.1.2.symbols.nupkg %1 -source https://nuget.smbsrc.net/
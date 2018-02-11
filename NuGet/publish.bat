@echo off
echo Push NodeNetwork to NuGet? (Didn't forget version bump?)
pause
nuget push NodeNetwork.2.0.0.nupkg %1 -Source https://api.nuget.org/v3/index.json
nuget push NodeNetworkToolkit.2.0.0.nupkg %1 -Source https://api.nuget.org/v3/index.json
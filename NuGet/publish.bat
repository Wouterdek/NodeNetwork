@echo off
echo Push NodeNetwork to NuGet?
pause
nuget push NodeNetwork.1.0.0.nupkg %1 -Source https://api.nuget.org/v3/index.json
nuget push NodeNetworkToolkit.1.0.0.nupkg %1 -Source https://api.nuget.org/v3/index.json
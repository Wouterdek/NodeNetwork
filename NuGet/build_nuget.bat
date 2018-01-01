@echo off
echo Building NodeNetwork NuGet package...
nuget.exe pack ../NodeNetwork/NodeNetwork.csproj -IncludeReferencedProjects -properties Configuration=Release

echo.
echo Building NodeNetworkToolkit NuGet package...
nuget.exe pack ../NodeNetworkToolkit/NodeNetworkToolkit.csproj -IncludeReferencedProjects -properties Configuration=Release
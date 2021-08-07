# DocFX executable download URL
$url = "https://github.com/dotnet/docfx/releases/download/v2.58/docfx.zip"

# Check if docfx is already generated, otherwise exit
if (!(Test-Path .\Documentation\docfx.json)) {
    Write-Error "docfx.json was not found. Are you sure you are running this script from the root directory and have pulled the right files?"
    [void](Read-Host 'Press Enter to exit...')
    exit
}

# Download tools
if (!(Test-Path ".\Documentation\Tools\docfx\docfx.exe")) {
    Write-Output "Downloading necessary tools..."

	$outputdir = ".\Documentation\Tools\"
	$output = ".\Documentation\Tools\docfx.zip"

	if (!(Test-Path $outputdir)) {
	    New-Item -ItemType Directory -Force -Path $outputdir
	}
	if (!(Test-Path $output)) {
	    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
	    Invoke-WebRequest -Uri $url -OutFile $output
	}

	# Unzip tools
	Write-Output "" "Unpacking tools..."

	Add-Type -AssemblyName System.IO.Compression.FileSystem
	$zipfile = $output
	$output = ".\Documentation\Tools\docfx\"

	Expand-Archive -Force $zipfile -DestinationPath $output
}


# Set an alias for the tool
Set-Alias -Name docfx -Value "$((Get-Item -Path ./).FullName)\Documentation\Tools\docfx\docfx.exe"


# Initialize docfx
# Write-Output "--------------------------" "" "Initializing docfx framework..." ""
# docfx init -q -o Documentation


# Force build the metadata
Write-Output "--------------------------" "" "Force building the metadata..." ""
docfx metadata .\Documentation\docfx.json -f


# Force build the documentation
Write-Output "--------------------------" "" "Force building the documentation..." ""
docfx build .\Documentation\docfx.json -f


# Serve the documentation if wanted by the user
Write-Output "--------------------------"

$choices = New-Object Collections.ObjectModel.Collection[Management.Automation.Host.ChoiceDescription]
$choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&Yes'))
$choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&No'))

$answer = $Host.UI.PromptForChoice("The documentation can be served on http://localhost:8080.", "Do you want this to happen?", $choices, 1)
if ($answer -eq 0) {
    docfx serve .\Documentation\_site
}

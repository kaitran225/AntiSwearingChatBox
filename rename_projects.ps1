# Get the solution name from the .sln file
$solutionName = "AntiSwearingChatBox"

# Define the new project names
$newProjectNames = @{
    "Anti-Swearing_Chat_Box.AI" = "$solutionName.AI"
    "Anti-Swearing_Chat_Box.Presentation" = "$solutionName.Presentation"
    "Anti-Swearing_Chat_Box.Core" = "$solutionName.Core"
    "Anti-Swearing_Chat_Box.ConsoleTest" = "$solutionName.ConsoleTest"
    "Anti-Swearing_Chat_Box.Services" = "$solutionName.Services"
    "Anti-Swearing_Chat_Box.Repositories" = "$solutionName.Repositories"
}

# Function to rename a directory
function Rename-ProjectDirectory {
    param (
        [string]$oldName,
        [string]$newName
    )
    
    if (Test-Path $oldName) {
        Write-Host "Renaming $oldName to $newName..." -ForegroundColor Yellow
        Rename-Item -Path $oldName -NewName $newName -Force
    } else {
        Write-Host "Directory $oldName not found." -ForegroundColor Red
    }
}

# Function to update project file content
function Update-ProjectFile {
    param (
        [string]$projectPath,
        [string]$oldName,
        [string]$newName
    )
    
    if (Test-Path $projectPath) {
        Write-Host "Updating project file: $projectPath" -ForegroundColor Yellow
        $content = Get-Content $projectPath -Raw
        $content = $content.Replace($oldName, $newName)
        Set-Content -Path $projectPath -Value $content
    }
}

# Function to update solution file content
function Update-SolutionFile {
    param (
        [string]$solutionPath,
        [string]$oldName,
        [string]$newName
    )
    
    if (Test-Path $solutionPath) {
        Write-Host "Updating solution file..." -ForegroundColor Yellow
        $content = Get-Content $solutionPath -Raw
        $content = $content.Replace($oldName, $newName)
        Set-Content -Path $solutionPath -Value $content
    }
}

# Main process
Write-Host "Starting project renaming process..." -ForegroundColor Green

# Rename directories and update files
foreach ($oldName in $newProjectNames.Keys) {
    $newName = $newProjectNames[$oldName]
    
    # Rename directory
    Rename-ProjectDirectory -oldName $oldName -newName $newName
    
    # Update project file
    $projectFile = Join-Path $newName "$newName.csproj"
    Update-ProjectFile -projectPath $projectFile -oldName $oldName -newName $newName
    
    # Update solution file
    Update-SolutionFile -solutionPath "AntiSwearingChatBox.sln" -oldName $oldName -newName $newName
}

Write-Host "`nProject renaming completed!" -ForegroundColor Green
Write-Host "Please rebuild the solution to ensure everything is working correctly." -ForegroundColor Yellow 
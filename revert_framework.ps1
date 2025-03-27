# Project directories
$projects = @(
    "AntiSwearingChatBox.Core",
    "AntiSwearingChatBox.Services",
    "AntiSwearingChatBox.Presentation",
    "AntiSwearingChatBox.Repositories",
    "AntiSwearingChatBox.AI",
    "AntiSwearingChatBox.ConsoleTest"
)

foreach ($project in $projects) {
    $projectFile = Join-Path $project "$project.csproj"
    
    if (Test-Path $projectFile) {
        Write-Host "Processing $projectFile..."
        
        # Read the content
        $content = Get-Content -Path $projectFile -Raw
        
        # Revert TargetFramework back to net8.0
        $content = $content -replace '<TargetFramework>net9\.0</TargetFramework>', '<TargetFramework>net8.0</TargetFramework>'
        
        # Revert package versions back to 8.0.2
        $content = $content -replace 'Version="9\.0\.0"', 'Version="8.0.2"'
        $content = $content -replace '<Version>9\.0\.0</Version>', '<Version>8.0.2</Version>'
        
        # Save the changes
        Set-Content -Path $projectFile -Value $content
        Write-Host "Updated $projectFile"
    } else {
        Write-Host "Project file not found: $projectFile"
    }
}

Write-Host "`nDone! Please rebuild the solution in Visual Studio." 
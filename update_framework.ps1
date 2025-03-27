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
        
        # Update TargetFramework
        $content = $content -replace '<TargetFramework>net\d+\.\d+</TargetFramework>', '<TargetFramework>net9.0</TargetFramework>'
        
        # Update package versions
        $content = $content -replace 'Version="\d+\.\d+\.\d+"', 'Version="9.0.0"'
        
        # Save the changes
        Set-Content -Path $projectFile -Value $content
        Write-Host "Updated $projectFile"
    } else {
        Write-Host "Project file not found: $projectFile"
    }
}

Write-Host "`nDone! Please rebuild the solution in Visual Studio." 
# Project directories and their new names
$projects = @(
    "AntiSwearingChatBox.Core",
    "AntiSwearingChatBox.Services",
    "AntiSwearingChatBox.Presentation",
    "AntiSwearingChatBox.Repositories",
    "AntiSwearingChatBox.AI",
    "AntiSwearingChatBox.ConsoleTest"
)

foreach ($project in $projects) {
    $projectDir = $project
    $oldCsproj = Get-ChildItem -Path $projectDir -Filter "*.csproj" | Select-Object -First 1
    
    if ($oldCsproj) {
        $newCsprojName = "$project.csproj"
        $oldCsprojPath = Join-Path $projectDir $oldCsproj.Name
        $newCsprojPath = Join-Path $projectDir $newCsprojName
        
        Write-Host "Processing $projectDir..."
        Write-Host "  Old csproj: $($oldCsproj.Name)"
        Write-Host "  New csproj: $newCsprojName"
        
        # Rename the .csproj file
        if ($oldCsproj.Name -ne $newCsprojName) {
            Write-Host "  Renaming project file..."
            Rename-Item -Path $oldCsprojPath -NewName $newCsprojName -Force
        }
        
        # Update the content of the .csproj file
        Write-Host "  Updating project file content..."
        $content = Get-Content -Path $newCsprojPath -Raw
        $content = $content -replace "Anti-Swearing_Chat_Box", "AntiSwearingChatBox"
        Set-Content -Path $newCsprojPath -Value $content
        
        # Update any AssemblyName and RootNamespace if they exist
        $xml = [xml](Get-Content $newCsprojPath)
        $propertyGroup = $xml.Project.PropertyGroup[0]
        
        if ($propertyGroup) {
            $assemblyName = $xml.CreateElement("AssemblyName")
            $assemblyName.InnerText = $project
            $rootNamespace = $xml.CreateElement("RootNamespace")
            $rootNamespace.InnerText = $project
            
            $propertyGroup.AppendChild($assemblyName) | Out-Null
            $propertyGroup.AppendChild($rootNamespace) | Out-Null
            
            $xml.Save($newCsprojPath)
        }
    } else {
        Write-Host "No .csproj file found in $projectDir"
    }
}

# Now update the solution file
Write-Host "`nUpdating solution file..."
$solutionPath = "AntiSwearingChatBox.sln"
$solutionContent = Get-Content -Path $solutionPath -Raw

# Update project references in the solution file
$solutionContent = $solutionContent -replace "Anti-Swearing_Chat_Box", "AntiSwearingChatBox"
Set-Content -Path $solutionPath -Value $solutionContent

Write-Host "`nDone! Please rebuild the solution in Visual Studio." 
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
    $projectFile = Join-Path $project "$project.csproj"
    
    if (Test-Path $projectFile) {
        Write-Host "Processing $projectFile..."
        
        # Load the project file as XML
        $xml = [xml](Get-Content $projectFile)
        
        # Find the PropertyGroup node
        $propertyGroup = $xml.Project.PropertyGroup | Where-Object { $_.RootNamespace -or $_.AssemblyName }
        
        if (-not $propertyGroup) {
            $propertyGroup = $xml.Project.PropertyGroup[0]
        }
        
        # Update or add RootNamespace
        $rootNamespace = $propertyGroup.RootNamespace
        if ($rootNamespace) {
            $propertyGroup.RootNamespace = $project
        } else {
            $newElement = $xml.CreateElement("RootNamespace")
            $newElement.InnerText = $project
            $propertyGroup.AppendChild($newElement) | Out-Null
        }
        
        # Update or add AssemblyName
        $assemblyName = $propertyGroup.AssemblyName
        if ($assemblyName) {
            $propertyGroup.AssemblyName = $project
        } else {
            $newElement = $xml.CreateElement("AssemblyName")
            $newElement.InnerText = $project
            $propertyGroup.AppendChild($newElement) | Out-Null
        }
        
        # Save the changes
        $xml.Save($projectFile)
        Write-Host "Updated $projectFile"
    } else {
        Write-Host "Project file not found: $projectFile"
    }
}

Write-Host "`nDone! Please rebuild the solution in Visual Studio." 
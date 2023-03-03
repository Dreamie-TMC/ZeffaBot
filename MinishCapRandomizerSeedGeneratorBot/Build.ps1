$outputPath = Read-Host "Please enter the path to output zeffa to: "

dotnet publish .\MinishCapRandomizerSeedGeneratorBot.csproj -c Release -o $outputPath\
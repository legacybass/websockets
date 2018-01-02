@echo off

echo Building WebSockets

dotnet build --configuration Release ..\WebSockets
REM dotnet pack ..\src\WebSockets --output ..\Releases\ --configuration Release

echo Building WebSockets for .NET Framework

nuget pack ..\WebSockets\WebSockets.nuspec -OutputDirectory ..\Releases\ -Build -Properties Configuration=Release




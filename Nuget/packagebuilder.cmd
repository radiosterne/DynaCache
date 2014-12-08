pushd "%~dp0"
Nuget pack PackageContents\DynaCache.nuspec -OutputDirectory ..\output
popd
REM nuget pack Transformalize.Provider.SqlServer.nuspec -OutputDirectory "c:\temp\modules"
REM nuget pack Transformalize.Provider.SqlServer.Autofac.nuspec -OutputDirectory "c:\temp\modules"

nuget push "c:\temp\modules\Transformalize.Provider.SqlServer.0.8.1-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
nuget push "c:\temp\modules\Transformalize.Provider.SqlServer.Autofac.0.8.1-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json







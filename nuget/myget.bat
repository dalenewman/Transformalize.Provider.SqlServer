nuget pack Transformalize.Provider.SqlServer.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Provider.SqlServer.Autofac.nuspec -OutputDirectory "c:\temp\modules"

REM nuget push "c:\temp\modules\Transformalize.Provider.SqlServer.0.7.8-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
REM nuget push "c:\temp\modules\Transformalize.Provider.SqlServer.Autofac.0.7.8-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json






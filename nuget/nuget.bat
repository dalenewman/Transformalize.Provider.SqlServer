nuget pack Transformalize.Provider.SqlServer.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Provider.SqlServer.Autofac.nuspec -OutputDirectory "c:\temp\modules"

nuget push "c:\temp\modules\Transformalize.Provider.SqlServer.0.3.4-beta.nupkg" -source https://api.nuget.org/v3/index.json
nuget push "c:\temp\modules\Transformalize.Provider.SqlServer.Autofac.0.3.4-beta.nupkg" -source https://api.nuget.org/v3/index.json







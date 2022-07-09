nuget pack Transformalize.Provider.SqlServer.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Provider.SqlServer.Autofac.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Provider.SqlServer.SqlClient.1.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Provider.SqlServer.SqlClient.1.Autofac.nuspec -OutputDirectory "c:\temp\modules"

REM nuget push "c:\temp\modules\Transformalize.Provider.SqlServer.0.10.1-beta.nupkg" -source https://api.nuget.org/v3/index.json
REM nuget push "c:\temp\modules\Transformalize.Provider.SqlServer.Autofac.0.10.1-beta.nupkg" -source https://api.nuget.org/v3/index.json
REM nuget push "c:\temp\modules\Transformalize.Provider.SqlServer.SqlClient.1.0.10.1-beta.nupkg" -source https://api.nuget.org/v3/index.json
REM nuget push "c:\temp\modules\Transformalize.Provider.SqlServer.SqlClient.1.Autofac.0.10.1-beta.nupkg" -source https://api.nuget.org/v3/index.json

#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;
using System.Linq;
using Autofac;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PoorMansTSqlFormatterLib;
using Transformalize.Configuration;
using Transformalize.Containers.Autofac;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Providers.Ado.Ext;
using Transformalize.Providers.Console;
using Transformalize.Providers.SqlServer;
using Transformalize.Providers.SqlServer.Autofac;
using Transformalize.Transforms.CSharp.Autofac;

namespace IntegrationTests {

    [TestClass]
    public class SlaveGetsInserts {

        const string xml = @"
<cfg name='Test' mode='@(Mode)' flatten='true'>
  <parameters>
    <add name='Mode' value='default' />
  </parameters>
  <connections>
    <add name='input' provider='sqlserver' database='TestInput' />
    <add name='output' provider='sqlserver' database='TestOutput' />
  </connections>
  <entities>
    <add name='MasterTable'>
        <fields>
            <add name='Id' type='int' primary-key='true' />
            <add name='d1' />
            <add name='d2' />
        </fields>
    </add>
    <add name='SlaveTable'>
        <fields>
            <add name='Id' type='int' primary-key='true' />
            <add name='d3' />
            <add name='d4' />
        </fields>
    </add>
  </entities>
  <relationships>
    <add left-entity='MasterTable' left-field='Id' right-entity='SlaveTable' right-field='Id' />
  </relationships>
</cfg>
";

        public Connection InputConnection { get; set; } = new Connection {
            Name = "input",
            Provider = "sqlserver",
            ConnectionString = "server=localhost;database=NorthWind;trusted_connection=true;"
        };

        public Connection OutputConnection { get; set; } = new Connection {
            Name = "output",
            Provider = "sqlserver",
            ConnectionString = "Server=localhost;Database=TflNorthWind;trusted_connection=true;"
        };

        [TestMethod]
        public void SlaveGetsInserts_Integration() {

            // SETUP 
            using (var cn = new SqlServerConnectionFactory(InputConnection).GetConnection()) {
                cn.Open();
                Assert.AreEqual(3, cn.Execute(@"
                    IF OBJECT_ID('MasterTable') IS NULL
	BEGIN
		create table MasterTable(
			Id int not null primary key,
			d1 nvarchar(64) not null,
			d2 nvarchar(64) not null
		);
END

IF OBJECT_ID('SlaveTable') IS NULL
	BEGIN
		create table SlaveTable(
			Id int not null primary key,
			d3 nvarchar(64) not null,
			d4 nvarchar(64) not null
		);
	END

TRUNCATE TABLE MasterTable;
TRUNCATE TABLE SlaveTable;

INSERT INTO MasterTable(Id,d1,d2)VALUES(1,'d1','d2');
INSERT INTO MasterTable(Id,d1,d2)VALUES(2,'d3','d4');

INSERT INTO SlaveTable(Id,d3,d4)VALUES(1,'d5','d6');

                "));
            }

            // RUN INIT AND TEST
            using (var outer = new ConfigurationContainer().CreateScope(@"Files\SlaveGetsInsert.xml?Mode=init")) {
                using (var inner = new TestContainer(new SqlServerModule()).CreateScope(outer, new ConsoleLogger(LogLevel.Debug))) {
                    var process = inner.Resolve<Process>();
                    var controller = inner.Resolve<IProcessController>();
                    controller.Execute();
                    Assert.AreEqual((uint)2, process.Entities.First().Inserts);
                    Assert.AreEqual((uint)1, process.Entities.Last().Inserts);
                }
            }

            using (var cn = new SqlServerConnectionFactory(OutputConnection).GetConnection()) {
                cn.Open();
                Assert.AreEqual(2, cn.ExecuteScalar<int>("SELECT COUNT(*) FROM TestMasterTable;"));
                Assert.AreEqual(1, cn.ExecuteScalar<int>("SELECT COUNT(*) FROM TestSlaveTable;"));
            }

            // FIRST DELTA, NO CHANGES
            using (var outer = new ConfigurationContainer().CreateScope(@"Files\SlaveGetsInsert.xml")) {
                using (var inner = new TestContainer(new SqlServerModule()).CreateScope(outer, new ConsoleLogger(LogLevel.Debug))) {
                    var process = inner.Resolve<Process>();
                    var controller = inner.Resolve<IProcessController>();
                    controller.Execute();
                    Assert.AreEqual((uint)0, process.Entities.First().Inserts);
                    Assert.AreEqual((uint)0, process.Entities.Last().Inserts);
                }
            }

            using (var cn = new SqlServerConnectionFactory(OutputConnection).GetConnection()) {
                cn.Open();
                Assert.AreEqual(2, cn.ExecuteScalar<int>("SELECT COUNT(*) FROM TestMasterTable;"));
                Assert.AreEqual(1, cn.ExecuteScalar<int>("SELECT COUNT(*) FROM TestSlaveTable;"));
                Assert.AreEqual(2, cn.ExecuteScalar<int>("select Id from TestStar where d3 = '' and d4 = '';"));
            }

            // insert into slave
            using (var cn = new SqlServerConnectionFactory(InputConnection).GetConnection()) {
                cn.Open();
                const string sql = @"INSERT INTO SlaveTable(Id,d3,d4)VALUES(2,'d7','d8');";
                Assert.AreEqual(1, cn.Execute(sql));
            }

            // RUN AND CHECK
            using (var outer = new ConfigurationContainer().CreateScope(@"Files\SlaveGetsInsert.xml")) {
                using (var inner = new TestContainer(new SqlServerModule()).CreateScope(outer, new ConsoleLogger(LogLevel.Debug))) {
                    var process = inner.Resolve<Process>();
                    var controller = inner.Resolve<IProcessController>();
                    controller.Execute();
                    Assert.AreEqual((uint)0, process.Entities.First().Inserts);
                    Assert.AreEqual((uint)1, process.Entities.Last().Inserts);
                }
            }

            using (var cn = new SqlServerConnectionFactory(OutputConnection).GetConnection()) {
                cn.Open();
                Assert.AreEqual(2, cn.ExecuteScalar<int>("SELECT COUNT(*) FROM TestMasterTable;"));
                Assert.AreEqual(2, cn.ExecuteScalar<int>("SELECT COUNT(*) FROM TestSlaveTable;"));
                Assert.AreEqual(0, cn.ExecuteScalar<int>("select Id from TestStar where d3 = '' and d4 = '';"));
                Assert.AreEqual(2, cn.ExecuteScalar<int>("select Id from TestStar where d3 = 'd7' and d4 = 'd8';"));
            }

        }
    }
}

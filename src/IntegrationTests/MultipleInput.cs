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

using Autofac;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Transformalize.Configuration;
using Transformalize.Containers.Autofac;
using Transformalize.Contracts;
using Transformalize.Providers.Console;
using Transformalize.Providers.SqlServer;
using Transformalize.Providers.SqlServer.Autofac;

namespace IntegrationTests {

    [TestClass]
    public class MultipleInput {

        public string TestFile { get; set; } = @"Files\MultipleInput.xml";

        public Connection InputConnection { get; set; } = new Connection {
            Name = "input",
            Provider = "sqlserver",
            ConnectionString = "server=localhost;database=master;trusted_connection=true;"
        };

        public Connection OutputConnection { get; set; } = new Connection {
            Name = "output",
            Provider = "sqlserver",
            ConnectionString = "Server=localhost;Database=Junk;trusted_connection=true;"
        };

        [TestMethod]
        [Ignore("decided not to do multiple input")]
        public void MultipleInput_Integration() {

            // INITIALIZE DATA
            using (var cn = new SqlServerConnectionFactory(InputConnection).GetConnection()) {
                cn.Open();
                cn.Execute(System.IO.File.ReadAllText(@"files\CreateDatabase1.sql"));
                cn.Execute(System.IO.File.ReadAllText(@"files\CreateDatabase2.sql"));
            }

            // RUN INIT AND TEST
            using (var outer = new ConfigurationContainer(new TransformModule()).CreateScope(TestFile + "?Mode=init")) {
                using (var inner = new TestContainer(new TransformModule(), new SqlServerModule()).CreateScope(outer, new ConsoleLogger(LogLevel.Debug))) {
                    var controller = inner.Resolve<IProcessController>();
                    controller.Execute();
                }
            }

            using (var cn = new SqlServerConnectionFactory(OutputConnection).GetConnection()) {
                cn.Open();
                Assert.AreEqual(5, cn.ExecuteScalar<int>("SELECT COUNT(*) FROM MultipleInputStar;"), "There should be 5 questions (2 in one database, 3 in the other).");
            }


        }
    }
}

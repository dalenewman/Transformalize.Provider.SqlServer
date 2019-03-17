﻿#region license
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
using System.Linq;
using Transformalize.Configuration;
using Transformalize.Containers.Autofac;
using Transformalize.Contracts;
using Transformalize.Providers.Ado;
using Transformalize.Providers.Ado.Autofac;
using Transformalize.Providers.Bogus.Autofac;
using Transformalize.Providers.Console;
using Transformalize.Providers.SqlServer.Autofac;
using Transformalize.Transforms.Ado.Autofac;

namespace IntegrationTests {

   [TestClass]
   public class Test {

      [TestMethod]
      public void Write() {
         const string xml = @"<add name='Bogus' mode='init'>
  <parameters>
    <add name='Size' type='int' value='1000' />
  </parameters>
  <connections>
    <add name='input' provider='bogus' seed='1' />
    <add name='output' provider='sqlserver' database='Junk' />
  </connections>
  <entities>
    <add name='Contact' size='@[Size]'>
      <fields>
        <add name='Identity' type='int' />
        <add name='FirstName' />
        <add name='LastName' />
        <add name='Stars' type='byte' min='1' max='5' />
        <add name='Reviewers' type='int' min='0' max='500' />
      </fields>
    </add>
  </entities>
</add>";
         using (var outer = new ConfigurationContainer().CreateScope(xml)) {

            var process = outer.Resolve<Process>();
            using (var inner = new TestContainer(new BogusModule(), new AdoProviderModule(), new SqlServerModule()).CreateScope(process, new ConsoleLogger(LogLevel.Debug))) {

               var controller = inner.Resolve<IProcessController>();
               controller.Execute();

               Assert.AreEqual(process.Entities.First().Inserts, (uint)1000);
            }
         }
      }

      [TestMethod]
      public void Read() {
         const string xml = @"<add name='Bogus'>
  <connections>
    <add name='input' provider='sqlserver' database='Junk' />
    <add name='output' provider='internal' />
  </connections>
  <entities>
    <add name='BogusStar' alias='Contact' page='1' size='10'>
      <order>
        <add field='Identity' />
      </order>
      <fields>
        <add name='Identity' type='int' />
        <add name='FirstName' />
        <add name='LastName' />
        <add name='Stars' type='byte' />
        <add name='Reviewers' type='int' />
      </fields>
    </add>
  </entities>
</add>";
         using (var outer = new ConfigurationContainer().CreateScope(xml)) {
            var process = outer.Resolve<Process>();
            using (var inner = new TestContainer(new SqlServerModule()).CreateScope(process, new ConsoleLogger(LogLevel.Debug))) {

               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
               var rows = process.Entities.First().Rows;

               Assert.AreEqual(10, rows.Count);

            }
         }
      }

      [TestMethod]
      public void ReadWithExpression() {
         const string xml = @"<add name='Bogus'>
  <parameters>
     <add name='Id' value='2' />
  </parameters>
  <connections>
    <add name='input' provider='sqlserver' database='Junk' />
    <add name='output' provider='internal' />
  </connections>
  <entities>
    <add name='BogusStar' alias='Contact'>
      <order>
        <add field='Identity' />
      </order>
      <filter>
         <add expression='[Identity] = @Id' />
      </filter>
      <fields>
        <add name='Identity' type='int' />
        <add name='FirstName' />
        <add name='LastName' />
        <add name='Stars' type='byte' />
        <add name='Reviewers' type='int' />
      </fields>
    </add>
  </entities>
</add>";
         using (var outer = new ConfigurationContainer().CreateScope(xml)) {
            var process = outer.Resolve<Process>();
            using (var inner = new TestContainer(new SqlServerModule()).CreateScope(process, new ConsoleLogger(LogLevel.Debug))) {

               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
               var rows = process.Entities.First().Rows;

               Assert.AreEqual(1, rows.Count);

            }
         }
      }

      [TestMethod]
      public void CorrelatedSubQuery() {
         const string xml = @"<add name='Test'>
  <connections>
    <add name='input' provider='internal' />
    <add name='northwind' provider='sqlserver' server='localhost' database='NorthWind' />
    <add name='output' provider='internal' />
  </connections>
  <entities>
    <add name='Test'>
      <rows>
        <add CustomerID='OCEAN' />
        <add CustomerID='PARIS' />
      </rows>
      <fields>
        <add name='CustomerID' length='5' />
      </fields>
      <calculated-fields>
        <add name='x' output='false'>
            <transforms>
                <add method='fromquery'
                     connection='northwind'
                     query='SELECT City, Country FROM Customers WHERE CustomerID = @CustomerID'>
                    <fields>
                        <add name='City' />
                        <add name='Country' />
                    </fields>
                </add>
            </transforms>
        </add>
      </calculated-fields>

    </add>
  </entities>
</add>";
         using (var outer = new ConfigurationContainer().CreateScope(xml)) {
            var process = outer.Resolve<Process>();

            using (var inner = new TestContainer(new AdoTransformModule(), new SqlServerModule()).CreateScope(process, new ConsoleLogger(LogLevel.Debug))) {

               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
               var rows = process.Entities.First().Rows;

               Assert.AreEqual(2, rows.Count);
               Assert.AreEqual("Buenos Aires", rows[0]["City"]);
               Assert.AreEqual("France", rows[1]["Country"]);

            }
         }
      }

      [TestMethod]
      public void CorrelatedSubCommand() {

         /* -- test table
          * create table TestTable(
TestTableId INT NOT NULL PRIMARY KEY IDENTITY(1,1),
TestColumn1 NVARCHAR(10) NOT NULL,
TestColumn2 INT NOT NULL
); */

         const string xml = @"<add name='Test'>
  <connections>
    <add name='input' provider='internal' />
    <add name='junk' provider='sqlserver' server='localhost' database='Junk' />
    <add name='output' provider='internal' />
  </connections>
  <entities>
    <add name='Test'>
      <rows>
        <add TestColumn1='OCEAN' TestColumn2='3' />
        <add TestColumn1='PARIS' TestColumn2='4' />
      </rows>
      <fields>
        <add name='TestColumn1' length='10' t='lower()' />
        <add name='TestColumn2' type='int' />
      </fields>
      <calculated-fields>
        <add name='x' output='false' length='128' connection='junk' t='run(INSERT INTO TestTable(TestColumn1,TestColumn2) VALUES (@TestColumn1,@TestColumn2);)' />
      </calculated-fields>
    </add>
  </entities>
</add>";
         using (var outer = new ConfigurationContainer(new AdoTransformModule()).CreateScope(xml)) {
            var process = outer.Resolve<Process>();
            using (var inner = new TestContainer(new AdoTransformModule(), new SqlServerModule()).CreateScope(process, new ConsoleLogger(LogLevel.Debug))) {

               var factory = inner.ResolveNamed<IConnectionFactory>(process.Connections[1].Key);

               using (var cn = factory.GetConnection()) {
                  cn.Open();
                  cn.Execute("DELETE FROM TestTable;");
               }

               var controller = inner.Resolve<IProcessController>();
               controller.Execute();


               using (var cn = factory.GetConnection()) {
                  cn.Open();
                  Assert.AreEqual(2, cn.ExecuteScalar<int>("SELECT COUNT(*) FROM TestTable;"));
               }
            }
         }
      }
   }
}

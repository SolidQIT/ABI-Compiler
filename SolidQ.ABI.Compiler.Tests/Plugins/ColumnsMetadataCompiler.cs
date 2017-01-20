using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SolidQ.ABI.Compiler.Infrastructure;
using System.Data.OleDb;
using System.Diagnostics;

namespace SolidQ.ABI.Compiler.Tests.Plugins
{
    [TestClass]
    public class ColumnsMetadataCompiler
    {
        private static string ConnectionString = @"Provider=SQLNCLI11.1;Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=tempdb;Integrated Security=SSPI;";

        [TestInitialize]
        public void Initialize()
        {
            using (var connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new OleDbCommand("CREATE TABLE [dbo].[SolidQABIPluginColumnsMetadataCompilerTest] ([column1] INT, [column2] VARCHAR(10))", connection))
                    command.ExecuteNonQuery();
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            using (var connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new OleDbCommand("DROP TABLE [dbo].[SolidQABIPluginColumnsMetadataCompilerTest]", connection))
                    command.ExecuteNonQuery();
            }
        }

        [TestMethod]
        public void PluginColumnsMetadataCompilerResolveColumns()
        {
            string jsonEscapedConnectionStirng = ConnectionString.Replace(@"\", @"\\");
            string jsonInput = @"
            {
                ""Object"": {
                    ""Schema"": ""$$Run-Plugin::ColumnsMetadataCompiler['" + jsonEscapedConnectionStirng + @"', 'dbo', 'SolidQABIPluginColumnsMetadataCompilerTest']""
                }
            }";
            string jsonOutput = @"
            {
                ""Object"": {
                    ""Schema"": [
                        { 
                            ""Name"" : ""column1"",
                            ""DataType"" : ""int""
                        },
                        { 
                            ""Name"" : ""column2"",
                            ""DataType"" : ""varchar(10)""
                        }                          
                    ]
                }
            }";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolvePluginExpressions(ref warnings);

            Debug.Print(Utilities.Linearize(json.ToString(Formatting.None)));
            Debug.Print(Utilities.Linearize(jsonOutput));

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(Utilities.Linearize(json.ToString(Formatting.None)), Utilities.Linearize(jsonOutput));
        }
    }
}

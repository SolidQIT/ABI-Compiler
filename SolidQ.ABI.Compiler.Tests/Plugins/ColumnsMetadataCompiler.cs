using System.Data;
using System.Data.OleDb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SolidQ.ABI.Compiler.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SolidQ.ABI.Compiler.Tests.Plugins
{
    [TestClass]
    public class ColumnsMetadataCompiler
    {      
        // The string must be escaped to fit into a JSON document: the single backslash must be specified using a double backslash.
        // If you don't have a named instance to use, you can just create an Alias to your default instance via SQL Server Configuration Manager
        private static string _connectionStringNamed = @"Provider=SQLNCLI11.1; Data Source=test\\named;Initial Catalog=TempDB;Integrated Security=SSPI;";
        private static string _connectionStringDefault = @"Provider=SQLNCLI11.1; Data Source=localhost;Initial Catalog=TempDB;Integrated Security=SSPI;";

        [TestInitialize]
        public void Initialize()
        {
            using (var connection = new OleDbConnection(_connectionStringDefault))
            {
                connection.Open();

                using (var command = new OleDbCommand("create table dbo.ColumnsMetadataCompilerTest ([column1] int, column2 varchar(10))", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            using (var connection = new OleDbConnection("Provider=SQLNCLI11.1; Data Source=localhost;Initial Catalog=TempDB;Integrated Security=SSPI;"))
            {
                connection.Open();

                using (var command = new OleDbCommand("drop table dbo.ColumnsMetadataCompilerTest", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [TestMethod]
        public void ExecutePluginWithDefaultInstance()
        {
            string jsonInput = @"
            {
                ""Object"": {
                    ""Schema"": ""$$Run-Plugin::ColumnsMetadataCompiler['" + _connectionStringDefault + @"', 'dbo', 'ColumnsMetadataCompilerTest']""
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

            System.Diagnostics.Debug.Print(Utilities.Linearize(json.ToString(Formatting.None)));
            System.Diagnostics.Debug.Print(Utilities.Linearize(jsonOutput));

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(Utilities.Linearize(json.ToString(Formatting.None)), Utilities.Linearize(jsonOutput));
        }

        [TestMethod]
        public void ExecutePluginWithNamedInstance()
        {
            string jsonInput = @"
            {
                ""Object"": {
                    ""Schema"": ""$$Run-Plugin::ColumnsMetadataCompiler['" + _connectionStringNamed  + @"', 'dbo', 'ColumnsMetadataCompilerTest']""
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

            System.Diagnostics.Debug.Print(Utilities.Linearize(json.ToString(Formatting.None)));
            System.Diagnostics.Debug.Print(Utilities.Linearize(jsonOutput));

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(Utilities.Linearize(json.ToString(Formatting.None)), Utilities.Linearize(jsonOutput));
        }
    }
}

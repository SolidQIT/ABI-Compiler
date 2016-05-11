using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SolidQ.ABI.Compiler.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SolidQ.ABI.Plugin.Tests
{
    [TestClass]
    public class Plugins
    {
        private string Linearize(string value)
        {
            if (value == null)
                return null;

            while (value.IndexOf(" ") != -1) value = value.Replace(" ", "");
            while (value.IndexOf("\t") != -1) value = value.Replace("\t", "");
            while (value.IndexOf("\r") != -1) value = value.Replace("\r", "");
            while (value.IndexOf("\n") != -1) value = value.Replace("\n", "");

            return value;
        }

        [TestMethod]
        public void ExecutePluginColumnsMetadataCompiler()
        {
            string jsonInput = @"
            {
                ""Object"": {
                    ""Schema"": ""$$Run-Plugin::ColumnsMetadataCompiler['Provider=SQLNCLI11.1; Data Source=localhost;Initial Catalog=TempDB;Integrated Security=SSPI;', 'dbo', 'test']""
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

            System.Diagnostics.Debug.Print(Linearize(json.ToString(Formatting.None)));
            System.Diagnostics.Debug.Print(Linearize(jsonOutput));

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(Linearize(json.ToString(Formatting.None)), Linearize(jsonOutput));
        }
    }
}

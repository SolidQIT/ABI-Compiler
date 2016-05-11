using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SolidQ.ABI.Compiler.Infrastructure;

namespace SolidQ.ABI.Compiler.Tests
{
    [TestClass]
    public class Extensions
    {        
        [TestMethod]
        public void ResolveJsonReferencesValueSingle()
        {
            string jsonInput = @"{""Name1"":""Value1"",""Name2"":""$(Name1)""}";
            string jsonOutput = @"{""Name1"":""Value1"",""Name2"":""Value1""}";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(json.ToString(Formatting.None), jsonOutput);
        }

        [TestMethod]
        public void ResolveJsonReferencesObjectSingle()
        {
            string jsonInput = @"
            {
	            ""Parent"":
	            {
		            ""ParentProperty1"": ""Value1"",
		            ""ParentProperty2"": ""Value2-$(Parent.ParentProperty1)"",
                    ""ParentArray"": 
                    [
                        ""ArrayValue1"", ""ArrayValue2"", ""$(Parent.ParentProperty2)"" 
                    ]
	            },
	            ""Child1"": ""$(Parent)"",
                ""Child2"": 
                {
                    ""Child2Property1"": ""Value1"",
                    ""Child2Property2"": ""$(Parent)"",
                },
	            ""Child3"": ""$(Parent.ParentArray)""            
            }";
            string jsonOutput = @"
            {
	            ""Parent"":
	            {
		            ""ParentProperty1"": ""Value1"",
		            ""ParentProperty2"": ""Value2-Value1"",
                    ""ParentArray"": 
                    [
                        ""ArrayValue1"", ""ArrayValue2"", ""Value2-Value1""
                    ]
	            },
	            ""Child1"": 
				{
		            ""ParentProperty1"": ""Value1"",
		            ""ParentProperty2"": ""Value2-Value1"",
                    ""ParentArray"": 
                    [
                        ""ArrayValue1"", ""ArrayValue2"", ""Value2-Value1""
                    ]
	            },
                ""Child2"": 
                {
                    ""Child2Property1"": ""Value1"",
                    ""Child2Property2"": 
                    {
		                ""ParentProperty1"": ""Value1"",
		                ""ParentProperty2"": ""Value2-Value1"",
                        ""ParentArray"": 
                        [
                            ""ArrayValue1"", ""ArrayValue2"",""Value2-Value1""
                        ]
	                }
                },
	            ""Child3"":
                [
                    ""ArrayValue1"", ""ArrayValue2"", ""Value2-Value1""
                ]
            }";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(Utilities.Linearize(json.ToString(Formatting.None)), Utilities.Linearize(jsonOutput));
        }

        [TestMethod]
        public void ResolveJsonReferencesValueSingleWithWarnings()
        {
            string jsonInput = @"{""Name1"":""Value1"",""Name2"":""$(Name1)"",""Name3"":""$(Name9)""}";
            string jsonOutput = @"{""Name1"":""Value1"",""Name2"":""Value1"",""Name3"":""$(Name9)""}";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(1, warnings);
            Assert.AreEqual(json.ToString(Formatting.None), jsonOutput);
        }

        [TestMethod]
        public void ResolveJsonReferencesObjectSingleWithWarnings()
        {
            string jsonInput = @"
            {
	            ""Parent"":
	            {
		            ""Property1"": ""Value1"",
		            ""Property2"": ""Value2-$(Parent.Property1)""
	            },
	            ""Child"": ""$(Parent9)""
            }";
            string jsonOutput = @"
            {
	            ""Parent"":
	            {
		            ""Property1"": ""Value1"",
		            ""Property2"": ""Value2-Value1""
	            },
	            ""Child"": ""$(Parent9)""
            }";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(1, warnings);
            Assert.AreEqual(Utilities.Linearize(json.ToString(Formatting.None)), Utilities.Linearize(jsonOutput));
        }

        [TestMethod]
        public void ResolveJsonReferencesValueMultiple()
        {
            string jsonInput = @"{""Name1"":""Value1"",""Name2"":""$(Name1)"",""Name3"":""$(Name1)""}";
            string jsonOutput = @"{""Name1"":""Value1"",""Name2"":""Value1"",""Name3"":""Value1""}";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(json.ToString(Formatting.None), jsonOutput);
        }

        [TestMethod]
        public void ResolveJsonReferencesObjectMultiple()
        {
            string jsonInput = @"
            {
	            ""Parent"":
	            {
		            ""Property1"": ""Value1"",
		            ""Property2"": ""Value2""
	            },
	            ""Child1"": ""$(Parent)"",
	            ""Child2"": ""$(Parent)""
            }";
            string jsonOutput = @"
            {
	            ""Parent"":
	            {
		            ""Property1"": ""Value1"",
		            ""Property2"": ""Value2""
	            },
	            ""Child1"": 
				{
		            ""Property1"": ""Value1"",
		            ""Property2"": ""Value2""
	            },
	            ""Child2"": 
				{
		            ""Property1"": ""Value1"",
		            ""Property2"": ""Value2""
	            }
            }";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(Utilities.Linearize(json.ToString(Formatting.None)), Utilities.Linearize(jsonOutput));
        }

        [TestMethod]
        public void ResolveJsonReferencesValueMultipleWithWarnings()
        {
            string jsonInput = @"{""Name1"":""Value1"",""Name2"":""$(Name1)"",""Name3"":""$(Name1)"",""Name4"":""$(Name9)""}";
            string jsonOutput = @"{""Name1"":""Value1"",""Name2"":""Value1"",""Name3"":""Value1"",""Name4"":""$(Name9)""}";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(1, warnings);
            Assert.AreEqual(json.ToString(Formatting.None), jsonOutput);
        }

        [TestMethod]
        public void ResolveJsonReferencesObjectMultipleWithWarnings()
        {
            string jsonInput = @"
            {
	            ""Parent"":
	            {
		            ""ParentProperty1"": ""Value1"",
		            ""ParentProperty2"": ""Value2-$(Parent.ParentProperty1)""
	            },
	            ""Child1"": ""$(Parent)"",
                ""FooChild2"":
	            {
		            ""FooProperty1"": ""Value1"",
		            ""FooProperty2"": ""Value2""
	            },
	            ""Child3"": ""$(Parent8)"",
	            ""Child4"": ""$(Parent)"",
	            ""Child5"": ""$(Parent9)""
            }";
            string jsonOutput = @"
            {
	            ""Parent"":
	            {
		            ""ParentProperty1"": ""Value1"",
		            ""ParentProperty2"": ""Value2-Value1""
	            },
	            ""Child1"": 
				{
		            ""ParentProperty1"": ""Value1"",
		            ""ParentProperty2"": ""Value2-Value1""
	            },
                ""FooChild2"":
	            {
		            ""FooProperty1"": ""Value1"",
		            ""FooProperty2"": ""Value2""
	            },
	            ""Child3"": ""$(Parent8)"",
	            ""Child4"": 
				{
		            ""ParentProperty1"": ""Value1"",
		            ""ParentProperty2"": ""Value2-Value1""
	            },
	            ""Child5"": ""$(Parent9)""
            }";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(2, warnings);
            Assert.AreEqual(Utilities.Linearize(json.ToString(Formatting.None)), Utilities.Linearize(jsonOutput));
        }

        [TestMethod]
        public void ResolveJsonReferencesValueRecursive()
        {
            string jsonInput = @"{""Name1"":""Value1"",""Name2"":""$(Name1)"",""Name3"":""$(Name2)"",""Name4"":""$(Name3)""}";
            string jsonOutput = @"{""Name1"":""Value1"",""Name2"":""Value1"",""Name3"":""Value1"",""Name4"":""Value1""}";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(json.ToString(Formatting.None), jsonOutput);
        }

        [TestMethod]
        public void ResolveJsonReferencesObjectRecursive()
        {
            string jsonInput = @"
            {
	            ""Parent"":
	            {
		            ""ParentProperty1"": ""Value1"",
		            ""ParentProperty2"": ""Value2-$(Parent.ParentProperty1)""
	            },
	            ""Child1"":
                {
		            ""Child1Property1"": ""Value1"",
		            ""Child1Property2"": ""Value2"",
                    ""ParentObject"": ""$(Parent)"",
                    ""ParentObject2"": ""$(Parent)""
                },
	            ""Child2"": ""$(Child1)"",
	            ""Child3"": ""$(Child1)""
            }";
            string jsonOutput = @"
            {
	            ""Parent"":
	            {
		            ""ParentProperty1"": ""Value1"",
		            ""ParentProperty2"": ""Value2-Value1""
	            },
	            ""Child1"": 
				{
		            ""Child1Property1"": ""Value1"",
		            ""Child1Property2"": ""Value2"",
                    ""ParentObject"": 
                    {
		                ""ParentProperty1"": ""Value1"",
		                ""ParentProperty2"": ""Value2-Value1""
	                },
                    ""ParentObject2"": 
                    {
		                ""ParentProperty1"": ""Value1"",
		                ""ParentProperty2"": ""Value2-Value1""
	                }
	            },
	            ""Child2"": 
                {
		            ""Child1Property1"": ""Value1"",
		            ""Child1Property2"": ""Value2"",
                    ""ParentObject"": 
                    {
		                ""ParentProperty1"": ""Value1"",
		                ""ParentProperty2"": ""Value2-Value1""
	                },
                    ""ParentObject2"": 
                    {
		                ""ParentProperty1"": ""Value1"",
		                ""ParentProperty2"": ""Value2-Value1""
	                }
	            },
	            ""Child3"": 
                {
		            ""Child1Property1"": ""Value1"",
		            ""Child1Property2"": ""Value2"",
                    ""ParentObject"": 
                    {
		                ""ParentProperty1"": ""Value1"",
		                ""ParentProperty2"": ""Value2-Value1""
	                },
                    ""ParentObject2"": 
                    {
		                ""ParentProperty1"": ""Value1"",
		                ""ParentProperty2"": ""Value2-Value1""
	                }
	            }
            }";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(Utilities.Linearize(json.ToString(Formatting.None)), Utilities.Linearize(jsonOutput));
        }

        [TestMethod]
        public void ResolveJsonReferencesObjectRecursiveCascade()
        {
            string jsonInput = @"
            {
	            ""Parent"":
	            {
		            ""ParentProperty1"": ""Value1"",
		            ""ParentProperty2"": ""Value2-$(Parent.ParentProperty1)""
	            },
	            ""Child1"":
                {
		            ""Child1Property1"": ""Value1"",
		            ""Child1Property2"": ""Value2"",
                    ""ParentObject"": ""$(Parent)"",
                    ""ParentObject2"": ""$(Parent)""
                },
	            ""Child2"": ""$(Child1)"",
	            ""Child3"": ""$(Child2)"",
	            ""Child4"": ""$(Child3)""
            }"; ;
            string jsonOutput = @"
            {
	            ""Parent"" :
	            {
		            ""ParentProperty1"" : ""Value1"",
		            ""ParentProperty2"" : ""Value2-Value1""
	            },
	            ""Child1"" :
	            {
		            ""Child1Property1"" : ""Value1"",
		            ""Child1Property2"" : ""Value2"",
		            ""ParentObject"" :
		            {
			            ""ParentProperty1"" : ""Value1"",
			            ""ParentProperty2"" : ""Value2-Value1""
		            },
		            ""ParentObject2"" :
		            {
			            ""ParentProperty1"" : ""Value1"",
			            ""ParentProperty2"" : ""Value2-Value1""
		            }
	            },
	            ""Child2"" :
	            {
		            ""Child1Property1"" : ""Value1"",
		            ""Child1Property2"" : ""Value2"",
		            ""ParentObject"" :
		            {
			            ""ParentProperty1"" : ""Value1"",
			            ""ParentProperty2"" : ""Value2-Value1""
		            },
		            ""ParentObject2"" :
		            {
			            ""ParentProperty1"" : ""Value1"",
			            ""ParentProperty2"" : ""Value2-Value1""
		            }
	            },
	            ""Child3"" : ""{\r\n\""Child1Property1\"":\""Value1\"",\r\n\""Child1Property2\"":\""Value2\"",\r\n\""ParentObject\"":\""{\r\n\""ParentProperty1\"":\""Value1\"",\r\n\""ParentProperty2\"":\""Value2-Value1\""\r\n}\"",\r\n\""ParentObject2\"":\""{\r\n\""ParentProperty1\"":\""Value1\"",\r\n\""ParentProperty2\"":\""Value2-Value1\""\r\n}\""\r\n}"",
	            ""Child4"" : ""{\r\n\""Child1Property1\"":\""Value1\"",\r\n\""Child1Property2\"":\""Value2\"",\r\n\""ParentObject\"":\""{\r\n\""ParentProperty1\"":\""Value1\"",\r\n\""ParentProperty2\"":\""Value2-Value1\""\r\n}\"",\r\n\""ParentObject2\"":\""{\r\n\""ParentProperty1\"":\""Value1\"",\r\n\""ParentProperty2\"":\""Value2-Value1\""\r\n}\""\r\n}""
            }";

            // Condizione non gestita al momento.
            // Quando viene valutata l'espressione $(Child2) questa risulta essere un oggetto di tipo JValue quindi viene copiata come valore e non come oggetto.
            // Per gestire questa condizione è necessario effettuare il parsing della string JSON ad ogni modifica perchè un oggetto potrebbe cambiare da JValue a JObject a seguito dell'applicazione di una espressione.

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(Utilities.Linearize(json.ToString(Formatting.None)), Utilities.Linearize(jsonOutput));
        }

        [TestMethod]
        public void ResolveJsonReferencesValueRecursiveWithWarnings()
        {
            string jsonInput = @"{""Name1"":""Value1"",""Name2"":""$(Name1)"",""Name3"":""$(Name2)"",""Name4"":""$(Name9)""}";
            string jsonOutput = @"{""Name1"":""Value1"",""Name2"":""Value1"",""Name3"":""Value1"",""Name4"":""$(Name9)""}";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(1, warnings);
            Assert.AreEqual(json.ToString(Formatting.None), jsonOutput);
        }

        [TestMethod]
        public void ResolveJsonReferencesObjectRecursiveWithWarnings()
        {
            string jsonInput = @"
            {
	            ""Parent"":
	            {
		            ""ParentProperty1"": ""Value1"",
		            ""ParentProperty2"": ""Value2-$(Parent.ParentProperty3)""
	            },
	            ""Child1"":
                {
		            ""Child1Property1"": ""Value1"",
		            ""Child1Property2"": ""Value2"",
                    ""ParentObject"": ""$(Parent)"",
                    ""ParentObject2"": ""$(Parent)""
                },
	            ""Child2"": ""$(Child1)"",
	            ""Child3"": ""$(Child1)"",
	            ""Child4"": ""$(Child9)""
            }";
            string jsonOutput = @"
            {
	            ""Parent"" :
	            {
		            ""ParentProperty1"" : ""Value1"",
		            ""ParentProperty2"" : ""Value2-$(Parent.ParentProperty3)""
	            },
	            ""Child1"" :
	            {
		            ""Child1Property1"" : ""Value1"",
		            ""Child1Property2"" : ""Value2"",
		            ""ParentObject"" : ""$(Parent)"",
		            ""ParentObject2"" : ""$(Parent)""
	            },
	            ""Child2"" : ""$(Child1)"",
	            ""Child3"" : ""$(Child1)"",
	            ""Child4"" : ""$(Child9)""
            }";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(2, warnings);
            Assert.AreEqual(Utilities.Linearize(json.ToString(Formatting.None)), Utilities.Linearize(jsonOutput));
        }

        [TestMethod]
        public void ResolveJsonReferencesValuePreserveEscapeOnFilePath()
        {
            string jsonInput = @"{""File1"":""C:\\Folder\\File.txt"",""File2"":""$(File1)""}";
            string jsonOutput = @"{""File1"":""C:\\Folder\\File.txt"",""File2"":""C:\\Folder\\File.txt""}";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(json.ToString(Formatting.None), jsonOutput);
        }

        [TestMethod]
        public void ResolveJsonReferencesObjectPreserveEscapeOnFilePath()
        {
            string jsonInput = @"
            {
	            ""Parent"":
	            {
		            ""ParentProperty1"": ""C:\\Folder\\File.txt"",
		            ""ParentProperty2"": ""Value2""
	            },
	            ""Child1"":
                {
		            ""Child1Property1"": ""C:\\Folder\\File.txt"",
		            ""Child1Property2"": ""Value2"",
                    ""ParentObject"": ""$(Parent)""
                },
	            ""Child2"": ""$(Child1)""
            }";
            string jsonOutput = @"
            {
	            ""Parent"":
	            {
		            ""ParentProperty1"": ""C:\\Folder\\File.txt"",
		            ""ParentProperty2"": ""Value2""
	            },
	            ""Child1"": 
				{
		            ""Child1Property1"": ""C:\\Folder\\File.txt"",
		            ""Child1Property2"": ""Value2"",
                    ""ParentObject"": 
                    {
		                ""ParentProperty1"": ""C:\\Folder\\File.txt"",
		                ""ParentProperty2"": ""Value2""
	                }
	            },
	            ""Child2"": 
                {
		            ""Child1Property1"": ""C:\\Folder\\File.txt"",
		            ""Child1Property2"": ""Value2"",
                    ""ParentObject"": 
                    {
		                ""ParentProperty1"": ""C:\\Folder\\File.txt"",
		                ""ParentProperty2"": ""Value2""
	                }
	            }
            }";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolveJsonReferences(ref warnings);

            Assert.AreEqual<int>(0, warnings);
            Assert.AreEqual(Utilities.Linearize(json.ToString(Formatting.None)), Utilities.Linearize(jsonOutput));
        }

        [TestMethod]
        public void ResolvePluginExpressionsSimple()
        {
            string jsonInput = @"
            {
                ""Object"": {
                    ""Schema"": ""$$Run-Plugin::Foo['Value1', 'Value2', 'Value3']""
                }
            }";
            string jsonOutput = @"
            {
                ""Object"": {
                    ""Schema"": [
                        ""Value1"",
                        ""Value2"",
                        ""Value3""
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
        public void ResolvePluginExpressionsPreserveEscape()
        {
            string jsonInput = @"
            {
                ""Object"": {
                    ""Schema"": ""$$Run-Plugin::Foo['Value1', 'Value2', 'c:\\folder\\file.txt']""
                }
            }";
            string jsonOutput = @"
            {
                ""Object"": {
                    ""Schema"": [
                        ""Value1"",
                        ""Value2"",
                        ""c:\\folder\\file.txt""
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
        public void ResolvePluginExpressionsWithMissingPluginWarning()
        {
            string jsonInput = @"
            {
                ""Object"": {
                    ""Schema"": ""$$Run-Plugin::NotExistingPlugin['Value1', 'Value2', 'Value3']""
                }
            }";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolvePluginExpressions(ref warnings);

            Assert.AreEqual<int>(1, warnings);
        }

        [TestMethod]
        public void ResolvePluginExpressionsWithMissingPluginMultipleWarning()
        {
            string jsonInput = @"
            {
                ""Object"": {
                    ""Schema1"": ""$$Run-Plugin::NotExistingPlugin1['Value1', 'Value2', 'Value3']"",
                    ""Schema2"": ""$$Run-Plugin::NotExistingPlugin2['Value1', 'Value2', 'Value3']""
                }
            }";

            var json = JObject.Parse(jsonInput);
            var warnings = 0;

            json = json.ResolvePluginExpressions(ref warnings);

            Assert.AreEqual<int>(2, warnings);
        }
    }
}

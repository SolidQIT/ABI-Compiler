using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using SolidQ.ABI.Compiler.Infrastructure.Extensibility;
using SolidQ.ABI.Extensibility.Compiler;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace SolidQ.ABI.Compiler.Infrastructure
{
    internal static class Extensions
    {
        private static Logger _logger = LogManager.GetLogger(typeof(CompilerEngine).FullName);

        internal class RawStringJsonConverter : JsonConverter
        {
            public static RawStringJsonConverter Instance = new RawStringJsonConverter();

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(string);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return reader.Value;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteRawValue((string)value);
            }
        }

        public static string GetDescription(this Enum @enum)
        {
            var attribute = @enum.GetType().GetMember(@enum.ToString())
                .Select((m) => m.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false))
                .FirstOrDefault();
            
            if (attribute != null)
                return ((DescriptionAttribute)attribute.First()).Description;

            return @enum.ToString();
        }

        /// <summary>
        /// Syntax $$Run-Plugin::PluginName['arg1', 'arg2', 'argN']
        /// </summary>
        public static JObject ResolvePluginExpressions(this JObject json, ref int warnings)
        {
            var expressionRegex = new Regex(@"(?<expression>\$\$Run-Plugin::(?<name>[^\[]*)\[(?<arguments>[^\]]*)\])", RegexOptions.Compiled);

            var pluginExpressions = expressionRegex.Matches(json.ToString()).Cast<Match>()
                .Select((m) => new
                {
                    Expression = m.Groups["expression"].Value,
                    Name = m.Groups["name"].Value,
                    Arguments = Regex.Unescape(m.Groups["arguments"].Value)
                        .Split(',') // do not use StringSplitOptions.RemoveEmptyEntries
                        .Select((a) => a.Trim(new[] { ' ', '\'' }))
                        .ToArray()
                })
                .Distinct()
                .ToList();

            #region Print debug expressions

            _logger.Debug($"Plugin expressions found '{ pluginExpressions.Count }'"); 
                       
            foreach (var plugin in pluginExpressions)
            {
                _logger.Debug($"\tExpression '{ plugin.Expression }'");
                _logger.Debug($"\t\tPlugin '{ plugin.Name }'");

                foreach (var argument in plugin.Arguments)
                    _logger.Debug($"\t\tArgument '{ argument }'");
            }

            #endregion

            _logger.Info("Collecting plugins");

            using (var collector = new PluginCollector())
            {
                _logger.Info($"Plugins found '{ collector.Plugins.Count }'");

                foreach (var plugin in collector.Plugins)
                    _logger.Debug($"\tPlugin '{ plugin.Name }' - '{ plugin.Version }' - '{ plugin.Description }'");

                string resolvedJson = json.ToString();

                foreach (var expression in pluginExpressions)
                {
                    var plugin = collector.Plugins.SingleOrDefault((p) => p.Name.Equals(expression.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (plugin == null)
                    {
                        _logger.Warn($"Plugin not found for expression '{ expression.Expression }'");
                        warnings++;
                        continue;
                    }

                    dynamic pluginResult;

                    #region Execute plugin

                    _logger.Info($"Executing plugin '{ plugin.Name }' - '{ plugin.Version }' - '{ plugin.Description }'");

                    plugin.Initialize(_logger.Factory);
                    try
                    {
                        if (plugin is IMetadataCompilerPlugin)
                        {
                            pluginResult = (plugin as IMetadataCompilerPlugin).Compile(expression.Arguments);                        
                        }
                        /*
                         * else if (plugin is I<MyCustom>CompilerPlugin)
                         * {
                         *      pluginResult = (plugin as I<MyCustom>CompilerPlugin).Compile(expression.Arguments);
                         * }
                         */
                        else
                        {
                            throw new ApplicationException($"Invalid plugin type in expression '{ expression.Expression }'");
                        }
                    }
                    finally
                    {
                        plugin.Shutdown();
                    }

                    #endregion

                    _logger.Debug($"Resolved plugin expression type is '{ pluginResult.Get‌​Type() }' => '{ pluginResult }'");

                    var replaceExpression = expression.Expression;
                    // Add beginning and trailing quotes, since we're replacing a json value fragment with raw string json object.
                    if (!(pluginResult is JValue))
                        replaceExpression = "\"" + expression.Expression + "\"";
                    
                    resolvedJson = resolvedJson.Replace(replaceExpression, pluginResult.ToString());
                }

                _logger.Info("Parsing resolved plugin metadata");
                _logger.Debug(resolvedJson);

                return JObject.Parse(resolvedJson);
            }
        }

        /// <summary>
        /// Syntax $(path)
        /// </summary>
        public static JObject ResolveJsonReferences(this JObject json, ref int warnings)
        {
            var expressionRegex = new Regex(@"(?<expression>\$\((?<path>([^\)]+))\))", RegexOptions.Compiled);

            var expressions = expressionRegex.Matches(json.ToString()).Cast<Match>()
                .Select((m) => new
                {
                    Token = json.SelectToken(m.Groups["path"].Value),
                    Expression = m.Groups["expression"].Value,
                    Path = m.Groups["path"].Value,
                })
                .Distinct()
                .ToList();

            #region Notify missing references

            foreach (var item in expressions.Where((e) => e.Token == null || !(e.Token is JValue || e.Token is JObject || e.Token is JArray)))
            {
                _logger.Warn($"Reference to '{ item.Path }' not found.");
                warnings++;
            };

            #endregion

            string resolvedJson = json.ToString();

            #region Resolving found references

            #region Resolve function

            Func<JToken, string> resolve = (token) =>
            {
                string currentToken = token.ToString();

                while (expressionRegex.IsMatch(currentToken))
                {
                    var expression = expressions.SingleOrDefault((i) => i.Expression == expressionRegex.Match(currentToken).Value);
                    if (expression == null || expression.Token == null)
                        return null;

                    var replaceExpression = expression.Expression;
                    if (!(token is JValue) && !(expression.Token is JValue))
                        replaceExpression = "\"" + expression.Expression + "\"";

                    currentToken = currentToken.Replace(replaceExpression, expression.Token.ToString());
                }

                return currentToken;
            };

            #endregion

            foreach (var item in expressions.Where((e) => e.Token != null))
            {
                var resolvedToken = resolve(item.Token);
                if (resolvedToken == null)
                    continue;

                var replaceExpression = item.Expression;
                var replaceToken = (item.Token is JValue)
                    ? JsonConvert.SerializeObject(resolvedToken)
                    : JsonConvert.SerializeObject(resolvedToken, RawStringJsonConverter.Instance);

                // Remove beginning and trailing quotes, since we're replacing a json value fragment.
                if (item.Token is JValue)
                    replaceToken = replaceToken.Substring(1, replaceToken.Length - 2);

                // Add beginning and trailing quotes, since we're replacing a json value fragment with raw string json object.
                if (!(item.Token is JValue))
                    replaceExpression = "\"" + item.Expression + "\"";

                resolvedJson = resolvedJson.Replace(replaceExpression, replaceToken);
                _logger.Debug($"Resolved reference: '{ replaceExpression }' => '{ replaceToken }'");
            };

            #endregion

            _logger.Info("Parsing resolved metadata");
            _logger.Debug(resolvedJson);

            return JObject.Parse(resolvedJson);
        }
    }
}

using CommandLine;
using NLog;
using SolidQ.ABI.CommandLine.Options;
using SolidQ.ABI.Compiler;
using SolidQ.ABI.Compiler.Infrastructure.Extensibility;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace SolidQ.ABI
{
    public class Program
    {
        #region Consts

        private const int ErrorArgumentParsingFailed = -1; // use only negative values (ABIExitCode are >= 0)
        private const string ErrorArgumentParsingFailedMessageFormat = "Argument parsing result [{0}]";
        private const string ErrorArgumentVerbNotImplemented = "Command option not implemented";

        #endregion

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static int Main(string[] args)
        {
#if DEBUG
            _logger.Info("DEBUG Compile");
#endif
            _logger.Info("{0} v{1}", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version);
            _logger.Info("{0} v{1}", ABIFileSystemCompiler.Name, ABIFileSystemCompiler.Version);
            _logger.Info("");

            var result = Parser.Default
                .ParseArguments<CompileOptions, ValidateOptions, PluginsOptions>(args)
                .MapResult
                (
                    (CompileOptions opts) => Compile(opts),
                    (PluginsOptions opts) => Plugins(opts),
                    (ValidateOptions opts) => Validate(opts),
                    (errs) =>
                    {
                        foreach (var error in errs)
                        {
                            if (!(error is HelpRequestedError) || (error is HelpVerbRequestedError))
                                _logger.Error(ErrorArgumentParsingFailedMessageFormat, error.Tag);
                        }

                        return ErrorArgumentParsingFailed;
                    }
                );
#if DEBUG
            Console.ReadKey();
#endif
            return result;
        }

        private static int Compile(CompileOptions compileOptions)
        {
            var metadataSearchSubDirectories =  Convert.ToBoolean(ConfigurationManager.AppSettings["MetadataSearchSubDirectories"] ?? bool.TrueString);

            var metadataSearchPattern = compileOptions.MetadataSearchPattern ?? ConfigurationManager.AppSettings["MetadataSearchPattern"] ?? "*.json";
            if (metadataSearchPattern.EndsWith("*"))
                metadataSearchPattern += ABIFileSystemOptions.StandardMetadataFileExtension;

            var fileSystemRootPath = ConfigurationManager.AppSettings["FileSystemRootPath"] ?? @".";
            var metadataFolderName = ConfigurationManager.AppSettings["MetadataFolderName"] ?? @"metadata";
            var templateFolderName = ConfigurationManager.AppSettings["TemplateFolderName"] ?? @"templates";
            var outputFolderName = ConfigurationManager.AppSettings["OutputFolderName"] ?? @"output";

            var options = new ABIFileSystemOptions(
                rootPath: fileSystemRootPath,
                metadataFolder: metadataFolderName,
                templateFolder: templateFolderName,
                outputFolder: outputFolderName,
                metadataSearchPattern: metadataSearchPattern,
                metadataSearchSubDirectories: metadataSearchSubDirectories
                );

            var result = ABIFileSystemCompiler.Compile(options, _logger.Factory);
            if (result != ABIExitCode.CompileCompleted)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result);
                Console.ResetColor();
            }

            return (int)result;
        }

        private static int Validate(ValidateOptions compileOptions)
        {
            _logger.Warn(ErrorArgumentVerbNotImplemented);

            return 0;
        }

        private static int Plugins(PluginsOptions pluginsOptions)
        {
            var action = pluginsOptions.Action.ToLower();
            if (action == "list")
            {
                using (var collector = new PluginCollector())
                {
                    _logger.Info($"Plugins found: { collector.Plugins.Count }");

                    foreach (var plugin in collector.Plugins)
                        _logger.Info($"- { plugin.Name } v.{ plugin.Version }: { plugin.Description }");
                }
            }
            else if (action == "help")
            {
                using (var collector = new PluginCollector())
                {
                    var plugin = collector.Plugins.SingleOrDefault((p) => p.Name.Equals(pluginsOptions.PluginName, StringComparison.InvariantCultureIgnoreCase));
                    if (plugin != null)
                    {
                        plugin.Initialize(_logger.Factory);

                        _logger.Info($"{ plugin.Name } v.{ plugin.Version }");
                        _logger.Info($"by { plugin.Author }");
                        _logger.Info(plugin.Description);
                        _logger.Info(plugin.Help);

                        plugin.Shutdown();
                    }
                }
            }

            return 0;
        }
    }
}

using CommandLine;
using NLog;
using SolidQ.ABI.CommandLine.Options;
using SolidQ.ABI.Compiler;
using System;
using System.Configuration;

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
            _logger.Info("{0} v{1}", ABIFileSystemCompiler.Name, ABIFileSystemCompiler.Version);
            _logger.Info("");

            var result = Parser.Default
                .ParseArguments<CompileOptions, ValidateOptions>(args)
                .MapResult(
                    (CompileOptions opts) => Compile(opts),
                    (ValidateOptions opts) => Validate(opts),
                    (errs) =>
                    {
                        foreach (var error in errs)
                            _logger.Error(ErrorArgumentParsingFailedMessageFormat, error.Tag);

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
            var metadataSearchSubDirectories = Convert.ToBoolean(compileOptions.MetadataSearchSubDirectories ?? ConfigurationManager.AppSettings["MetadataSearchSubDirectories"] ?? bool.TrueString);

            var metadataSearchPattern = compileOptions.MetadataSearchPattern ?? ConfigurationManager.AppSettings["MetadataSearchPattern"] ?? "*.json";
            if (metadataSearchPattern.EndsWith("*"))
                metadataSearchPattern += ABIFileSystemOptions.StandardMetadataFileExtension;

            var fileSystemRootPath = compileOptions.FileSystemRootPath ?? ConfigurationManager.AppSettings["FileSystemRootPath"] ?? @".";
            var metadataFolderName = compileOptions.MetadataFolderName ?? ConfigurationManager.AppSettings["MetadataFolderName"] ?? @"metadata";
            var templateFolderName = compileOptions.TemplateFolderName ?? ConfigurationManager.AppSettings["TemplateFolderName"] ?? @"templates";
            var outputFolderName = compileOptions.OutputFolderName ?? ConfigurationManager.AppSettings["OutputFolderName"] ?? @"output";

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
    }
}

using NLog;
using SolidQ.ABI.Compiler.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SolidQ.ABI.Compiler
{
    public class ABIFileSystemCompiler : ABICompiler
    {
        private static Logger _logger;

        public static ABIExitCode Compile(ABIFileSystemOptions options, LogFactory factory)
        {
            #region Argument exceptions

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            #endregion
            
            _logger = factory.GetCurrentClassLogger();
            _logger.Debug(options);         
            try
            {
                if (!Validate(options))
                    return ABIExitCode.ErrorDuringValidation;

                var engine = new CompilerEngine(options, factory);
                var result = engine.Compile();

                Debug.Assert(result != ABIExitCode.ErrorExitCodeUnassigned);

                return result;
            }
            catch (Exception ex) // avoid external unhandled exceptions
            {
                _logger.Error(ex);
            }

            return ABIExitCode.ErrorUnhandledException;
        }

        private static bool Validate(ABIFileSystemOptions options)
        {
            bool validated = true;

            #region DotLiquid

            _logger.Info("Validating DotLiquid assembly");

            var liquidAssemblies = ExecutingAssembly.GetReferencedAssemblies()
                .Where((a) => a.Name.Contains("DotLiquid"));

            var liquidRequiredVersion = new Version(1, 8, 0, 2);

            foreach (var assembly in liquidAssemblies)
            {
                _logger.Info($"Using { assembly.Name } v{ assembly.Version }");

                if (assembly.Version.CompareTo(liquidRequiredVersion) != 0)
                {
                    _logger.Error($"DotLiquid v{ liquidRequiredVersion } is needed to work properly");
                    validated = false;
                }
            }

            #endregion

            #region Option paths

            _logger.Info("Validating folder structure integrity");

            if (!Directory.Exists(options.MetadataPath))
            {
                _logger.Error($"Metadata path not found [{ options.MetadataPath }]");
                validated = false;
            }

            if (!Directory.Exists(options.TemplatePath))
            {
                _logger.Error($"Template path not found [{ options.TemplatePath }]");
                validated = false;
            }

            if (!Directory.Exists(options.OutputPath))
            {
                _logger.Info($"Output path not found. Folders have been created [{ options.OutputPath }]");
                Directory.CreateDirectory(options.OutputPath);
            }

            #endregion

            #region Option search patterns

            _logger.Info("Validating search pattern options");

            if (!options.MetadataSearchPattern.ToLower().EndsWith(ABIFileSystemOptions.StandardMetadataFileExtension))
            {
                _logger.Error($"Invalid metadata search pattern [{ options.MetadataSearchPattern }]");
                validated = false;
            }                

            #endregion

            return validated;
        }
    }
}

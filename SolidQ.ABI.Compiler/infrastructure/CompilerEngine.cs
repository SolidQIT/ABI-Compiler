﻿using DotLiquid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NLog;
using SolidQ.ABI.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SolidQ.ABI.Compiler.Infrastructure
{
    internal class CompilerEngine
    {
        public static string PluginsPath = ".\\plugins"; // todo: TBD

        private static Logger _logger;
        private ABIFileSystemOptions _options;

        public CompilerEngine(ABIFileSystemOptions options, LogFactory factory)
        {
            _logger = factory.GetCurrentClassLogger();
            _options = options;           
        }

        private void Initialize()
        {
            // DotLiquid
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            Template.FileSystem = new DotLiquid.FileSystems.LocalFileSystem(_options.TemplatePath);

            // Plugins
            Directory.CreateDirectory(PluginsPath);
        }
        
        private List<MetadataFile> CollectCommonMetadataFiles()
        {
            _logger.Info("Looking for common metadata files");

            var commonMetadataFiles = Directory
                .GetFiles(_options.CommonMetadataPath, "*" + ABIFileSystemOptions.StandardMetadataFileExtension, SearchOption.AllDirectories)
                .Select((file) => new MetadataFile(file, _options))
                .OrderBy((item) => item.FullName)
                .ToList();

            foreach (var item in commonMetadataFiles)
                _logger.Info($"\t{ item.RelativePath }");

            if (commonMetadataFiles.Count == 0)
                _logger.Info("Common metadata files not found");

            return commonMetadataFiles;
        }

        private List<MetadataFile> CollectMetadataFiles()
        {
            _logger.Info("Looking for metadata files");

            var searchOption = _options.MetadataSearchSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var metadataFiles = Directory
                .GetFiles(_options.MetadataPath, _options.MetadataSearchPattern, searchOption)
                .Where((file) => !file.StartsWith(_options.CommonMetadataPath) && !file.StartsWith(_options.SchemaPath))
                .Select((file) => new MetadataFile(file, _options))
                .OrderBy((item) => item.FullName)
                .ToList();

            foreach (var item in metadataFiles)
                _logger.Info($"\t{ item.RelativePath }");

            if (metadataFiles.Count == 0)
                _logger.Error("Metadata files not found");

            return metadataFiles;
        }

        public ABIExitCode Compile()
        {
            Initialize();

            var metadataFiles = CollectMetadataFiles();
            if (metadataFiles.Count == 0)
                return ABIExitCode.ErrorNoMetadataFilesFound;

            var commonMetadataFiles = CollectCommonMetadataFiles();

            foreach (var metadataFile in metadataFiles)
            {
                metadataFile.Result.Code = InternalCompile(metadataFile, commonMetadataFiles);

                // blocking error
                if (metadataFile.Result.Code != ABIExitCode.CompileCompleted &&
                    metadataFile.Result.Code != ABIExitCode.CompileCompletedWithWarnings)
                    return metadataFile.Result.Code;
            }

            _logger.Info("All metadata files compiled, listing results:");

            foreach (var metadataFile in metadataFiles)
                _logger.Info("\tArtifacts = {0,-2}, Warnings = {1,-2} - {2}", metadataFile.Result.Artifacts, metadataFile.Result.Warnings, metadataFile.RelativePath);

            if (metadataFiles.Any((i) => i.Result.Warnings > 0))
            {
                _logger.Warn("Done *** WITH WARNINGS ***");
                return ABIExitCode.CompileCompletedWithWarnings;
            }

            _logger.Info("Done");

            return ABIExitCode.CompileCompleted;
        }

        private ABIExitCode InternalCompile(MetadataFile metadataFile, List<MetadataFile> commonMetadataFiles)
        {
            _logger.Info("");
            _logger.Info("Compiling metadata file '{0}'", metadataFile.RelativePath);

            metadataFile.Result.Code = CompileMetadata(metadataFile, commonMetadataFiles);

            if (metadataFile.Result.Code != ABIExitCode.CompileCompleted &&
                metadataFile.Result.Code != ABIExitCode.CompileCompletedWithWarnings)
                return metadataFile.Result.Code;

            return CompileTemplates(metadataFile);
        }

        private ABIExitCode CompileMetadata(MetadataFile metadataFile, List<MetadataFile> commonMetadataFiles)
        {
            _logger.Info("Metadata file process starting");

            #region Metadata

            _logger.Info("Parsing metadata file");
            metadataFile.ParseJson();

            #endregion

            #region Common metadata

            foreach (var commonMetadata in commonMetadataFiles)
            {
                _logger.Info("Parsing common metadata file '{0}'", commonMetadata.RelativePath);
                commonMetadata.ParseJson();

                _logger.Info("Merging common metadata file '{0}'", commonMetadata.RelativePath);
                metadataFile.Json.Merge(commonMetadata.Json, new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Union });
            }

            #endregion

            #region Extended metadata

            _logger.Info("Looking for extended metadata");
            _logger.Debug($"Extended metadata file '{ metadataFile.ExtendedRelativePath }'");

            if (!File.Exists(metadataFile.ExtendedFullName))
            {
                _logger.Info("Extended metadata file not found");
            }
            else
            {
                _logger.Info($"Parsing extended metadata file '{  metadataFile.ExtendedRelativePath }'");
                metadataFile.ParseExtendedJson();

                _logger.Info($"Merging extended metadata file '{ metadataFile.ExtendedRelativePath }'");
                metadataFile.Json.Merge(metadataFile.ExtendedJson, new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Union });
            }

            #endregion

            #region Analyze metadata

            _logger.Info("Analyzing metadata");

            if (metadataFile.Json["ABI3"] == null)
                return ABIExitCode.ErrorABISectionMissingInMetadata;
            
            _logger.Info("Reading project directives");
            _logger.Info($"\tTargetSQLPlatformVersion: '{ metadataFile.TargetSQLPlatformVersion }'");

            _logger.Info("Reading template directives");
            if (metadataFile.Template == null)
                return ABIExitCode.ErrorTemplateSectionMissingInMetadata;
                  
            _logger.Info($"\tPhase: '{ metadataFile.Phase }'");
            _logger.Info($"\tPattern: '{ metadataFile.Pattern }'");
            _logger.Info($"\tSource: '{ metadataFile.Source }'");
            _logger.Info($"\tImplementation: '{ metadataFile.Implementation }'");
            _logger.Info($"\tVersion: '{ metadataFile.Version }'");

            if (string.IsNullOrEmpty(metadataFile.Pattern))
                return ABIExitCode.ErrorTemplatePatternMissingInMetadata;

            _logger.Debug(metadataFile.Json);

            _logger.Info("Resolving json references");
            metadataFile.ResolveJsonReferences();

            _logger.Info("Resolving plugin expressions");
            metadataFile.ResolvePluginExpressions();

            #endregion

            #region Compile metadata

            _logger.Info("Generating compiler metadata");
            var compilerInfo = new
            {
                ABI3 = new
                {
                    Compiler = new
                    {
                        CompilationDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss")
                    }
                }
            };
            JObject compilerMetadata = JObject.Parse(JsonConvert.SerializeObject(compilerInfo));

            _logger.Info("Merging compiler metadata");
            metadataFile.Json.Merge(compilerMetadata, new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Union });

            #endregion

            #region Schema validation

            _logger.Info("Looking for $schema");

            string fileSchema = (string)metadataFile.Json["$schema"];

            if (string.IsNullOrEmpty(fileSchema))
            {
                _logger.Info("$schema element not found, skipping validation");
            }
            else
            {
                _logger.Info("$schema element found, attempting validation");

                if (fileSchema.StartsWith("/"))
                    fileSchema = "." + fileSchema;

                fileSchema = Path.GetFullPath(Path.Combine(_options.RootPath, fileSchema.Replace("/", "\\")));            
                
                _logger.Debug($"$schema file path is '{ fileSchema }'");

                if (!File.Exists(fileSchema))
                {
                    _logger.Warn("$schema file not found, cannot validate json metadata");
                }
                else
                {
                    var jsonSchema = JSchema.Parse(File.ReadAllText(fileSchema));

                    IList<ValidationError> errors;
                    if (!metadataFile.Json.IsValid(jsonSchema, out errors))
                    {
                        metadataFile.Result.Warnings += ProcessSchemaValidationErrors(errors);
                    }
                    else
                    {
                        _logger.Info("$schema is valid");
                    }
                }
            }

            #endregion

            _logger.Info("Metadata file process completed");

            if (metadataFile.Result.Warnings > 0)
                return ABIExitCode.CompileCompletedWithWarnings;

            return ABIExitCode.CompileCompleted;
        }

        private ABIExitCode CompileTemplates(MetadataFile metadataFile)
        {
            _logger.Info("Templates process starting");

            #region Template path

            string templatePath = Path.Combine(_options.TemplatePath, metadataFile.Phase, metadataFile.Source);            
            if (!Directory.Exists(templatePath))
                return ABIExitCode.ErrorTemplatePathSourceDirectoryNotExisting;

            string templatePathVersionSpecific = Path.Combine(templatePath, metadataFile.TargetSQLPlatformVersion);
            if (Directory.Exists(templatePathVersionSpecific))
            {
                templatePath = templatePathVersionSpecific;
            }
            else
            {
                _logger.Debug($"Template path for TargetSQLPlatformVersion '{ templatePathVersionSpecific }' not found. Falling back to catch-all folder.");
            }

            _logger.Info($"Using template path '{ templatePath }'");

            #endregion

            #region Template search pattern

            string templateSearchPattern = metadataFile.Pattern;
            if (!string.IsNullOrEmpty(metadataFile.Source)) templateSearchPattern += "-" + metadataFile.Source;
            if (!string.IsNullOrEmpty(metadataFile.Implementation)) templateSearchPattern += "-" + metadataFile.Implementation;

            // BASE     -> phase\pattern-source-implementation.version.*
            // EXTENDED -> phase\pattern-source-implementation[-extensions].version.*
            string baseTemplateSearchPattern = templateSearchPattern + "." + metadataFile.Version + ".*";
            string extendedTemplateSearchPattern = templateSearchPattern + "-*." + metadataFile.Version + ".*";

            _logger.Info($"Template search patterns are '{ baseTemplateSearchPattern }' '{ extendedTemplateSearchPattern }'");

            #endregion 

            var templateFiles = Directory.EnumerateFiles(templatePath, baseTemplateSearchPattern)
                .Union(Directory.EnumerateFiles(templatePath, extendedTemplateSearchPattern))
                .OrderBy((path) => path)
                .ToList();

            _logger.Info($"Template files found '{ templateFiles.Count }'");
            _logger.Debug("Listing template files found");

            foreach (string templateFile in templateFiles)
                _logger.Debug("\t'{0}'", templateFile.Replace(templatePath, string.Empty));

            foreach (string templateFile in templateFiles)
            {
                #region Process template

                _logger.Info("Processing template file '{0}'", Path.GetFileName(templateFile));
                _logger.Debug("Template file full path '{0}'", templateFile);

                _logger.Info("Creating context");
                var renderMetadata = (Dictionary<string, object>)JsonHelper.Deserialize(metadataFile.Json.ToString());

                _logger.Info("Parsing template file");
                var template = Template.Parse(File.ReadAllText(templateFile));

                _logger.Info("Rendering");
                string outputFileContent = string.Empty;
                try
                {
                    outputFileContent = template.Render(parameters: new RenderParameters
                    { 
                        LocalVariables = Hash.FromDictionary(renderMetadata), 
                        RethrowErrors = true 
                    });

                    metadataFile.Result.Warnings += template.Errors.Count();

                    foreach (var error in template.Errors)
                        _logger.Warn(error.Message);                    
                }
                catch (DotLiquid.Exceptions.LiquidException le)
                {
                    _logger.Error(le, "Exception occurred during template rendering");
                    return ABIExitCode.ErrorTemplatePatternMissingInMetadata;
                }

                string outputPath = Path.Combine(_options.OutputPath, metadataFile.Phase);
                if (metadataFile.Phase.ToLower() == "load")
                    outputPath = Path.Combine(outputPath, metadataFile.Pattern);
                outputPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(metadataFile.FullName));
                _logger.Info($"Using output path '{ outputPath }'");

                string outputFile = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(metadataFile.FullName) + GetTemplateFileExtension(templateFile));
                
                _logger.Info("Writing output file '{0}'", outputFile.Replace(_options.OutputPath, string.Empty));
                _logger.Debug($"Output file full path '{ outputFile }'");

                Directory.CreateDirectory(outputPath);
                File.WriteAllText(outputFile, outputFileContent);

                metadataFile.Result.Artifacts += 1;

                #endregion
            }

            _logger.Info("Templates process completed");

            if (metadataFile.Result.Warnings > 0)
                return ABIExitCode.CompileCompletedWithWarnings;

            return ABIExitCode.CompileCompleted;
        }
   
        private int ProcessSchemaValidationErrors(IList<ValidationError> validationErrors)
        {
            int errorsCount = validationErrors.Count();

            foreach (ValidationError error in validationErrors)
            {
                _logger.Warn(error.Message);

                if (error.ChildErrors != null)
                    errorsCount += ProcessSchemaValidationErrors(error.ChildErrors);
            }

            return errorsCount;
        }     
        
        internal string GetTemplateFileExtension(string templateFilePath)
        {
            _logger.Debug($"Extracting extension from template '{ templateFilePath }'");
            
            var match = Regex.Match(input: Path.GetFileName(templateFilePath), pattern: @"(\.[\d]*)(.*)(\.liquid[^.]*)", options: RegexOptions.IgnoreCase);
            if (match.Success == false || match.Groups.Count != 4)
                throw new ApplicationException("Template file name has some missing part. Expected format is '<pattern-source-implementation>.<version>[.<extension>].<extension>.liquid'.");

            var extension = match.Groups[2].Value;

            _logger.Debug($"Extracted extension '{ extension }'");

            return extension;
        }  
    }
}

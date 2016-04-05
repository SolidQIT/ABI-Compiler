using Newtonsoft.Json.Linq;
using System.IO;
using SolidQ.ABI.Compiler.Infrastructure.Helper;
using System;

namespace SolidQ.ABI.Compiler.Infrastructure
{
    internal class MetadataFile
    {
        private ABIFileSystemOptions _options;

        #region Properties

        public CompilerEngineResult Result { get; private set; }

        public JObject Json { get; private set; }

        public string FullName { get; private set; }

        public string RelativePath
        {
            get
            {
                return FullName.Replace(_options.MetadataPath, string.Empty);
            }
        }

        #region Template properties

        public JToken Template
        {
            get
            {
                return Json["ABI3"]["Template"];
            }
        } 

        public string Phase
        {
            get
            {
                return Convert.ToString(Template["Phase"]);
            }
        }        
        public string Pattern
        {
            get
            {
                return Convert.ToString(Template["Pattern"]);
            }
        }  
        public string Source
        {
            get
            {
                return Convert.ToString(Template["Source"]);
            }
        }
        public string Implementation
        {
            get
            {
                return Convert.ToString(Template["Implementation"]);
            }
        }
        public string Version
        {
            get
            {
                return Convert.ToString(Template["Version"]);
            }
        }

        #endregion

        public JToken Project
        {
            get
            {
                return Json["ABI3"]["Project"];
            }

        }
        public string TargetSQLPlatformVersion
        {
            get
            {
                return Convert.ToString(Project["TargetSQLPlatformVersion"]);
            }
        }

        #region Extended metadata file properties

        public JObject ExtendedJson { get; private set; }

        public string ExtendedFullName
        {
            get
            {
                return Path.ChangeExtension(FullName, ABIFileSystemOptions.StandardExtendedMetadataFileExtension);
            }
        }

        public string ExtendedRelativePath
        {
            get
            {
                return ExtendedFullName.Replace(_options.MetadataPath, string.Empty);
            }
        }

        #endregion

        #endregion

        public MetadataFile(string fullName, ABIFileSystemOptions options)
        {
            _options = options;

            FullName = fullName;
            Result = new CompilerEngineResult();
        }

        public void ParseJson()
        {
            Json = JObject.Parse(File.ReadAllText(FullName));
        }

        public void ParseExtendedJson()
        {
            ExtendedJson = JObject.Parse(File.ReadAllText(ExtendedFullName));
        }

        public void ResolveJsonReferences()
        {
            Json = Json.ResolveJsonReferences(ref Result.Warnings);
        }

        public void ResolvePluginExpressions()
        {
            Json = Json.ResolvePluginExpressions(ref Result.Warnings);
        }
    }
}

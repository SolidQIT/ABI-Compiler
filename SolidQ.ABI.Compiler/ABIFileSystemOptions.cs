using System.IO;
using System.Text;

namespace SolidQ.ABI.Compiler
{
    public class ABIFileSystemOptions
    {
        #region Consts

        public const string StandardMetadataFileExtension = ".json";
        public const string StandardExtendedMetadataFileExtension = ".ext.json";

        private const string DefaultSchemaFolder = "_schemas";
        private const string DefaultCommonMetadataFolder = "common";
        private const string DefaultMetadataSearchPattern = "*" + StandardMetadataFileExtension;

        #endregion

        #region Properties

        public string RootPath
        {
            get
            {
                return _rootPath;
            }
        }
        public string MetadataPath
        { 
            get
            {
                return Path.Combine(_rootPath, _metadataFolder);
            }
        }
        public string TemplatePath
        {
            get
            {
                return Path.Combine(_rootPath, _templateFolder);
            }
        }
        public string OutputPath
        {
            get
            {
                return Path.Combine(_rootPath, _outputFolder);
            }
        }
        public string CommonMetadataPath
        {
            get
            {
                return Path.Combine(MetadataPath, DefaultCommonMetadataFolder);
            }
        }
        public string SchemaPath
        {
            get
            {
                return Path.Combine(MetadataPath, DefaultSchemaFolder);
            }
        }

        public string MetadataSearchPattern
        {
            get
            {
                return _metadataSearchPattern ?? DefaultMetadataSearchPattern;
            }
        }
        public bool MetadataSearchSubDirectories
        {
            get
            {
                return _metadataSearchSubDirectories;
            }
        }

        #endregion

        private string _rootPath;
        private string _metadataFolder;
        private string _templateFolder;
        private string _outputFolder;
        private string _metadataSearchPattern;
        private bool _metadataSearchSubDirectories;

        public ABIFileSystemOptions(string rootPath, string metadataFolder, string templateFolder, string outputFolder, string metadataSearchPattern, bool metadataSearchSubDirectories = true)
        {
            _metadataSearchSubDirectories = metadataSearchSubDirectories;
            _metadataSearchPattern = metadataSearchPattern;
            _metadataFolder = metadataFolder;
            _templateFolder = templateFolder;
            _outputFolder = outputFolder;
            _rootPath = Path.GetFullPath(rootPath);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Compile Options:");

            builder.AppendLine($"\tRootPath: { RootPath }" );
            builder.AppendLine($"\tSchemaPath: { SchemaPath }" );
            builder.AppendLine($"\tMetadataPath: { MetadataPath }" );
            builder.AppendLine($"\tOutputPath: { OutputPath }" );
            builder.AppendLine($"\tTemplatePath: { TemplatePath }" );
            builder.AppendLine($"\tCommonMetadataPath: { CommonMetadataPath }" );
            builder.AppendLine($"\tMetadataSearchPattern: { MetadataSearchPattern }" );
            builder.AppendLine($"\tMetadataSearchSubDirectories: { MetadataSearchSubDirectories }" );

            return builder.ToString();
        }
    }
}

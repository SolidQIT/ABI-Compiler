using System.IO;
using System.Text;

namespace SolidQ.ABI.Compiler
{
    public class ABIFileSystemOptions
    {
        #region Consts

        public const string StandardMetadataFileExtension = ".json";
        public const string StandardExtendedMetadataFileExtension = ".ext.json";

        private const string DefaultSchemaFolder = "_schema";
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
            _rootPath = rootPath;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendFormat("Compile Options:\r\n");

            builder.AppendFormat("\tRootPath: {0}\r\n", RootPath);
            builder.AppendFormat("\tSchemaPath: {0}\r\n", SchemaPath);
            builder.AppendFormat("\tMetadataPath: {0}\r\n", MetadataPath);
            builder.AppendFormat("\tOutputPath: {0}\r\n", OutputPath);
            builder.AppendFormat("\tTemplatePath: {0}\r\n", TemplatePath);
            builder.AppendFormat("\tCommonMetadataPath: {0}\r\n", CommonMetadataPath);
            builder.AppendFormat("\tMetadataSearchPattern: {0}\r\n", MetadataSearchPattern);
            builder.AppendFormat("\tMetadataSearchSubDirectories: {0}\r\n", MetadataSearchSubDirectories);

            return builder.ToString();
        }
    }
}

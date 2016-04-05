using CommandLine;

namespace SolidQ.ABI.CommandLine.Options
{
    [Verb("compile", HelpText = "Compile JSON metadata with template files and generate merged results into files")]
    internal class CompileOptions
    {
        [Value(0, Required = false)]
        [Option('s', "search", HelpText = "Search pattern for metadata files")]
        public string MetadataSearchPattern { get; set; }
        
        [Value(1, Required = false)]
        [Option('r', "recursive", HelpText = "Search option for include metadata subdirectories")]
        public string MetadataSearchSubDirectories { get; set; }

        [Value(2, Required = false)]
        [Option('p', "path", HelpText = "FileSystem path containing metadata/template/output folders")]
        public string FileSystemRootPath { get; set; }

        [Value(3, Required = false)]
        [Option('m', "metadata", HelpText = "Metadata folder name")]
        public string MetadataFolderName { get; set; }

        [Value(4, Required = false)]
        [Option('t', "template", HelpText = "Template folder name")]
        public string TemplateFolderName { get; set; }

        [Value(5, Required = false)]
        [Option('o', "output", HelpText = "Output folder name")]
        public string OutputFolderName { get; set; }
    } 
}

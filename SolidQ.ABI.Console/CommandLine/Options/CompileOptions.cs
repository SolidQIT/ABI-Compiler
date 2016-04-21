using CommandLine;

namespace SolidQ.ABI.CommandLine.Options
{
    [Verb("compile", HelpText = "Compile JSON metadata with template files and generate merged results into files")]
    internal class CompileOptions
    {
        [Value(0, Required = false, HelpText = "Search pattern for metadata files")]
        public string MetadataSearchPattern { get; set; }
    } 
}

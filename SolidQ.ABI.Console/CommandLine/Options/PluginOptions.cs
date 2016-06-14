using CommandLine;

namespace SolidQ.ABI.CommandLine.Options
{
    [Verb("plugins", HelpText = "Get information about availble plugins")]
    internal class PluginsOptions
    {
        [Value(0, Required = true, HelpText = "List or Help")]
        public string Action { get; set; }

        [Value(1, Required = false, HelpText = "Plugin Name to be used with the requested action")]
        public string PluginName { get; set; }
    }
}

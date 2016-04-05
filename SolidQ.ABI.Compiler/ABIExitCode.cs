using System.ComponentModel;

namespace SolidQ.ABI.Compiler
{
    public enum ABIExitCode
    {
        [Description("Compile successfully completed")]
        CompileCompleted = 0,

        [Description("Compile completed with warnings, see log for details")]
        CompileCompletedWithWarnings = 1,
        
        [Description("Unhandled exception occurred during compilation")]
        ErrorUnhandledException = 2,

        [Description("Errors occurred during validation, see log for details")]
        ErrorDuringValidation = 3,

        [Description("Engine exit code was unassigned")]
        ErrorExitCodeUnassigned = 4,

        [Description("No metadata files found")]
        ErrorNoMetadataFilesFound = 5,

        [Description("ABI3 section is missing in metadata file")]
        ErrorABISectionMissingInMetadata = 6,

        [Description("Template section is missing in metadata file")]
        ErrorTemplateSectionMissingInMetadata = 7,

        [Description("Template pattern is missing in metadata file")]
        ErrorTemplatePatternMissingInMetadata = 8,

        [Description("Errors occurred during plugin execution, see log for details")]
        ErrorDuringPluginExecution = 9,

        [Description("Template path not existing using source metadata attribute")]
        ErrorTemplatePathSourceDirectoryNotExisting = 10,
    }
}

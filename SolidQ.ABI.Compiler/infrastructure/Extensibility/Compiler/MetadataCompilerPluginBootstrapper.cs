using SolidQ.ABI.Extensibility.Compiler;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace SolidQ.ABI.Compiler.Infrastructure.Extensibility.Compiler
{
    internal class MetadataCompilerPluginBootstrapper
    {
        [ImportMany]
        public IEnumerable<IMetadataCompilerPlugin> MetadataCompilers { get; set; }
    }
}

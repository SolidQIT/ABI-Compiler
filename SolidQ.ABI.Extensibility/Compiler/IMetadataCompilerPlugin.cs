using System;

namespace SolidQ.ABI.Extensibility.Compiler
{
    public interface IMetadataCompilerPlugin : IPlugin
    {
        dynamic Compile(params string[] args);
    }
}

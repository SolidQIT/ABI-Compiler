using System;

namespace SolidQ.ABI.Compiler.Infrastructure
{
    internal class CompilerEngineResult
    {
        public ABIExitCode Code = ABIExitCode.ErrorExitCodeUnassigned;

        public int Warnings = 0;
        public int Artifacts = 0;

        public CompilerEngineResult()
        {
        }
    }
}

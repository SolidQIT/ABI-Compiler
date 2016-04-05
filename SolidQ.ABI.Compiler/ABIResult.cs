using SolidQ.ABI.Compiler.Infrastructure;
using System;

namespace SolidQ.ABI.Compiler
{
    public class ABIResult
    {
        public ABIExitCode ExitCode { get; private set; }

        public string ExitCodeDescription
        {
            get
            {
                return ExitCode.GetDescription();
            }
        }

        public ABIResult(ABIExitCode exitCode)
        {
            ExitCode = exitCode;
        }
    }
}

using System;
using System.Linq   ;
using System.Reflection;

namespace SolidQ.ABI.Compiler
{
    public class ABICompiler
    {
        public static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
        
        #region Properties

        public static string Name { get; } = ExecutingAssembly.GetName().Name;

        public static Version Version { get; } = ExecutingAssembly.GetName().Version;

        public static string Configuration { get; } = ExecutingAssembly.GetCustomAttributes(inherit: true).OfType<AssemblyConfigurationAttribute>().First().Configuration;

        #endregion
    }
}

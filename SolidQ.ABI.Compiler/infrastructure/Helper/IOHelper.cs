using System;
using System.IO;

namespace SolidQ.ABI.Compiler.Infrastructure.Helper
{
    internal static class IOHelper
    {
        /// <summary>
        /// If param "referencePath" is null use Environment.CurrentDirectory
        /// </summary>
        public static string ResolveRelativePath(string relativePath, string referencePath = null)
        {
            if (referencePath == null)
                referencePath = Environment.CurrentDirectory;

            return Path.GetFullPath(Path.Combine(referencePath, relativePath));
        } 
    }
}

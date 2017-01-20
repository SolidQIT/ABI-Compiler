namespace SolidQ.ABI.Compiler.Tests
{
    internal class Utilities
    {
        internal static string Linearize(string value)
        {
            if (value == null)
                return null;

            while (value.IndexOf(" ")  != -1) value = value.Replace(" ",  "");
            while (value.IndexOf("\t") != -1) value = value.Replace("\t", "");
            while (value.IndexOf("\r") != -1) value = value.Replace("\r", "");
            while (value.IndexOf("\n") != -1) value = value.Replace("\n", "");

            return value;
        }
    }
}

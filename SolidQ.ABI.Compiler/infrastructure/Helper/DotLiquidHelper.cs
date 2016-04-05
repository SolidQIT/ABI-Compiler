using DotLiquid;
using System.Collections.Generic;

namespace SolidQ.ABI.Compiler.Infrastructure.Helper
{
    public static class DotLiquidHelper
    {
        public static Hash FromStrictDictionary(this IDictionary<string, object> dictionary)
        {
            Hash result = new Hash(); 

            //Hash result = new Hash((h, k) => { throw new Exception("Unknown variable '" + k + "'"); }); // Removed for now since it make "includes" not working

            foreach (var keyValue in dictionary)
                result.Add(keyValue);

            return result;
        }
    }
}

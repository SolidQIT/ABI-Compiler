using DotLiquid;
using System.Collections.Generic;

namespace SolidQ.ABI.Compiler.Infrastructure.Helper
{
    public static class DotLiquidHelper
    {
        public static Hash FromStrictDictionary(this IDictionary<string, object> dictionary)
        {
            var result = new Hash();

            //var result = new Hash((h, k) => { throw new Exception("Unknown variable '" + k + "'"); }); // Removed for now since it make "includes" not working

            foreach (var item in dictionary)
                result.Add(item);

            return result;
        }
    }
}

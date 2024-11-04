using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGO.ASPNetCore.FileManager.Helpers
{
    public static class EnumHelper
    {
        public static Dictionary<string, int> GetEnumAsDictionary<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                       .Cast<T>()
                       .ToDictionary(e => e.ToString(), e => Convert.ToInt32(e));
        }

        public static Dictionary<string, int> GetEnumListAsDictionary<T>(List<T> values) where T : Enum
        {
            return values.ToDictionary(e => e.ToString(), e => Convert.ToInt32(e));
        }
    }
}

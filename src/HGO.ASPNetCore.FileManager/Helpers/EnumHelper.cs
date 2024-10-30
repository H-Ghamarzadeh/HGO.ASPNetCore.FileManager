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

        //public static void GenerateEnumDefinition<T>(string filePath) where T : Enum
        //{
        //    var enumValues = Enum.GetValues(typeof(T)).Cast<T>();
        //    var sb = new StringBuilder();
        //    sb.AppendLine($"declare const {typeof(T).Name}Enum: {{");

        //    foreach (var value in enumValues)
        //    {
        //        var name = value.ToString();
        //        var intValue = Convert.ToInt32(value);
        //        sb.AppendLine($"    {name}: {intValue};");
        //    }

        //    sb.AppendLine("};");
        //    sb.AppendLine();

        //    File.WriteAllText(filePath, sb.ToString());
        //}

        //public static void GenerateEnumDefinition<T>(string outputPath)
        //{
        //    var enumValues = Enum.GetValues(typeof(T));
        //    var enumDefinition = "export enum Functions {\n";

        //    foreach (var value in enumValues)
        //    {
        //        enumDefinition += $"    {value} = '{value}',\n";
        //    }
        //    enumDefinition += "}\n";

        //    // Write the enum definition to the specified path
        //    File.WriteAllText(outputPath, enumDefinition);
        //}
    }
}

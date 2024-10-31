using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HGO.ASPNetCore.FileManager.Models.LangugageModels
{
    public abstract class LanguageBase
    {
        /// <summary>
        /// if needed...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Encode(string value) => HttpUtility.HtmlEncode(value);
    }
}

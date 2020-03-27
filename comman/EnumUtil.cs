using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WJ.PdfRead.comman
{
    public class EnumUtil
    {
        private static ConcurrentDictionary<Enum, string> _enum2DisplayName = new ConcurrentDictionary<Enum, string>();
        private static ConcurrentDictionary<Enum, string> _enum2ShortName = new ConcurrentDictionary<Enum, string>();
        public static string GetDescription<T>(T enumName) where T:Enum
        {
            var  field = enumName.GetType().GetField(enumName.ToString());
            DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute), false) as DescriptionAttribute;
            return attr.Description;
        }
        public static string GetDisplayName<T>(T enumName, bool returnNullIfNoDisplay = false) where T : Enum
        {
            if (enumName != null)
            {
                return _enum2DisplayName.GetOrAdd(enumName, x =>
                {
                    var field = enumName.GetType().GetField(enumName.ToString());
                    if (field == null)
                    {
                        if (!returnNullIfNoDisplay)
                            return string.Empty;
                        return null;
                    }
                    var customAttribute = field.GetCustomAttribute<DisplayAttribute>(false);
                    if (customAttribute != null)
                        return customAttribute.GetName();
                    return enumName.ToString();
                });
            }
            if (!returnNullIfNoDisplay)
                return string.Empty;
            return null;
        }

    }
}

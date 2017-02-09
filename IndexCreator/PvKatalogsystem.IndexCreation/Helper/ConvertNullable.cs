using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PvKatalogsystem.IndexCreation.Helper
{
    class ConvertNullable
    {
        public static int? ToInt32(long? value)
        {
            if (value.HasValue)
            {
                return Convert.ToInt32(value.Value);
            }
            return null;
        }
    }
}

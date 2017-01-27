using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MMO_RPG_Engine.Helpers
{
    public class ColumnNameHelper
    {
        public static string GetColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
    }
}
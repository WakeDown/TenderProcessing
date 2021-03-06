﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SpeCalc.Helpers
{
    public class DbHelper
    {

        public static string GetValueString(DataRow row, string name)
        {
            if (row.Table.Columns.Contains(name))
            {
                if (row[name] != DBNull.Value) return row[name].ToString();
            }
            return null;
        }

        public static string GetValueStringOrEmpty(DataRow row, string name)
        {
            if (row.Table.Columns.Contains(name))
            {
                if (row[name] != DBNull.Value) return row[name].ToString();
            }
            return String.Empty;
        }

        public static int GetValueIntOrDefault(DataRow row, params string[] names)
        {
            return (from name in names where row.Table.Columns.Contains(name) select GetValueIntOrDefault(row[name])).FirstOrDefault();
        }

        public static int? GetValueIntOrNull(DataRow row, string name)
        {
            if (row.Table.Columns.Contains(name))
            {
                return GetValueIntOrNull(row[name]);
            }
            return null;
        }

        //public static int GetValueInt(DataRow row, string name)
        //{
        //    if (row.Table.Columns.Contains(name))
        //    {
        //        return GetValueInt(row[name]);
        //    }
        //    return null;
        //}

        public static int GetValueInt(object value)
        {
            int result = Convert.ToInt32(value);
            return result;
        }

        public static int? GetValueIntOrNull(object value)
        {
            int? result = null;
            if (value != null && !String.IsNullOrEmpty(value.ToString()))
            {
                result = GetValueInt(value);
            }
            return result;
        }

        public static int GetValueIntOrDefault(object value)
        {
            int? result = GetValueIntOrNull(value);
            return result ?? 0;
        }

        public static decimal GetValueDecimalOrDefault(DataRow row, params string[] names)
        {
            return (from name in names where row.Table.Columns.Contains(name) select GetValueDecimalOrDefault(row[name])).FirstOrDefault();
        }

        public static decimal? GetValueDecimalOrNull(DataRow row, string name)
        {
            if (row.Table.Columns.Contains(name))
            {
                return GetValueDecimalOrNull(row[name]);
            }
            return null;
        }

        public static decimal GetValueDecimal(object value)
        {
            decimal result = Convert.ToDecimal(value);
            return result;
        }

        public static decimal GetValueDecimalOrDefault(object value)
        {
            decimal? result = GetValueDecimalOrNull(value);
            return result ?? 0;
        }

        public static decimal? GetValueDecimalOrNull(object value)
        {
            decimal? result = null;
            if (value != null && !String.IsNullOrEmpty(value.ToString()))
            {
                result = GetValueDecimal(value);
            }
            return result;
        }

        public static decimal? GetValueDecimalOrNull(string value)
        {
            decimal? result = null;

            if (!String.IsNullOrEmpty(value))
            {
                result = Convert.ToDecimal(value);
            }

            return result;
        }

        public static DateTime GetValueDateTimeOrDefault(DataRow row, string name)
        {
            if (row.Table.Columns.Contains(name))
            {
                return GetValueDateTime(row[name]);
            }

            return new DateTime();
        }

        public static DateTime GetValueDateTime(object value)
        {
            DateTime result = Convert.ToDateTime(value);
            return result;
        }

        public static DateTime? GetValueDateTimeOrNull(DataRow row, string name)
        {
            if (row.Table.Columns.Contains(name))
            {
                return GetValueDateTimeOrNull(row[name]);
            }

            return null;
        }

        public static DateTime? GetValueDateTimeOrNull(object value)
        {
            DateTime? result = null;

            if (value != null && !String.IsNullOrEmpty(value.ToString()))
            {
                result = GetValueDateTime(value);
            }

            return result;
        }

        public static bool GetValueBool(object value)
        {
            bool result = false;

            if (!String.IsNullOrEmpty(value.ToString()))
            {
                result = value.ToString().Equals("1");
            }

            return result;
        }

        public static bool GetValueBool(DataRow row, string name)
        {
            bool result = false;
            if (row.Table.Columns.Contains(name) && row[name] != DBNull.Value) result = GetValueBool(row[name]);
            return result;
        }

        public static bool? GetValueBoolOrNull(DataRow row, string name)
        {
            bool result = false;
            if (row.Table.Columns.Contains(name) && row[name] != DBNull.Value) { result = GetValueBool(row[name]); }
            else { return null; }
            return result;
        }

        public static byte[] GetByteArr(DataRow row, string name)
        {
            byte[] result = null;
            if (row.Table.Columns.Contains(name) && row[name] != DBNull.Value) result = GetByteArr(row[name]);
            return result;
        }

        public static byte[] GetByteArr(object value)
        {
            byte[] result = null;

            result = (byte[])value;

            return result;
        }
    }
}
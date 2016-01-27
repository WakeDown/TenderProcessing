using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeCalc.Helpers
{
    public class MainHelper
    {
        public static int GetValueInt(object obj)
        {
            int result = 0;

            try
            {
                result = Convert.ToInt32(obj);
            }
            catch (Exception)
            {
                
            }

            return result;
        }

        public static bool GetValueBool(object obj)
        {
            bool result = false;

            try
            {
                result = Convert.ToBoolean(obj);
            }
            catch (Exception)
            {

            }

            return result;
        }
        

        public static string ShortName(string fullName)
        {
            if (String.IsNullOrEmpty(fullName)) return String.Empty;
            string result = String.Empty;
            string[] nameArr = fullName.Split(' ');
            for (int i = 0; i < nameArr.Count(); i++)
            {
                //if (i > 2) break;
                string name = nameArr[i];
                if (String.IsNullOrEmpty(name)) continue;
                if (i > 0) name = name[0] + ".";
                if (i == 1) name = " " + name;
                result += name;
            }
            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace SpeCalc.Objects
{
    public class Settings
    {
        //private static NetworkCredential nc = GetAdUserCredentials();
        public static NetworkCredential GetAdUserCredentials()
        {
            string accUserName = "UN1T\rehov";
            string accUserPass = "R3xQwi!!";

            string domain = "UN1T";//accUserName.Substring(0, accUserName.IndexOf("\\"));
            string name = "rehov";//accUserName.Substring(accUserName.IndexOf("\\") + 1);

            NetworkCredential nc = new NetworkCredential(name, accUserPass, domain);

            return nc;
        }
    }
}
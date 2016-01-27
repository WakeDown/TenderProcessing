﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace SpeCalcDataAccessLayer.Objects
{
    public class AdUser
    {
        public IPrincipal User { get; set; }
        public string Sid { get; set; }
        public string Login { get; set; }
        private string _fullName;
        public string FullName
        {
            get { return _fullName; }
            set
            {
                _fullName = value;
                DisplayName= ShortName(FullName);
            }
        }

        public string Email { get; set; }

        public string DisplayName { get; set; }
        

        public List<AdGroup> AdGroups { get; set; }


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
        public bool Is(params AdGroup[] groups)
        {
            return groups.Select(grp => AdGroups.Contains(grp)).Any(res => res);
        }

        public bool HasAccess(params AdGroup[] groups)
        {
            if (AdGroups == null || !AdGroups.Any()) return false;
            if (AdGroups.Contains(AdGroup.SuperAdmin) || AdGroups.Contains(AdGroup.SpeCalcKontroler)) return true;
            return groups.Select(grp => AdGroups.Contains(grp)).Any(res => res);
        }

        public bool UserIsAdmin()
        {
            if (String.IsNullOrEmpty(Sid)) return false;
            return HasAccess(AdGroup.SuperAdmin);
        }


    }
}
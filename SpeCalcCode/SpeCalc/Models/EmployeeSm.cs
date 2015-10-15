using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Presentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeCalc.Helpers;
using Stuff.Objects;
using SpeCalcDataAccessLayer.Enums;
using SpeCalcDataAccessLayer.Models;

namespace SpeCalc.Models
{
    public class EmployeeSm : DbModel
    {
        public int Id { get; set; }
        public string AdSid { get; set; }
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string DepartmentName { get; set; }
        public EmployeeSm()
        {
        }
        
        public EmployeeSm(string sid)
        {
            Uri uri = new Uri($"{OdataServiceUri}/Employee/GetSimple?sid={sid}");
            string jsonString = GetJson(uri);

            var empSm = (jsonString != null && jsonString != "{}")
                ? JsonConvert.DeserializeObject<EmployeeSm>(jsonString)
                : null;
            Id = empSm?.Id ?? -1;
            AdSid = empSm?.AdSid;
            DisplayName = empSm?.DisplayName;
            FullName = empSm?.FullName;
            Email = empSm?.Email;
            DepartmentName = empSm?.DepartmentName;
        }

        public UserBase GetUserBase(List<Role> roles)
        {
            var user = new UserBase()
            {
                Id = this?.AdSid,
                Name = this?.FullName,
                ShortName = this?.DisplayName,
                Email = this?.Email,
                Roles = roles
            };
            return user;
        }
    }
}
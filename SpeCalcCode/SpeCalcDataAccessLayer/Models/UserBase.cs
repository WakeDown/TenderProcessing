using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeCalcDataAccessLayer.Enums;

namespace SpeCalcDataAccessLayer.Models
{
    //класс - пользователь из ActiveDirectory 
    public class UserBase
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string Email { get; set; }
        public string ManagerName { get; set; }

        public List<Role> Roles { get; set; }
    }
}

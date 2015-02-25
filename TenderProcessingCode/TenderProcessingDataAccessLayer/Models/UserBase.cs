using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenderProcessingDataAccessLayer.Enums;

namespace TenderProcessingDataAccessLayer.Models
{
    //класс - пользователь из ActiveDirectory 
    public class UserBase
    {

        public string Id { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string Email { get; set; }

        public List<Role> Roles { get; set; }
    }
}

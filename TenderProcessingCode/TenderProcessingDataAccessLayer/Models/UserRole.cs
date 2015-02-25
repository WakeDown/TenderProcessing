using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenderProcessingDataAccessLayer.Enums;

namespace TenderProcessingDataAccessLayer.Models
{
    //класс - роль пользователя
    public class UserRole
    {
        public Role Role { get; set; }

        public string Sid { get; set; }

        public string Name { get; set; }
    }
}

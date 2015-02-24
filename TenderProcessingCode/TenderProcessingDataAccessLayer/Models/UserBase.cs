using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenderProcessingDataAccessLayer.Enums;

namespace TenderProcessingDataAccessLayer.Models
{
    public class UserBase
    {

        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public List<Role> Roles { get; set; }
    }
}

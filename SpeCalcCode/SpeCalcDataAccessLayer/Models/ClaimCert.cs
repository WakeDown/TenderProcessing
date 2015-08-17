using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeCalcDataAccessLayer.Models
{
    public class ClaimCert
    {
        public int Id { get; set; }
        public int IdClaim { get; set; }
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileGuid { get; set; }
    }
}

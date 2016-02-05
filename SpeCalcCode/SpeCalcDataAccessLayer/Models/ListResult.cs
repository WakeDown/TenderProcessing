using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeCalcDataAccessLayer.Models
{
    public class ListResult<T>
    {
        public IEnumerable<T> _list;

        public IEnumerable<T> List
        {
            get { return _list; }
            set
            {
                _list = value;
                ListCount = List.Count();
            }
        }

        public int ListCount { get; set; }
        public int TotalCount { get; set; }
    }
}

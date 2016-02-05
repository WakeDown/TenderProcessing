using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    class ProjectStateModel
    {
        public static ProjectStates GetState(string sysName)
        {
            using(var db = new SpeCalcEntities())
            {
                return db.ProjectStates.Single(x => x.Enabled && x.SysName == "NEW");
            }
        }
    }
}

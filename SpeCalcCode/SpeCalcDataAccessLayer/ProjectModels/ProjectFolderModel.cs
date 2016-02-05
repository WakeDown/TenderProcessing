using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    

    public class ProjectFolderModel
    {
        public ProjectFolders Folder { get; set; }
        public IEnumerable<ProjectFileModel> FileList { get; set; }

        public static IEnumerable<ProjectFolders> GetList()
        {
            using (var db = new SpeCalcEntities())
            {
            return db.ProjectFolders.Where(x => x.Enabled).ToList();
            }
        }

        public static IEnumerable<ProjectFolderModel> GetListWithFiles(int projectId)
        {
            var db = new SpeCalcEntities();
            //using (var db = new SpeCalcEntities())
            //{
            var list = new List<ProjectFolderModel>();
            var folders = GetList();
            var files = db.ProjectFiles.Where(x => x.Enabled && x.ProjectId == projectId && x.PreviousFileId == null).Select(x=>new ProjectFileModel() { FolderId =x.FolderId, FileGUID = x.FileGUID.ToString(), FileName=x.FileName} );
            int i = 0;
            foreach (ProjectFolders fold in folders)
            {
                i++;
                var f = new ProjectFolderModel();
                f.Folder = fold;
                f.FileList = files.Where(x => x.FolderId == fold.Id).ToList();
                list.Add(f);
            }
            return list;
            //}
        }
    }
}

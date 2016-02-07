using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    

    public class ProjectFolderModel
    {
        public int ProjectId { get; set; }
        public ProjectFolders Folder { get; set; }
        public IEnumerable<ProjectFileModel> FileList { get; set; }

        public static IEnumerable<ProjectFolders> GetList()
        {
            using (var db = new SpeCalcEntities())
            {
            return db.ProjectFolders.Where(x => x.Enabled).ToList();
            }
        }

        public static IEnumerable<ProjectFileModel> GetFileList(int projectId, int folderId)
        {
            //using (var db = new SpeCalcEntities())
            //{
            var db = new SpeCalcEntities();
                return db.ProjectFiles.Where(x => x.Enabled && x.ProjectId == projectId && x.FolderId== folderId && x.PreviousFileGUID == null).OrderBy(x=>x.FileName).Select(x => new ProjectFileModel() { FolderId = x.FolderId, FileGUID = x.FileGUID.ToString(), FileName = x.FileName, VersionNumber = x.VersionNumber, CreatorName = x.CreatorName, CreateDate = x.CreateDate }).OrderBy(x=>x.FileName);
            //}
        }

        public static IEnumerable<ProjectFileModel> GetFileListHistory(int projectId, int folderId)
        {
            //using (var db = new SpeCalcEntities())
            //{
            var db = new SpeCalcEntities();
            return db.ProjectFiles.Where(x => x.Enabled && x.ProjectId == projectId && x.FolderId == folderId).OrderBy(x => x.FileName).Select(x => new ProjectFileModel() { FolderId = x.FolderId, FileGUID = x.FileGUID.ToString(), FileName = x.FileName, VersionNumber = x.VersionNumber, CreatorName = x.CreatorName, CreateDate = x.CreateDate, IsLastVersion = x.PreviousFileGUID == null }).OrderBy(x => x.FileName).ThenBy(x => x.VersionNumber);
            //}
        }

        public static IEnumerable<ProjectFolderModel> GetListWithFiles(int projectId)
        {
            var db = new SpeCalcEntities();
            //using (var db = new SpeCalcEntities())
            //{
            
            var list = new List<ProjectFolderModel>();
            var folders = GetList();
            var files = db.ProjectFiles.Where(x => x.Enabled && x.ProjectId == projectId && x.PreviousFileGUID == null).Select(x=>new ProjectFileModel { FolderId =x.FolderId, FileGUID = x.FileGUID.ToString(), FileName=x.FileName, VersionNumber = x.VersionNumber, CreatorName = x.CreatorName, CreateDate = x.CreateDate} );
            int i = 0;
            foreach (ProjectFolders fold in folders)
            {
                i++;
                var f = new ProjectFolderModel();
                f.Folder = fold;
                f.ProjectId = projectId;
                f.FileList = files.Where(x => x.FolderId == fold.Id).OrderBy(x => x.FileName);
                list.Add(f);
            }
            return list;
            //}
        }

        public static IEnumerable<ProjectFolderModel> GetListWithFilesHistory(int projectId)
        {
            var db = new SpeCalcEntities();
            //using (var db = new SpeCalcEntities())
            //{

            var list = new List<ProjectFolderModel>();
            var folders = GetList();
            var files = db.ProjectFiles.Where(x => x.ProjectId == projectId).Select(x => new ProjectFileModel { FolderId = x.FolderId, FileGUID = x.FileGUID.ToString(), FileName = x.FileName, VersionNumber = x.VersionNumber, CreatorName = x.CreatorName, CreateDate = x.CreateDate, IsLastVersion = x.PreviousFileGUID == null, Enabled = x.Enabled, DeleteDate = x.DeleterDate, DeleterName = x.DeleterName});
            int i = 0;
            foreach (ProjectFolders fold in folders)
            {
                i++;
                var f = new ProjectFolderModel();
                f.Folder = fold;
                f.ProjectId = projectId;
                f.FileList = files.Where(x => x.FolderId == fold.Id).OrderBy(x => x.FileName).ThenBy(y=> y.VersionNumber);
                list.Add(f);
            }
            return list;
            //}
        }
    }
}

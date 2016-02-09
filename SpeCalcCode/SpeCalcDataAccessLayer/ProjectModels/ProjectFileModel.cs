using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceClaim.Helpers;
using SpeCalcDataAccessLayer.Models;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class ProjectFileModel
    {
        public int FolderId { get; set; }
        public string FileGUID { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatorName { get; set; }
        public bool IsLastVersion { get; set; }
        public bool Enabled { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string DeleterName { get; set; }
        public int ProjectId { get; set; }

        public static ProjectFiles Get(string guid)
        {
            using (var db = new SpeCalcEntities())
            {
                return db.ProjectFiles.Single(x => x.FileGUID.ToString() == guid);
            }
        }

        public static byte[] GetData(string guid)
        {
            using (var db = new SpeCalcEntities())
            {
                return db.ProjectFiles.Single(x => x.FileGUID.ToString() == guid).fileDATA;
            }
        }

        public static void SaveFile(byte[] data, string fileName, int projectId, int folderId, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                //Если есть файл с таким же названием в этом проекте то значить такой же новый это его новая версия, поэтому заменяем версию
                var curFiles =
                    db.ProjectFiles.Where(
                        x =>
                            x.Enabled && x.PreviousFileGUID == null && x.ProjectId == projectId && x.FolderId == folderId &&
                            x.FileName == fileName).ToList();

                var file = new ProjectFiles();
                file.Enabled = true;
                file.CreateDate = DateTime.Now;
                file.CreatorSid = user.Sid;
                file.CreatorName = user.DisplayName;
                file.ProjectId = projectId;
                file.FolderId = folderId;
                file.FileGUID=Guid.NewGuid();
                file.FileName = fileName;
                file.fileDATA = data;
                file.PreviousFileGUID = null;
                 int maxVer = db.ProjectFiles.Where(x => x.Enabled && x.ProjectId == projectId && x.FolderId == folderId &&
                            x.FileName == fileName).Select(x=>x.VersionNumber).DefaultIfEmpty(0).Max();
                file.VersionNumber = maxVer + 1;
                db.ProjectFiles.Add(file);

                if (curFiles.Count > 0)
                {
                    curFiles.ForEach(x=>x.PreviousFileGUID = file.FileGUID.ToString());
                }
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(projectId, "Добавление файла", $"{file.FileName} v.{file.VersionNumber}", new[] { file }, user);

                string mesg = $"В проекте №{projectId} новый файл<br />";
                    mesg += $" <a href=\"{ProjectHelper.GetFileLink(file.FileGUID.ToString(), projectId)}\">{file.FileName}</a> v.{file.VersionNumber}<br />";
                mesg +=$"<br />Краткая информация о проекте:<br />{ProjectHelper.GetProjectShortInfo(projectId)}<br /><br />Ссылка: {ProjectHelper.GetProjectLink(projectId)}";
                var emails = ProjectTeamModel.GetEmailList(projectId);
                MessageHelper.SendMailSmtpAsync($"[Проект №{projectId}] Новый файл", mesg, true, null, emails.ToArray());
            }
        }

        public static void Delete(string guid, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var file = db.ProjectFiles.Single(x => x.FileGUID.ToString() == guid);
                file.Enabled = false;
                file.DeleterDate = DateTime.Now;
                file.DeleterSid = user.Sid;
                file.DeleterName = user.DisplayName;
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(file.ProjectId, "Удаление файла", $"{file.FileName} v.{file.VersionNumber}", new[] { file }, user);
            }
        }
    }
}

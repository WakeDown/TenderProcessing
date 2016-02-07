using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class ProjectPositionModel
    {
        public ProjectPositions Position { get; set; }
        public bool HasCalculations { get; private set; }
        public List<ProjectPositionCalculations> _calculations; 
        public List<ProjectPositionCalculations> Calculations
        {
            get { return _calculations; }
            set
            {
                _calculations = value;
                HasCalculations = Calculations.Any();
            }
        }

        public ProjectPositionModel()
        {
            Position = new ProjectPositions();
            Calculations=new List<ProjectPositionCalculations>();
        }

        public static int Create(ProjectPositions position, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                position.Enabled = true;
                position.CreatorSid = user.Sid;
                position.CreatorName = user.DisplayName;
                position.CreateDate = DateTime.Now;
                //position.LastChangeDate = DateTime.Now;
                //position.LastChangerSid = user.Sid;
                //position.LastChangerName = user.DisplayName;
                db.ProjectPositions.Add(position);
                SetState(position, user, db);
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(position.ProjectId, "Добавление оборудования", new[] { position }, user);
                return position.Id;
            }
        }

        public static void Save(ProjectPositions position, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var oldpos =db.ProjectPositions.Single(x=>x.Id==position.Id);
                oldpos.CatalogNumber = position.CatalogNumber;
                oldpos.Name = position.Name;
                oldpos.Vendor = position.Vendor;
                oldpos.Quantity = position.Quantity;
                oldpos.QuantityUnitId = position.QuantityUnitId;
                oldpos.CalculatorSid = position.CalculatorSid;
                oldpos.CalculatorName = position.CalculatorName;
                oldpos.LastChangeDate = DateTime.Now;
                oldpos.LastChangerSid = user.Sid;
                oldpos.LastChangerName = user.DisplayName;
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(position.ProjectId, "Изменение оборудования", new[] { oldpos, position }, user);
            }
        }

        public static void Delete(int id, AdUser user, SpeCalcEntities context = null)
        {
            bool hasContext = context != null;
            var db = context ?? new SpeCalcEntities();

            var pos = db.ProjectPositions.Single(x => x.Id == id);
                pos.Enabled = false;
                pos.DeleteDate = DateTime.Now;
                pos.DeleterSid = user.Sid;
                pos.DeleterName = user.DisplayName;
                
            if (!hasContext)
            {
                db.SaveChanges();
                db.Dispose();
            }
            ProjectHistoryModel.CreateHistoryItem(pos.ProjectId, "Удаление оборудования", new[] { pos }, user);
        }

        public static void Delete(int[] ids, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                foreach (var id in ids)
                {
                    Delete(id, user, db);
                }
                db.SaveChanges();
            }
        }

        public static void SetState(ProjectPositions position, AdUser user, SpeCalcEntities context = null)
        {
            bool hasContext = context != null;
            var db = context ?? new SpeCalcEntities();

            position.StateChangeDate = DateTime.Now;
            position.StateChangerSid = user.Sid;
            position.StateChangerName = user.DisplayName;
            position.StateId = ProjectStateModel.GetState("NEW").Id;
            db.ProjectPositions.Add(position);

            if (!hasContext)
            {
                db.SaveChanges();
                db.Dispose();
            }
            ProjectHistoryModel.CreateHistoryItem(position.ProjectId, "Изменение статуса оборудования", new[] { position }, user);
        }

        public static ProjectPositions Get(int id)
        {
            using (var db = new SpeCalcEntities())
            {
                return db.ProjectPositions.SingleOrDefault(x => x.Id == id);
            }
        }

        public static ProjectPositionModel GetWithCalc(int id)
        {
            //using (var db = new SpeCalcEntities())
            //{
                var db = new SpeCalcEntities();
                var position = db.ProjectPositions.Include(x=>x.ProjectPositionStates).Include(x=>x.ProjectPositionStates).Include(x=>x.ProjectPositionQuantityUnits).SingleOrDefault(x => x.Id == id);
                var calculations = db.ProjectPositionCalculations.Where(x => x.Enabled && x.PositionId==id).Include(x => x.ProjectCurrencies).Include(x => x.ProjectProtectionFacts).Include(x => x.ProjectPositionDeliveryTimes).ToList();
                var p = new ProjectPositionModel();
                p.Position = position;
                p.Calculations = calculations.ToList();
                return p;
            //}
        }

        public static IEnumerable<ProjectPositionModel> GetListWithCalc(int projectId, bool? calced=null)
        {
            var db = new SpeCalcEntities();
            //using (var db = new SpeCalcEntities())
            //{

                var list = new List<ProjectPositionModel>();
                var positions = db.ProjectPositions.Where(x => x.Enabled && x.ProjectId == projectId).Where(
                    x=> calced == null || (calced.HasValue && ((calced.Value && x.ProjectPositionCalculations.Any()) || (!calced.Value && !x.ProjectPositionCalculations.Any())))
                    ).ToList();
                var calculations =  db.ProjectPositionCalculations.Where(x => x.Enabled && x.ProjectPositions.ProjectId == projectId).Include(x => x.ProjectCurrencies).Include(x => x.ProjectProtectionFacts).Include(x => x.ProjectPositionDeliveryTimes).ToList();
                int i = 0;
                foreach (ProjectPositions pos in positions)
                {
                    i++;
                    pos.RowNum = i.ToString();
                    var p = new ProjectPositionModel();
                    p.Position = pos;
                    p.Calculations = calculations.Where(x=>x.PositionId==pos.Id).ToList();
                    list.Add(p);
                }
                return list;
            //}
            
        } 
    }
}

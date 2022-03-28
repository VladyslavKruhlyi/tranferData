using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DrxTransfer;
using DrxTransfer.Engine;
using Newtonsoft.Json.Linq;
using Sungero.Domain.Shared;

namespace TranferSerializers
{
    [Export(typeof(SungeroSerializer))]
    class CuratorSerializer : SungeroSerializer
    {
        public CuratorSerializer() : base()
        {
            this.EntityName = "Curator";
            this.EntityTypeName = "company.Additions.ICurator";
        }

        public override IEnumerable<IEntity> Filter(IEnumerable<IEntity> entities)
        {
            return entities.Cast<company.Additions.ICurator>().Where(c => c.Status == company.Additions.Curator.Status.Active);
        }
        public override void Import(Dictionary<string, object> content)
        {
            var entityItem = content["Card"] as JObject;
            var employeeItem = content["Employee"] as JObject;
            var employeeName = employeeItem.Property("Name").Value.ToString();
            var employee = Session.Current.GetEntities("Sungero.Company.IEmployee").Cast<Sungero.Company.IEmployee>()
              .FirstOrDefault(g => g.Name == employeeName);
            var activeCurator = Session.Current.GetEntities(this.EntityTypeName).Cast<company.Additions.ICurator>()
               .Where(k => k.Employee.Name == employeeName).FirstOrDefault();
            company.Additions.ICurator сurator = null;
            
            if (activeCurator != null)
            {
                сurator = activeCurator;
                Log.Console.Info(string.Format("ИД = {0}. Обновление записи справочника Кураторы для {1}", сurator.Id, employeeName));
            }
            else
            {
                сurator = Session.Current.CreateEntity(this.EntityTypeName) as company.Additions.ICurator;
                Log.Console.Info(string.Format("ИД = {0}. Создание записи справочника Кураторы для {1}", сurator.Id, employeeName));
            }
            
            if (сurator != null)
            {
                сurator.Status = company.Additions.Curator.Status.Active;
                сurator.Employee = employee;
                сurator.Save();
                Session.Current.SubmitChanges();
            }
        }

        protected override Dictionary<string, object> Export(IEntity entity)
        {
            base.Export(entity);
            var curator = entity as company.Additions.ICurator;
            content["Employee"] = curator.Employee;
            return content;
        }
    }
}

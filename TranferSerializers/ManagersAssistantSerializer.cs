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
    class ManagersAssistantSerializer : SungeroSerializer
    {
        public ManagersAssistantSerializer() : base()
        {
            this.EntityName = "ManagersAssistant";
            this.EntityTypeName = "Sungero.Company.IManagersAssistant";
        }

        public override IEnumerable<IEntity> Filter(IEnumerable<IEntity> entities)
        {
            return entities.Cast<Sungero.Company.IManagersAssistant>().Where(c => c.Status == Sungero.Company.ManagersAssistant.Status.Active);
        }
        public override void Import(Dictionary<string, object> content)
        {
            var entityItem = content["Card"] as JObject;
            var employeeItem = content["Assistant"] as JObject;
            var employeeName = employeeItem.Property("Name").Value.ToString();
            var employee = Session.Current.GetEntities("Sungero.Company.IEmployee").Cast<Sungero.Company.IEmployee>()
              .FirstOrDefault(g => g.Name == employeeName);
            var managerItem = content["Manager"] as JObject;
            var managerName = managerItem.Property("Name").Value.ToString();
            var manager = Session.Current.GetEntities("Sungero.Company.IEmployee").Cast<Sungero.Company.IEmployee>()
              .FirstOrDefault(g => g.Name == managerName);
            var activeManagersAssistant = Session.Current.GetEntities(this.EntityTypeName).Cast<Sungero.Company.IManagersAssistant>()
               .Where(k => k.Assistant.Name == employeeName && k.Manager.Name == managerName).FirstOrDefault();

            Sungero.Company.IManagersAssistant managersAssistant = null;
            
            if (activeManagersAssistant != null)
            {
                managersAssistant = activeManagersAssistant;
                Log.Console.Info(string.Format("ИД = {0}. Обновление записи справочника Помощники руководителей для {1}", managersAssistant.Id, managerName));
            }
            else
            {
                managersAssistant = Session.Current.CreateEntity(this.EntityTypeName) as Sungero.Company.IManagersAssistant;
                Log.Console.Info(string.Format("ИД = {0}. Создание записи справочника Помощники руководителей для {1}", managersAssistant.Id, managerName));
            }
            
            if (managersAssistant != null)
            {
                managersAssistant.Status = company.Additions.Curator.Status.Active;
                managersAssistant.Manager = manager;
                managersAssistant.Assistant = employee;
                managersAssistant.PreparesResolution = entityItem.Property("PreparesResolution").ToObject<bool?>();
                managersAssistant.Save();
                Session.Current.SubmitChanges();
            }
        }

        protected override Dictionary<string, object> Export(IEntity entity)
        {
            base.Export(entity);
            var managersAssistant = entity as Sungero.Company.IManagersAssistant;
            content["Manager"] = managersAssistant.Manager;
            content["Assistant"] = managersAssistant.Assistant;
            return content;
        }
    }
}

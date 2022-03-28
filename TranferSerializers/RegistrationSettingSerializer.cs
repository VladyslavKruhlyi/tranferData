﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DrxTransfer;
using DrxTransfer.Engine;
using Newtonsoft.Json.Linq;
using Sungero.Domain.Shared;

namespace TranferSerializers
{
    [Export(typeof(SungeroSerializer))]
    class RegistrationSettingSerializer : SungeroSerializer
    {
        public RegistrationSettingSerializer() : base()
        {
            this.EntityName = "RegistrationSetting";
            this.EntityTypeName = "Sungero.Docflow.IRegistrationSetting";
        }

        public override IEnumerable<IEntity> Filter(IEnumerable<IEntity> entities)
        {
            return entities.Cast<Sungero.Docflow.IRegistrationSetting>();
        }

        public override void Import(Dictionary<string, object> content)
        {
            var entityItem = content["Card"] as JObject;
            var settingName = entityItem.Property("Name").Value.ToString();
            var documentFlow = entityItem.Property("DocumentFlow").Value.ToString();
            var settingType = entityItem.Property("SettingType").Value.ToString();

            var activeRegistrationSetting = Session.Current.GetEntities(this.EntityTypeName).Cast<Sungero.Docflow.IRegistrationSetting>()
               .Where(k => k.DocumentFlow.Value.ToString() == documentFlow &&
               k.SettingType.Value.ToString() == settingType &&
               k.Name.Contains(settingName)).FirstOrDefault();

            var statusName = entityItem.Property("Status").Value.ToString();
            Sungero.Docflow.IRegistrationSetting registrationSetting = null;
            
            if (activeRegistrationSetting != null)
            {
                registrationSetting = activeRegistrationSetting;
                Log.Console.Info(string.Format("ИД = {0}. Обновление настройки регистрации {1}", registrationSetting.Id, settingName));
            }
            else
            {
                if (statusName == "Active")
                {
                    registrationSetting = Session.Current.CreateEntity(this.EntityTypeName) as Sungero.Docflow.IRegistrationSetting;
                    Log.Console.Info(string.Format("ИД = {0}. Создание настройки регистрации {1}", registrationSetting.Id, settingName));
                }
            }

            if (registrationSetting != null)
            {
                if (statusName == "Active")
                    registrationSetting.Status = Sungero.Docflow.DocumentKind.Status.Active;
                else
                    registrationSetting.Status = Sungero.Docflow.DocumentKind.Status.Closed;

                registrationSetting.Name = settingName;
                registrationSetting.DocumentFlow = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.DocumentRegister.DocumentFlow)).FirstOrDefault(e => e.Value == documentFlow);
                registrationSetting.SettingType = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.RegistrationSetting.SettingType)).FirstOrDefault(e => e.Value == settingType);

                Log.Console.Info("Заполнение видов документов");
                var documentKinds = SungeroRepository.GetEntities<Sungero.Docflow.IDocumentKind>(content, "DocumentKinds", false, true);
                registrationSetting.DocumentKinds.Clear();
                
                foreach (var documentKind in documentKinds)
                {
                    var documentKindItem = registrationSetting.DocumentKinds.AddNew();
                    documentKindItem.DocumentKind = documentKind;
                }

                Log.Console.Info("Заполнение НОР");
                var businessUnits = SungeroRepository.GetEntities<Sungero.Company.IBusinessUnit>(content, "BusinessUnits", false, true);
                registrationSetting.BusinessUnits.Clear();
                
                foreach (var businessUnit in businessUnits)
                {
                    var businessUnitItem = registrationSetting.BusinessUnits.AddNew();
                    businessUnitItem.BusinessUnit = businessUnit;
                }

                Log.Console.Info("Заполнение подразделений");
                var departments = SungeroRepository.GetEntities<Sungero.Company.IDepartment>(content, "Departments", true, true);
                registrationSetting.Departments.Clear();
                
                foreach (var department in departments)
                {
                    var departmentItem = registrationSetting.Departments.AddNew();
                    departmentItem.Department = department;
                }

                var documentRegisterItem = content["DocumentRegister"] as JObject;
                var documentRegisterName = documentRegisterItem.Property("Name").Value.ToString();
                var documentRegister = Session.Current.GetEntities("Sungero.Docflow.IDocumentRegister").Cast<Sungero.Docflow.IDocumentRegister>()
                  .FirstOrDefault(g => g.Name == documentRegisterName);
                
                if (documentRegister != null)
                    registrationSetting.DocumentRegister = documentRegister;
                else
                    throw new System.IO.InvalidDataException(string.Format("Журнал регистрации {0} не найден", documentRegisterName));

                registrationSetting.Save();
                Session.Current.SubmitChanges();
            }
        }

        protected override Dictionary<string, object> Export(IEntity entity)
        {
            base.Export(entity);
            var registrationSetting = entity as Sungero.Docflow.IRegistrationSetting;
            content["DocumentRegister"] = registrationSetting.DocumentRegister;
            content["DocumentKinds"] = registrationSetting.DocumentKinds.Select(k => k.DocumentKind);
            content["BusinessUnits"] = registrationSetting.BusinessUnits.Select(u => u.BusinessUnit);
            content["Departments"] = registrationSetting.Departments.Select(d => d.Department);
            return content;
        }
    }
}

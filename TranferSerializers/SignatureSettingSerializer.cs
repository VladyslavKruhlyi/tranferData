
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
    class SignatureSettingSerializer : SungeroSerializer
    {
        public SignatureSettingSerializer() : base()
        {
            this.EntityName = "SignatureSetting";
            this.EntityTypeName = "Sungero.Docflow.ISignatureSetting";
        }

        public override IEnumerable<IEntity> Filter(IEnumerable<IEntity> entities)
        {
            return entities.Cast<Sungero.Docflow.ISignatureSetting>().Where(c => c.Status == Sungero.Docflow.SignatureSetting.Status.Active);
        }

        public override void Import(Dictionary<string, object> content)
        {
            var entityItem = content["Card"] as JObject;
            var recipientItem = content["Recipient"] as JObject;
            var recipientName = recipientItem.Property("Name").Value.ToString();
            var recipient = Session.Current.GetEntities("Sungero.CoreEntities.IRecipient").Cast<Sungero.CoreEntities.IRecipient>()
              .FirstOrDefault(t => t.Name == recipientName && t.Status == Sungero.CoreEntities.Recipient.Status.Active);
            
            var statusName = entityItem.Property("Status").Value.ToString();
            var activeSignatureSetting = Session.Current.GetEntities(this.EntityTypeName).Cast<Sungero.Docflow.ISignatureSetting>()
               .Where(k => k.Recipient.Name == recipientName && k.Status == Sungero.Docflow.SignatureSetting.Status.Active).FirstOrDefault();

            Sungero.Docflow.ISignatureSetting signatureSetting = null;
            if (activeSignatureSetting != null)
            {
                signatureSetting = activeSignatureSetting;
                Log.Console.Info(string.Format("ИД = {0}. Обновление права подписи для {1}", signatureSetting.Id, recipientName));
            }
            else
            {
                if (statusName == "Active")
                {
                    signatureSetting = Session.Current.CreateEntity(this.EntityTypeName) as Sungero.Docflow.ISignatureSetting;
                    Log.Console.Info(string.Format("ИД = {0}. Создание права подписи для {1}", signatureSetting.Id, recipientName));
                }
            }
            if (signatureSetting != null)
            {
                if (statusName == "Active")
                {
                    if (recipient != null)
                        signatureSetting.Recipient = recipient;
                    else
                        throw new System.IO.InvalidDataException(string.Format("Подписант {0} не найден", recipientName));

                    signatureSetting.Status = Sungero.Docflow.DocumentKind.Status.Active;
                    var reason = entityItem.Property("Reason").Value.ToString();
                    signatureSetting.Reason = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.SignatureSetting.Reason)).FirstOrDefault(e => e.Value == reason);
                    signatureSetting.DocumentInfo = entityItem.Property("DocumentInfo").Value.ToString();
                    var validFrom = entityItem.Property("ValidFrom").Value.ToString();
                    
                    if (!string.IsNullOrEmpty(validFrom))    
                        signatureSetting.ValidFrom = System.Convert.ToDateTime(validFrom);
                    
                    var validTill = entityItem.Property("ValidTill").Value.ToString();
                    
                    if (!string.IsNullOrEmpty(validTill))
                        signatureSetting.ValidTill = System.Convert.ToDateTime(validTill);
                    
                    signatureSetting.Priority = entityItem.Property("Priority").ToObject<int?>();
                    var status = entityItem.Property("Status").ToObject<string>();
                    
                    if (!string.IsNullOrEmpty(status))
                        signatureSetting.Status = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.SignatureSetting.Status)).FirstOrDefault(e => e.Value == status);
                    
                    var docflow = entityItem.Property("DocumentFlow").ToObject<string>();
                    
                    if (!string.IsNullOrEmpty(docflow))
                        signatureSetting.DocumentFlow = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.SignatureSetting.DocumentFlow)).FirstOrDefault(e => e.Value == docflow);
                    
                    var businessUnits = SungeroRepository.GetEntities<company.Head.IBusinessUnit>(content, "BusinessUnits", true, true);
                    
                    if (businessUnits.Any())
                    {
                        signatureSetting.BusinessUnits.Clear();
                        
                        foreach (var businessUnit in businessUnits)
                        {
                            var businessUnitItem = signatureSetting.BusinessUnits.AddNew();
                            businessUnitItem.BusinessUnit = businessUnit;
                        }
                    }

                    var documentKinds = SungeroRepository.GetEntities<Sungero.Docflow.IDocumentKind>(content, "DocumentKinds", true, true);                  
                    
                    if (documentKinds.Any())
                    {
                        signatureSetting.DocumentKinds.Clear();
                        
                        foreach (var documentKind in documentKinds)
                        {
                            var documentKindItem = signatureSetting.DocumentKinds.AddNew();
                            documentKindItem.DocumentKind = documentKind;
                        }
                    }

                    var departments = SungeroRepository.GetEntities<company.Head.IDepartment>(content, "Departments", false, true);
                    
                    if (departments.Any())
                    {
                        signatureSetting.Departments.Clear();
                        
                        foreach (var department in departments)
                        {
                            var departmentItem = signatureSetting.Departments.AddNew();
                            departmentItem.Department = department;
                        }
                    }
                    var categories = SungeroRepository.GetEntities<Sungero.Docflow.IDocumentGroupBase>(content, "Categories", false, true);
                    
                    if (categories.Any())
                    {
                        signatureSetting.Categories.Clear();
                        foreach (var category in categories)
                        {
                            var categoryItem = signatureSetting.Categories.AddNew();
                            categoryItem.Category = category;
                        }
                    }
                    
                    var limit = entityItem.Property("Limit").Value.ToString();
                    
                    if (!string.IsNullOrEmpty(limit))
                        signatureSetting.Limit = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.SignatureSetting.Limit)).FirstOrDefault(e => e.Value == limit);
                    
                    signatureSetting.Amount = entityItem.Property("Amount").ToObject<int?>();
                    signatureSetting.Note = entityItem.Property("Note").ToObject<string>();
                    signatureSetting.Save();
                    Session.Current.SubmitChanges();
                }
            }
        }

        protected override Dictionary<string, object> Export(IEntity entity)
        {
            base.Export(entity);
            var signatureSetting = (entity as Sungero.Docflow.ISignatureSetting);
            content["Recipient"] = signatureSetting.Recipient;
            content["Document"] = signatureSetting.Document;
            content["BusinessUnits"] = signatureSetting.BusinessUnits.Select(c => c.BusinessUnit);
            content["DocumentKinds"] = signatureSetting.DocumentKinds.Select(c => c.DocumentKind);
            content["Departments"] = signatureSetting.Departments.Select(c => c.Department);
            content["Categories"] = signatureSetting.Categories.Select(c => c.Category);
            content["Currency"] = signatureSetting.Currency;
            return content;
        }
    }
}

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
    class ApprovalMatrixSerializer : SungeroSerializer
    {
        public ApprovalMatrixSerializer() : base()
        {
            this.EntityName = "ApprovalMatrix";
            this.EntityTypeName = "company.Additions.IApprovalMatrix";
        }

        public override void Import(Dictionary<string, object> content)
        {
            var entityItem = content["Card"] as JObject;
            var documentKindItem = content["DocumentKind"] as JObject;
            var documentKindName = documentKindItem.Property("Name").Value.ToString();
            var documentKind = Session.Current.GetEntities("Sungero.Docflow.IDocumentKind").Cast<Sungero.Docflow.IDocumentKind>()
              .FirstOrDefault(g => g.Name == documentKindName);
            var documentGroupItem = content["DocumentGroup"] as JObject;
            var documentGroupName = documentGroupItem.Property("Name").Value.ToString();
            var documentGroup = Session.Current.GetEntities("Sungero.Docflow.IDocumentGroupBase").Cast<Sungero.Docflow.IDocumentGroupBase>()
              .FirstOrDefault(g => g.Name == documentGroupName);
            var activeApprovalRecord = Session.Current.GetEntities(this.EntityTypeName).Cast<company.Additions.IApprovalMatrix>()
               .Where(k => k.DocumentKind.Name == documentKindName && k.DocumentGroup.Name == documentGroupName).FirstOrDefault();

            company.Additions.IApprovalMatrix approvalRecord = null;
            
            if (activeApprovalRecord != null)
            {
                approvalRecord = activeApprovalRecord;
                Log.Console.Info(string.Format("ИД = {0}. Обновление записи справочника Матрица согласования для вида {1}", approvalRecord.Id, documentKindName));
            }
            else
            {
                approvalRecord = Session.Current.CreateEntity(this.EntityTypeName) as company.Additions.IApprovalMatrix;
                Log.Console.Info(string.Format("ИД = {0}. Создание записи справочника Матрица согласования для вида {1}", approvalRecord.Id, documentKindName));
            }

            if (approvalRecord != null)
            {
                approvalRecord.Status = company.Additions.ApprovalMatrix.Status.Active;
                approvalRecord.DocumentKind = documentKind;
                approvalRecord.DocumentGroup = documentGroup;
                
                var recipientsStage1 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage1", true, true);
                
                if (recipientsStage1.Any())
                {
                    approvalRecord.RecipientsStage1.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage1.AddNew();
                    recipientsStageItem.Recipient = recipientsStage1.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage1", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage1.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage1.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }
                
                var recipientsStage2 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage2", true, true);
                
                if (recipientsStage2.Any())
                {
                    approvalRecord.RecipientsStage2.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage2.AddNew();
                    recipientsStageItem.Recipient = recipientsStage2.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage2", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage2.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage2.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }
                
                var recipientsStage3 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage3", true, true);
                
                if (recipientsStage3.Any())
                {
                    approvalRecord.RecipientsStage3.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage3.AddNew();
                    recipientsStageItem.Recipient = recipientsStage3.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage3", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage3.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage3.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }
                
                var recipientsStage4 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage4", true, true);
                
                if (recipientsStage4.Any())
                {
                    approvalRecord.RecipientsStage4.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage4.AddNew();
                    recipientsStageItem.Recipient = recipientsStage4.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage4", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage4.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage4.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }
                
                var recipientsStage5 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage5", true, true);
                
                if (recipientsStage5.Any())
                {
                    approvalRecord.RecipientsStage5.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage5.AddNew();
                    recipientsStageItem.Recipient = recipientsStage5.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage5", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage5.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage5.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }
                
                var recipientsStage6 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage6", true, true);
               
                if (recipientsStage6.Any())
                {
                    approvalRecord.RecipientsStage6.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage6.AddNew();
                    recipientsStageItem.Recipient = recipientsStage6.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage6", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage6.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage6.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }

                var recipientsStage7 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage7", true, true);
                
                if (recipientsStage7.Any())
                {
                    approvalRecord.RecipientsStage7.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage7.AddNew();
                    recipientsStageItem.Recipient = recipientsStage7.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage7", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage7.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage7.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }

                var recipientsStage8 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage8", true, true);
                
                if (recipientsStage8.Any())
                {
                    approvalRecord.RecipientsStage8.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage8.AddNew();
                    recipientsStageItem.Recipient = recipientsStage8.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage8", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage8.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage8.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }

                var recipientsStage9 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage9", true, true);
                
                if (recipientsStage9.Any())
                {
                    approvalRecord.RecipientsStage9.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage9.AddNew();
                    recipientsStageItem.Recipient = recipientsStage9.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage9", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage9.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage9.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }

                var recipientsStage10 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage10", true, true);
                
                if (recipientsStage10.Any())
                {
                    approvalRecord.RecipientsStage10.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage10.AddNew();
                    recipientsStageItem.Recipient = recipientsStage10.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage10", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage10.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage10.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }

                var recipientsStage11 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage11", true, true);
                
                if (recipientsStage11.Any())
                {
                    approvalRecord.RecipientsStage11.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage11.AddNew();
                    recipientsStageItem.Recipient = recipientsStage11.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage11", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage11.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage11.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }
                
                var recipientsStage12 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage12", true, true);
                
                if (recipientsStage12.Any())
                {
                    approvalRecord.RecipientsStage12.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage12.AddNew();
                    recipientsStageItem.Recipient = recipientsStage12.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage12", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage12.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage12.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }

                var recipientsStage13 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage13", true, true);
                
                if (recipientsStage13.Any())
                {
                    approvalRecord.RecipientsStage13.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage13.AddNew();
                    recipientsStageItem.Recipient = recipientsStage13.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage13", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage13.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage13.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }

                var recipientsStage14 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage14", true, true);
                
                if (recipientsStage14.Any())
                {
                    approvalRecord.RecipientsStage14.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage14.AddNew();
                    recipientsStageItem.Recipient = recipientsStage14.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage14", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage14.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage14.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }

                var recipientsStage15 = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientsStage15", true, true);
                
                if (recipientsStage15.Any())
                {
                    approvalRecord.RecipientsStage15.Clear();
                    var recipientsStageItem = approvalRecord.RecipientsStage15.AddNew();
                    recipientsStageItem.Recipient = recipientsStage15.LastOrDefault();
                }
                else
                {
                    var recipientsrole = SungeroRepository.GetEntities<Sungero.CoreEntities.IRole>(content, "RecipientsStage15", true, true);
                    if (recipientsrole.Any())
                    {
                        approvalRecord.RecipientsStage15.Clear();
                        var recipientsStageItem = approvalRecord.RecipientsStage15.AddNew();
                        recipientsStageItem.Recipient = recipientsrole.LastOrDefault();
                    }
                }

                approvalRecord.DocumentKind = documentKind;
                approvalRecord.DocumentGroup = documentGroup;
                approvalRecord.Save();
                Session.Current.SubmitChanges();
            }

        }

        protected override Dictionary<string, object> Export(IEntity entity)
        {
            base.Export(entity);
            var role = entity as company.Additions.IApprovalMatrix;
            content["RecipientsStage1"] = role.RecipientsStage1.Select(l => l.Recipient);
            content["RecipientsStage2"] = role.RecipientsStage2.Select(l => l.Recipient);
            content["RecipientsStage3"] = role.RecipientsStage3.Select(l => l.Recipient);
            content["RecipientsStage4"] = role.RecipientsStage4.Select(l => l.Recipient);
            content["RecipientsStage5"] = role.RecipientsStage5.Select(l => l.Recipient);
            content["RecipientsStage6"] = role.RecipientsStage6.Select(l => l.Recipient);
            content["RecipientsStage7"] = role.RecipientsStage7.Select(l => l.Recipient);
            content["RecipientsStage8"] = role.RecipientsStage8.Select(l => l.Recipient);
            content["RecipientsStage9"] = role.RecipientsStage9.Select(l => l.Recipient);
            content["RecipientsStage10"] = role.RecipientsStage10.Select(l => l.Recipient);
            content["RecipientsStage11"] = role.RecipientsStage11.Select(l => l.Recipient);
            content["RecipientsStage12"] = role.RecipientsStage12.Select(l => l.Recipient);
            content["RecipientsStage13"] = role.RecipientsStage13.Select(l => l.Recipient);
            content["RecipientsStage14"] = role.RecipientsStage14.Select(l => l.Recipient);
            content["RecipientsStage15"] = role.RecipientsStage15.Select(l => l.Recipient);
            content["RecipientsStage15"] = role.RecipientsStage15.Select(l => l.Recipient);
            content["DocumentKind"] = role.DocumentKind;
            content["DocumentGroup"] = role.DocumentGroup;
            return content;
        }
    }
}

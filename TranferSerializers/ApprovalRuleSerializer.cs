﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DrxTransfer;
using DrxTransfer.Engine;
using Newtonsoft.Json.Linq;
using Sungero.Docflow;
using Sungero.Domain.Shared;

namespace TranferSerializers
{
    [Export(typeof(SungeroSerializer))]
    class ApprovalRuleSerializer : SungeroSerializer
    {
        public ApprovalRuleSerializer() : base()
        {
            this.EntityName = "ApprovalRule";
            this.EntityTypeName = "Sungero.Docflow.IApprovalRuleBase";
        }

        struct Stage
        {
            public int Number;
            public object StageObject;
            public object ApprovalRole;
            public object Assignee;
            public object ReworkPerformer;
            public object ReworkApprovalRole;
            public System.Type FunctionsType;
            public Sungero.Core.Enumeration? StageTypeRule;
            public IEnumerable<object> ApprovalRoles;
            public IEnumerable<object> Recipients;
        }

        struct Condition
        {
            public int Number;
            public object ConditionObject;
        }

        struct Transitions
        {
            public int? SourceStage;
            public int? TargetStage;
            public bool? ConditionValue;
        }

        public override void Import(Dictionary<string, object> content)
        {
            var entityItem = content["Card"] as JObject;
            var documentFlow = entityItem.Property("DocumentFlow").Value.ToString();
            var isContractsRule = documentFlow == "Contracts";
            var ruleName = entityItem.Property("Name").Value.ToString();
            Sungero.Docflow.IApprovalRuleBase rule = null;

            // Получение параметров правила.
            Log.Console.Info("Поиск видов документов");
            var documentKinds = SungeroRepository.GetEntities<Sungero.Docflow.IDocumentKind>(content, "DocumentKinds", false, true);
            Log.Console.Info("Поиск НОР");
            var businessUnits = SungeroRepository.GetEntities<Sungero.Company.IBusinessUnit>(content, "BusinessUnit", false, true);
            Log.Console.Info("Поиск подразделений");
            var departments = SungeroRepository.GetEntities<company.Head.IDepartment>(content, "Departments", false, true);
            Log.Console.Info("Поиск категорий договоров");
            var documentGroups = SungeroRepository.GetEntities<Sungero.Docflow.IDocumentGroupBase>(content, "DocumentGroup", false, true);
            #region Поиск существующих правил.

            var allRules = Session.Current.GetEntities(this.EntityTypeName).Cast<Sungero.Docflow.IApprovalRuleBase>()
                     .Where(r => r.DocumentFlow.Value.ToString() == documentFlow && r.Status == Sungero.Docflow.ApprovalRuleBase.Status.Active).ToList();

            // TODO: Сделать возможность создания версии правила через ключ командной строки.
            var activeRule = allRules.FirstOrDefault(r => r.Name == ruleName);
            
            if (activeRule != null)
            {
                rule = activeRule;
            }

            #endregion

            #region Создание нового правила.

            if (rule == null)
            {
                if (isContractsRule)
                    rule = Session.Current.CreateEntity("Sungero.Contracts.IContractsApprovalRule") as Sungero.Contracts.IContractsApprovalRule;
                else
                    rule = Session.Current.CreateEntity("Sungero.Docflow.IApprovalRule") as Sungero.Docflow.IApprovalRule;

                Log.Console.Info(string.Format("ИД = {0}. Создание правила согласования {1}", rule.Id, ruleName));
            }

            #endregion

            #region Заполнение карточки.

            rule.Name = ruleName;
            rule.DocumentFlow = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.ApprovalRule.DocumentFlow)).FirstOrDefault(e => e.Value == documentFlow);
            rule.IsSmallApprovalAllowed = entityItem.Property("IsSmallApprovalAllowed").ToObject<bool>();
            rule.Priority = entityItem.Property("Priority").ToObject<int>();
            rule.NeedRestrictInitiatorRights = entityItem.Property("NeedRestrictInitiatorRights").ToObject<bool>();
            rule.ReworkDeadline = entityItem.Property("ReworkDeadline").ToObject<int?>();
            var reworkPerformerType = entityItem.Property("ReworkPerformerType").Value.ToString();
            if (!string.IsNullOrEmpty(reworkPerformerType))
                rule.ReworkPerformerType = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.ApprovalRule.ReworkPerformerType)).FirstOrDefault(e => e.Value == reworkPerformerType);
            #endregion

            #region Заполнение параметров. 
            var reworkPerformerItem = content["ReworkPerformer"] as JObject;
            if (reworkPerformerItem != null)
            {
                var reworkPerformerItemName = reworkPerformerItem.Property("Name").Value.ToString();
                var reworkPerformer = Session.Current.GetEntities("Sungero.CoreEntities.IRecipient").Cast<Sungero.CoreEntities.IRecipient>()
                  .FirstOrDefault(e => e.Name == reworkPerformerItemName && e.Status == Sungero.CoreEntities.Recipient.Status.Active);
                if (reworkPerformer != null)
                    rule.ReworkPerformer = reworkPerformer;
                else
                    throw new System.IO.InvalidDataException(string.Format("Роль/сотрудник {0} не найден", reworkPerformerItemName));
            }

            var reworkApprovalRoleItem = content["ReworkApprovalRole"] as JObject;
            if (reworkApprovalRoleItem != null)
            {
                var reworkApprovalRoleItemName = reworkApprovalRoleItem.Property("Name").Value.ToString();
                var reworkApprovalRole = Session.Current.GetEntities("Sungero.Docflow.IApprovalRoleBase").Cast<Sungero.Docflow.IApprovalRoleBase>()
                  .FirstOrDefault(e => e.Name == reworkApprovalRoleItemName && e.Status == Sungero.CoreEntities.Recipient.Status.Active);
                if (reworkApprovalRole != null)
                    rule.ReworkApprovalRole = reworkApprovalRole;
                else
                    throw new System.IO.InvalidDataException(string.Format("Роль согласования {0} не найдена", reworkApprovalRoleItemName));
            }

            foreach (var documentKind in documentKinds)
            {
                var documentKindItem = rule.DocumentKinds.AddNew();
                documentKindItem.DocumentKind = documentKind;
            }

            foreach (var businessUnit in businessUnits)
            {
                var businessUnitItem = rule.BusinessUnits.AddNew();
                businessUnitItem.BusinessUnit = businessUnit;
            }

            foreach (var department in departments)
            {
                var departmentItem = rule.Departments.AddNew();
                departmentItem.Department = department;
            }

            if (isContractsRule)
            {
                foreach (var documentGroup in documentGroups)
                {
                    var documentGroupItem = rule.DocumentGroups.AddNew();
                    documentGroupItem.DocumentGroup = documentGroup;
                }
            }

            #endregion

            #region Создание схемы.

            #region Создание условий.

            var conditionsArray = content["Conditions"] as JArray;
            var conditions = conditionsArray.ToObject<List<Condition>>();

            foreach (var condition in conditions)
            {
                var conditionEntity = Sungero.Docflow.ConditionBases.Null;
                
                if (isContractsRule)
                    conditionEntity = Session.Current.CreateEntity("company.Head.IContractCondition") as company.Head.IContractCondition;
                else
                    conditionEntity = Session.Current.CreateEntity("company.Head.ICondition") as company.Head.ICondition;

                var conditionObject = (condition.ConditionObject as JObject).ToObject<Dictionary<string, object>>();
                var contitionItem = conditionObject["Card"] as JObject;
                var conditionName = contitionItem.Property("Name").Value.ToString();
                Log.Console.Info(string.Format("Создание условия согласования {0}", conditionName));
                var conditionType = contitionItem.Property("ConditionType").Value.ToString();
                
                if (isContractsRule)
                {
                    conditionEntity.ConditionType = Sungero.Core.Enumeration.GetItems(typeof(company.Head.ContractCondition.ConditionType)).FirstOrDefault(e => e.Value == conditionType);
                    var contractCategory = SungeroRepository.GetEntities<Sungero.Docflow.IDocumentGroupBase>(conditionObject, "Categoriescompany", false, true);
                    if (contractCategory.Any())
                        foreach (var category in contractCategory)
                        {
                            var currencyItem = company.Head.ContractConditions.As(conditionEntity).Categoriescompany.AddNew();
                            currencyItem.Category = category;
                        }
                }
                else
                    conditionEntity.ConditionType = Sungero.Core.Enumeration.GetItems(typeof(company.Head.Condition.ConditionType)).FirstOrDefault(e => e.Value == conditionType);
                
                conditionEntity.Amount = contitionItem.Property("Amount").ToObject<double?>();
                var amountOperator = contitionItem.Property("AmountOperator").Value.ToString();
                
                if (!string.IsNullOrEmpty(amountOperator))
                    conditionEntity.AmountOperator = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.ConditionBase.AmountOperator)).FirstOrDefault(e => e.Value == amountOperator);

                Log.Console.Info("Проверка наличия валют");
                var currencies = SungeroRepository.GetEntities<Sungero.Commons.ICurrency>(conditionObject, "Currencies", false, true);
                
                if (currencies.Any())
                    foreach (var currency in currencies)
                    {
                        var currencyItem = conditionEntity.Currencies.AddNew();
                        currencyItem.Currency = currency;
                    }
                
                var documentKindsEntity = SungeroRepository.GetEntities<Sungero.Docflow.IDocumentKind>(conditionObject, "DocumentKinds", false, true);
                
                foreach (var documentKind in documentKindsEntity)
                {
                    var documentKindItem = conditionEntity.DocumentKinds.AddNew();
                    documentKindItem.DocumentKind = documentKind;
                }

                conditionEntity.Note = contitionItem.Property("Note").ToObject<string>();
                Log.Console.Info("Проверка наличия Видов документов");
                var сonditionDocumentKinds = SungeroRepository.GetEntities<Sungero.Docflow.IDocumentKind>(conditionObject, "ConditionDocumentKinds", false, true);
                
                foreach (var documentKind in сonditionDocumentKinds)
                {
                    var documentKindItem = conditionEntity.ConditionDocumentKinds.AddNew();
                    documentKindItem.DocumentKind = documentKind;
                }

                var approvalRoleItem = conditionObject["ApprovalRole"] as JObject;
                
                if (approvalRoleItem != null)
                {
                    var approvalRoleName = approvalRoleItem.Property("Name").Value.ToString();
                    var approvalRole = Session.Current.GetEntities("Sungero.Docflow.IApprovalRoleBase").Cast<Sungero.Docflow.IApprovalRoleBase>().FirstOrDefault(r => r.Name == approvalRoleName);
                    if (approvalRole != null)
                        conditionEntity.ApprovalRole = approvalRole;
                    else
                        throw new System.IO.InvalidDataException(string.Format("Роль согласования {0} не найдена", approvalRoleName));
                }

                var approvalRoleForComparisonItem = conditionObject["ApprovalRoleForComparison"] as JObject;
                
                if (approvalRoleForComparisonItem != null)
                {
                    var approvalRoleForComparisonName = approvalRoleForComparisonItem.Property("Name").Value.ToString();
                    var approvalRoleForComparison = Session.Current.GetEntities("Sungero.Docflow.IApprovalRoleBase").Cast<Sungero.Docflow.IApprovalRoleBase>()
                      .FirstOrDefault(r => r.Name == approvalRoleForComparisonName);
                    if (approvalRoleForComparison != null)
                        conditionEntity.ApprovalRoleForComparison = approvalRoleForComparison;
                    else
                        throw new System.IO.InvalidDataException(string.Format("Роль согласования {0} не найдена", approvalRoleForComparisonName));
                }

                var recipientForComparisonItem = conditionObject["RecipientForComparison"] as JObject;
                
                if (recipientForComparisonItem != null)
                {
                    var recipientForComparisonName = recipientForComparisonItem.Property("Name").Value.ToString();
                    var recipientForComparison = Session.Current.GetEntities("Sungero.CoreEntities.IRecipient").Cast<Sungero.CoreEntities.IRecipient>()
                      .FirstOrDefault(r => r.Name == recipientForComparisonName && r.Status == Sungero.CoreEntities.Recipient.Status.Active);
                    if (recipientForComparison != null)
                        conditionEntity.RecipientForComparison = recipientForComparison;
                    else
                        throw new System.IO.InvalidDataException(string.Format("Сотрудник/Роль {0} отсутствует", recipientForComparisonName));
                }

                Log.Console.Info("Проверка наличия способов доставки");
                var deliveryMethods = SungeroRepository.GetEntities<Sungero.Docflow.IMailDeliveryMethod>(conditionObject, "DeliveryMethods", false, true);
                
                foreach (var deliveryMethod in deliveryMethods)
                {
                    var deliveryMethodItem = conditionEntity.DeliveryMethods.AddNew();
                    deliveryMethodItem.DeliveryMethod = deliveryMethod;
                }

                var addendaDocumentKindItem = conditionObject["AddendaDocumentKind"] as JObject;
                
                if (addendaDocumentKindItem != null)
                {
                    var addendaDocumentKindName = addendaDocumentKindItem.Property("Name").Value.ToString();
                    var addendaDocumentKind = Session.Current.GetEntities("Sungero.Docflow.IDocumentKind").Cast<Sungero.Docflow.IDocumentKind>()
                      .FirstOrDefault(k => k.Name == addendaDocumentKindName && k.Status == Sungero.Docflow.DocumentKind.Status.Active);
                    if (addendaDocumentKind != null)
                        conditionEntity.AddendaDocumentKind = addendaDocumentKind;
                    else
                        throw new System.IO.InvalidDataException(string.Format("Вид документа {0} не найден", addendaDocumentKindName));
                }

                if (!isContractsRule)
                {
                    Log.Console.Info("Проверка наличия адресатов");
                    var addressees = SungeroRepository.GetEntities<Sungero.Company.IEmployee>(conditionObject, "Addressees", false, true);
                    foreach (var addressee in addressees)
                    {
                        var addresseeItem = Sungero.Docflow.Conditions.As(conditionEntity).Addressees.AddNew();
                        addresseeItem.Addressee = addressee;
                    }
                }

                var conditionItem = rule.Conditions.AddNew();
                conditionItem.Condition = conditionEntity;
                conditionItem.Number = condition.Number;
                conditionEntity.Save();
            }

            #endregion

            #region Создание этапов.
            var functionsArray = content["Stages"] as JArray;
            var functions = functionsArray.ToObject<List<Stage>>().Where(s => s.FunctionsType != null).ToList();
            var currentStagesFunctions = new List<Sungero.Docflow.IApprovalStageBase>();
            
            foreach (var function in functions)
            {
                var functionObject = function.StageObject as JObject;
                var functionName = functionObject.Property("Name").Value.ToString();
                var functionType = function.StageTypeRule;
                var functionEntity = Sungero.Docflow.ApprovalStageBases.Null;
                functionEntity = Session.Current.GetEntities("Sungero.Docflow.IApprovalStageBase").Cast<Sungero.Docflow.IApprovalStageBase>().FirstOrDefault(c => c.Name == functionName) ??
                            currentStagesFunctions.FirstOrDefault(s => s.Name == functionName);
                
                if (functionEntity == null && function.FunctionsType.FullName == "Sungero.Docflow.ApprovalConvertPdfStage")
                {
                    var newFunctionEntity = Sungero.Docflow.ApprovalConvertPdfStages.As(Session.Current.CreateEntity(function.FunctionsType.ToString()));
                    newFunctionEntity.ConvertWithAddenda = functionObject.Property("ConvertWithAddenda").ToObject<bool?>();
                    functionEntity = Sungero.Docflow.ApprovalStageBases.As(newFunctionEntity);
                    functionEntity = FillFunction(functionEntity, functionObject);
                }
                
                if (functionEntity == null && function.FunctionsType.FullName == "Sungero.Docflow.IApprovalReviewTaskStage")
                {
                    var newFunctionEntity = Sungero.Docflow.ApprovalReviewTaskStages.As(Session.Current.CreateEntity(function.FunctionsType.ToString()));
                    newFunctionEntity.WaitReviewTaskCompletion = functionObject.Property("WaitReviewTaskCompletion").ToObject<bool?>();
                    functionEntity = Sungero.Docflow.ApprovalStageBases.As(newFunctionEntity);
                    functionEntity = FillFunction(functionEntity, functionObject);
                }
                
                if (functionEntity == null && function.FunctionsType.FullName == "Sungero.Contracts.IApprovalIncInvoicePaidStage")
                {
                    var newFunctionEntity = Sungero.Contracts.ApprovalIncInvoicePaidStages.As(Session.Current.CreateEntity(function.FunctionsType.ToString()));
                    functionEntity = Sungero.Docflow.ApprovalStageBases.As(newFunctionEntity);
                    functionEntity = FillFunction(functionEntity, functionObject);
                }
                
                currentStagesFunctions.Add(functionEntity);
                var functionItem = rule.Stages.AddNew();
                functionItem.StageBase = functionEntity;
                functionItem.Number = function.Number;
                functionItem.StageType = functionType;
            }
            var stagesArray = content["Stages"] as JArray;
            var stages = stagesArray.ToObject<List<Stage>>().Where(s => s.FunctionsType == null).ToList();
            var currentStages = new List<Sungero.Docflow.IApprovalStage>();
            
            foreach (var stage in stages)
            {
                var stageObject = stage.StageObject as JObject;
                var stageName = stageObject.Property("Name").Value.ToString();
                var stageType = stage.StageTypeRule;
                var stageEntity = Sungero.Docflow.ApprovalStages.Null;
                stageEntity = Session.Current.GetEntities("Sungero.Docflow.IApprovalStage").Cast<Sungero.Docflow.IApprovalStage>().FirstOrDefault(c => c.Name == stageName) ??
                              currentStages.FirstOrDefault(s => s.Name == stageName);

                if (stageEntity == null)
                {
                    stageEntity = company.Head.ApprovalStages.As(Session.Current.CreateEntity(typeof(company.Head.IApprovalStage)));
                    stageEntity.Name = stageName;
                    stageEntity.StageType = stageType;
                    stageEntity.AllowSendToRework = stageObject.Property("AllowSendToRework").ToObject<bool?>();
                    company.Head.ApprovalStages.As(stageEntity).CheckScancompany = stageObject.Property("CheckScancompany").ToObject<bool?>();
                    company.Head.ApprovalStages.As(stageEntity).CheckOriginalcompany = stageObject.Property("CheckOriginalcompany").ToObject<bool?>();
                    company.Head.ApprovalStages.As(stageEntity).CheckVersionCreatingcompany = stageObject.Property("CheckVersionCreatingcompany").ToObject<bool?>();

                    var deadlineInDays = stageObject.Property("DeadlineInDays").ToObject<int?>();
                    
                    if (deadlineInDays != null)
                        stageEntity.DeadlineInDays = deadlineInDays;
                    var deadlineInHours = stageObject.Property("DeadlineInHours").ToObject<int?>();
                    
                    if (deadlineInHours != null)
                        stageEntity.DeadlineInHours = deadlineInHours;

                    var sequence = stageObject.Property("Sequence").Value.ToString();
                    
                    if (!string.IsNullOrEmpty(sequence))
                        stageEntity.Sequence = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.ApprovalStage.Sequence)).FirstOrDefault(e => e.Value == sequence);

                    var reworkType = stageObject.Property("ReworkType").Value.ToString();
                    
                    if (!string.IsNullOrEmpty(reworkType))
                        stageEntity.ReworkType = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.ApprovalStage.ReworkType)).FirstOrDefault(e => e.Value == reworkType);

                    var reworkPerformerTypeStage = stageObject.Property("ReworkPerformerType").Value.ToString();
                    
                    if (!string.IsNullOrEmpty(reworkPerformerTypeStage))
                        stageEntity.ReworkPerformerType = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.ApprovalStage.ReworkPerformerType)).FirstOrDefault(e => e.Value == reworkPerformerTypeStage);

                    var reworkPerformerItemStage = content["ReworkPerformer"] as JObject;
                    
                    if (reworkPerformerItemStage != null)
                    {
                        var reworkPerformerItemName = reworkPerformerItemStage.Property("Name").Value.ToString();
                        var reworkPerformer = Session.Current.GetEntities("Sungero.CoreEntities.IRecipient").Cast<Sungero.CoreEntities.IRecipient>()
                          .FirstOrDefault(e => e.Name == reworkPerformerItemName && e.Status == Sungero.CoreEntities.Recipient.Status.Active);
                        
                        if (reworkPerformer != null)
                            stageEntity.ReworkPerformer = reworkPerformer;
                        else
                            throw new System.IO.InvalidDataException(string.Format("Роль/сотрудник {0} не найден", reworkPerformerItemName));
                    }

                    var reworkApprovalRoleItemStage = content["ReworkApprovalRole"] as JObject;
                    
                    if (reworkApprovalRoleItem != null)
                    {
                        var reworkApprovalRoleItemName = reworkApprovalRoleItemStage.Property("Name").Value.ToString();
                        var reworkApprovalRole = Session.Current.GetEntities("Sungero.Docflow.IApprovalRoleBase").Cast<Sungero.Docflow.IApprovalRoleBase>()
                          .FirstOrDefault(e => e.Name == reworkApprovalRoleItemName && e.Status == Sungero.CoreEntities.Recipient.Status.Active);
                        if (reworkApprovalRole != null)
                            rule.ReworkApprovalRole = reworkApprovalRole;
                        else
                            throw new System.IO.InvalidDataException(string.Format("Роль согласования {0} не найдена", reworkApprovalRoleItemName));
                    }

                    stageEntity.AllowChangeReworkPerformer = stageObject.Property("AllowChangeReworkPerformer").ToObject<bool?>();
                    stageEntity.NeedStrongSign = stageObject.Property("NeedStrongSign").ToObject<bool?>();
                    stageEntity.StartDelayDays = stageObject.Property("StartDelayDays").ToObject<int?>();
                    stageEntity.Subject = stageObject.Property("Subject").ToObject<string>();
                    stageEntity.IsConfirmSigning = stageObject.Property("IsConfirmSigning").ToObject<bool?>();
                    stageEntity.IsResultSubmission = stageObject.Property("IsResultSubmission").ToObject<bool?>();
                    stageEntity.Note = stageObject.Property("Note").ToObject<string>();
                    stageEntity.NeedRestrictPerformerRights = stageObject.Property("NeedRestrictPerformerRights").ToObject<bool>();
                    var rightType = stageObject.Property("RightType").Value.ToString();
                    
                    if (!string.IsNullOrEmpty(rightType))
                        stageEntity.RightType = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.ApprovalStage.RightType)).FirstOrDefault(e => e.Value == rightType);

                    var assigneeType = stageObject.Property("AssigneeType").Value.ToString();
                    
                    if (!string.IsNullOrEmpty(assigneeType))
                        stageEntity.AssigneeType = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.ApprovalStage.AssigneeType)).FirstOrDefault(e => e.Value == assigneeType);

                    stageEntity.AllowAdditionalApprovers = stageObject.Property("AllowAdditionalApprovers").ToObject<bool?>();

                    
                    if (stage.ApprovalRole != null)
                    {
                        var approvalRuleObject = stage.ApprovalRole as JObject;
                        var approvalRoleName = approvalRuleObject.Property("Name").Value.ToString();
                        var approvalRole = Session.Current.GetEntities("Sungero.Docflow.IApprovalRoleBase").Cast<Sungero.Docflow.IApprovalRoleBase>().FirstOrDefault(r => r.Name == approvalRoleName);
                        //TODO: Проверка на количество 
                        if (approvalRole != null)
                            stageEntity.ApprovalRole = approvalRole;
                        else
                            throw new System.Data.DataException(string.Format("Роль согласования {0} не найдена", approvalRoleName));
                    }

                    if (stage.Assignee != null)
                    {
                        var assigneeObject = stage.Assignee as JObject;
                        var assigneeName = assigneeObject.Property("Name").Value.ToString();
                        var assignee = Session.Current.GetEntities("Sungero.CoreEntities.IRecipient").Cast<Sungero.CoreEntities.IRecipient>().FirstOrDefault(r => r.Name == assigneeName);
                        //TODO: Проверка по типу.
                        if (assignee != null)
                            stageEntity.Assignee = assignee;
                        else
                            throw new System.Data.DataException(string.Format("Исполнитель {0} не найден", assigneeName));
                    }

                    if (stage.ApprovalRoles.Any())
                    {
                        foreach (var approvalRole in stage.ApprovalRoles)
                        {
                            var approvalRoleObject = approvalRole as JObject;
                            var approvalRoleName = approvalRoleObject.Property("Name").Value.ToString();
                            var approvalRoleEntity = Session.Current.GetEntities("Sungero.Docflow.IApprovalRoleBase").Cast<Sungero.Docflow.IApprovalRoleBase>().FirstOrDefault(c => c.Name == approvalRoleName);

                            if (approvalRoleEntity == null)
                                throw new System.Data.DataException(string.Format("Роль согласования {0} не найдена", approvalRoleName));

                            var approvalRoleItem = stageEntity.ApprovalRoles.AddNew();
                            approvalRoleItem.ApprovalRole = approvalRoleEntity;
                        }
                    }

                    if (stage.Recipients.Any())
                    {
                        foreach (var recipient in stage.Recipients)
                        {
                            var recipientObject = recipient as JObject;
                            var recipientName = recipientObject.Property("Name").Value.ToString();
                            var recipientEntity = Session.Current.GetEntities("Sungero.CoreEntities.IRecipient").Cast<Sungero.CoreEntities.IRecipient>().FirstOrDefault(c => c.Name == recipientName);

                            if (recipientEntity == null)
                                throw new System.Data.DataException(string.Format("Исполнитель {0} не найден", recipientName));

                            var recipientItem = stageEntity.Recipients.AddNew();
                            recipientItem.Recipient = recipientEntity;
                        }
                    }
                }

                currentStages.Add(stageEntity);

                var stageItem = rule.Stages.AddNew();
                stageItem.Stage = stageEntity;
                stageItem.Number = stage.Number;
                stageItem.StageType = stageType;
            }

            #endregion

            var transitionsArray = content["Transitions"] as JArray;
            var transitions = transitionsArray.ToObject<List<Transitions>>();

            foreach (var transition in transitions)
            {
                var transitionItem = rule.Transitions.AddNew();
                transitionItem.SourceStage = transition.SourceStage;
                transitionItem.TargetStage = transition.TargetStage;
                transitionItem.ConditionValue = transition.ConditionValue;
            }

            #endregion
            var statusName = entityItem.Property("Status").Value.ToString();
            
            if (statusName == "Active")
                rule.Status = Sungero.Docflow.ApprovalRuleBase.Status.Active;
            else
                rule.Status = Sungero.Docflow.ApprovalRuleBase.Status.Closed;
            Session.Current.SubmitChanges();
        }

        public override IEnumerable<IEntity> Filter(IEnumerable<IEntity> entities)
        {
            return entities.Cast<Sungero.Docflow.IApprovalRuleBase>().Where(e => e.Status == Sungero.Docflow.ApprovalRuleBase.Status.Active);
        }

        protected override Dictionary<string, object> Export(IEntity entity)
        {
            var isApprovalRule = Sungero.Docflow.ApprovalRules.Is(entity);

            if (isApprovalRule)
                content["Card"] = company.Head.ApprovalRules.As(entity);
            else
                content["Card"] = company.Head.ContractsApprovalRules.As(entity);

            var approvalRule = Sungero.Docflow.ApprovalRuleBases.As(entity);
            content["ReworkPerformer"] = approvalRule.ReworkPerformer;
            content["ReworkApprovalRole"] = approvalRule.ReworkApprovalRole;
            content["DocumentKinds"] = approvalRule.DocumentKinds.Select(k => k.DocumentKind);
            content["Departments"] = approvalRule.Departments.Select(d => d.Department);
            content["BusinessUnit"] = approvalRule.BusinessUnits.Select(u => u.BusinessUnit);
            content["DocumentGroup"] = approvalRule.DocumentGroups.Select(g => g.DocumentGroup);
            var stages = new List<Stage>();
            
            foreach (var stage in approvalRule.Stages)
            {
                if (stage.StageType.Value.Value != "Function")
                    stages.Add(new Stage()
                    {
                        Number = stage.Number.GetValueOrDefault(),
                        StageObject = stage.Stage,
                        Assignee = stage.Stage.Assignee,
                        ApprovalRole = stage.Stage.ApprovalRole,
                        ApprovalRoles = stage.Stage.ApprovalRoles.Select(r => r.ApprovalRole),
                        Recipients = stage.Stage.Recipients.Select(r => r.Recipient),
                        StageTypeRule = stage.StageType,
                        ReworkApprovalRole = stage.Stage.ReworkApprovalRole,
                        ReworkPerformer = stage.Stage.ReworkPerformer
                    });
                else
                {
                    stages.Add(new Stage()
                    {
                        Number = stage.Number.GetValueOrDefault(),
                        StageObject = stage.StageBase,
                        StageTypeRule = stage.StageType,
                        FunctionsType = stage.StageBase.GetEntityInterfaceType()
                    });
                }
            }
            content["Stages"] = stages;

            var conditions = new List<Condition>();
            foreach (var condition in approvalRule.Conditions)
            {
                var conditionContent = new Dictionary<string, object>();
                conditionContent["Card"] = condition.Condition;
                conditionContent["Currencies"] = condition.Condition.Currencies.Select(c => c.Currency);
                
                try
                {   
                    conditionContent["Categoriescompany"] = company.Head.ContractConditions.As(condition.Condition).Categoriescompany.Select(c => c.Category);
                }

                catch
                { }
                conditionContent["DocumentKinds"] = condition.Condition.DocumentKinds.Select(c => c.DocumentKind);
                conditionContent["ConditionDocumentKinds"] = condition.Condition.ConditionDocumentKinds.Select(c => c.DocumentKind);
                conditionContent["ApprovalRole"] = condition.Condition.ApprovalRole;
                conditionContent["ApprovalRoleForComparison"] = condition.Condition.ApprovalRoleForComparison;
                conditionContent["RecipientForComparison"] = condition.Condition.RecipientForComparison;
                conditionContent["DeliveryMethods"] = condition.Condition.DeliveryMethods.Select(c => c.DeliveryMethod);
                conditionContent["AddendaDocumentKind"] = condition.Condition.AddendaDocumentKind;

                if (isApprovalRule)
                    conditionContent["Addressees"] = Sungero.Docflow.Conditions.As(condition.Condition).Addressees.Select(a => a.Addressee);

                conditions.Add(new Condition() { ConditionObject = conditionContent, Number = condition.Number.GetValueOrDefault() });
            }
            content["Conditions"] = conditions;

            var transitions = new List<Transitions>();
            foreach (var transition in approvalRule.Transitions)
                transitions.Add(new Transitions() { SourceStage = transition.SourceStage, TargetStage = transition.TargetStage, ConditionValue = transition.ConditionValue });
            content["Transitions"] = transitions;

            return content;
        }
        private Sungero.Docflow.IApprovalStageBase FillFunction(Sungero.Docflow.IApprovalStageBase function, JObject functionObject)
        {
            var stageFunction = Sungero.Docflow.ApprovalFunctionStageBases.As(function);
            stageFunction.Name = functionObject.Property("Name").ToObject<string>();
            stageFunction.Note = functionObject.Property("Note").ToObject<string>();
            var deadlineInDays = functionObject.Property("DeadlineInDays").ToObject<int?>();
            
            if (deadlineInDays != null)
                stageFunction.DeadlineInDays = deadlineInDays;
            
            var deadlineInHours = functionObject.Property("DeadlineInHours").ToObject<int?>();
            
            if (deadlineInHours != null)
                stageFunction.DeadlineInHours = deadlineInHours;
            stageFunction.TimeoutInDays = functionObject.Property("TimeoutInDays").ToObject<int?>();
            stageFunction.TimeoutInHours = functionObject.Property("TimeoutInHours").ToObject<int?>();
            var timeoutAction = functionObject.Property("TimeoutAction").Value.ToString();
            
            if (!string.IsNullOrEmpty(timeoutAction))
                stageFunction.TimeoutAction = Sungero.Core.Enumeration.GetItems(typeof(Sungero.Docflow.ApprovalFunctionStageBase.TimeoutAction)).FirstOrDefault(e => e.Value == timeoutAction);
            
            function = Sungero.Docflow.ApprovalStageBases.As(stageFunction);
            return function;
        }
    }
}

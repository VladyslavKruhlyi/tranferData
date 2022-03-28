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
    class RoleSerializer : SungeroSerializer
    {
        public RoleSerializer() : base()
        {
            this.EntityName = "Role";
            this.EntityTypeName = "Sungero.CoreEntities.IRole";
        }

        public override void Import(Dictionary<string, object> content)
        {
            var entityItem = content["Card"] as JObject;
            var roleName = entityItem.Property("Name").Value.ToString();
            var sid = content["Sid"] as string;

            var activeRole = Session.Current.GetEntities(this.EntityTypeName).Cast<Sungero.CoreEntities.IRole>()
               .Where(r => r.Sid.ToString() == sid || r.Name == roleName).FirstOrDefault();
            Sungero.CoreEntities.IRole role;
            
            if (activeRole != null)
            {
                role = activeRole;
                Log.Console.Info(string.Format("ИД = {0}. Обновление роли {1}", role.Id, roleName));
            }
            
            else
            {
                role = Session.Current.CreateEntity(this.EntityTypeName) as Sungero.CoreEntities.IRole;
                Log.Console.Info(string.Format("ИД = {0}. Создание роли {1}", role.Id, roleName));
            }

            if (roleName != "Администраторы" && roleName != "Аудиторы" && roleName != "Менеджеры конфигураций" && roleName != "Все пользователи"
                && roleName != "Пользователи Solo" && roleName != "Служебные пользователи")
            {
                role.Name = roleName;
                role.Sid = Guid.Parse(sid);
                role.Description = entityItem.Property("Description").ToObject<string>();
                role.IsSingleUser = entityItem.Property("IsSingleUser").ToObject<bool?>();
                var statusName = entityItem.Property("Status").Value.ToString();
                
                if (statusName == "Active")
                    role.Status = Sungero.Docflow.DocumentKind.Status.Active;
                else
                    role.Status = Sungero.Docflow.DocumentKind.Status.Closed;

                Log.Console.Info("Заполнение участников роли");
                var recipients = SungeroRepository.GetEntities<Sungero.CoreEntities.IRecipient>(content, "RecipientLinks", true, true);
                role.RecipientLinks.Clear();
                
                foreach (var recipient in recipients)
                {
                    var recipientLinkItem = role.RecipientLinks.AddNew();
                    recipientLinkItem.Member = recipient;
                }

                if (role.IsSingleUser.GetValueOrDefault() && !recipients.Any())
                {
                    var serviceUsersRole = Session.Current.GetEntities("Sungero.CoreEntities.IRole").Cast<Sungero.CoreEntities.IRole>()
                      .Where(x => x.Sid == Sungero.Domain.Shared.SystemRoleSid.ServiceUsers).FirstOrDefault();
                    var serviceUser = serviceUsersRole.RecipientLinks.Where(x => x.Member.Name == "Service User").FirstOrDefault().Member;
                    var recipientLinkItem = role.RecipientLinks.AddNew();
                    recipientLinkItem.Member = serviceUser;
                    Log.Console.Info(string.Format("Роль {0} заполнена системным пользователем", roleName));
                }

                role.Save();
                Session.Current.SubmitChanges();
            }
        }

        protected override Dictionary<string, object> Export(IEntity entity)
        {
            base.Export(entity);
            var role = entity as Sungero.CoreEntities.IRole;
            content["RecipientLinks"] = role.RecipientLinks.Select(l => l.Member);
            content["Sid"] = role.Sid;
            return content;
        }
    }
}

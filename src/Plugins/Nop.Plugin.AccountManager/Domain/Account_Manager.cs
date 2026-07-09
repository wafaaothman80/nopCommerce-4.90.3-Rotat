using Nop.Core;
using Nop.Core.Domain.Common;


namespace Nop.Plugin.AccountManager.Domain
{
    public class Account_Manager : BaseEntity, ISoftDeletedEntity
    {
       
        public string AccountManagerName { get; set; }
        public bool Active { get; set; }
        public DateTime ManagerStartDate { get; set; }
        public int Customer_Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool Deleted { get; set; }
        public int ERPAccountManagerId { get; set; }
    }
}

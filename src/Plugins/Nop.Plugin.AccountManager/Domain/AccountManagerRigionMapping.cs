using Nop.Core;


namespace Nop.Plugin.AccountManager.Domain
{
   

    public partial class AccountManagerRigionMapping : BaseEntity
    {
       
        public int RigionId { get; set; }

        
        public int AccountManagerId { get; set; }
    }



}

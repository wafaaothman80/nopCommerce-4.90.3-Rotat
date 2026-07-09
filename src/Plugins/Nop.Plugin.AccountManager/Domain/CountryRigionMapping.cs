using Nop.Core;


namespace Nop.Plugin.AccountManager.Domain
{
   

    public partial class CountryRigionMapping : BaseEntity
    {
       
        public int RigionId { get; set; }

        
        public int CountryId { get; set; }
    }



}

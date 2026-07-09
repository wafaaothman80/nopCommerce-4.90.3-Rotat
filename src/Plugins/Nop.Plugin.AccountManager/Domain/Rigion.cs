using Nop.Core;


namespace Nop.Plugin.AccountManager.Domain
{
    public class Rigion : BaseEntity
    {
        public string RigionName { get; set; }
        public bool Active { get; set; }
        public DateTime RigionAddedDate { get; set; }

        public int DisplayOrder { get; set; }

    }

   



}

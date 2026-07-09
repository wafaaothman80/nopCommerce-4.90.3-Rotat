using Nop.Core;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention;

public class SS_MAP_EntityWidgetMapping : BaseEntity
{
    public int EntityType { get; set; }
    public int EntityId { get; set; }
    public string WidgetZone { get; set; }
    public int DisplayOrder { get; set; }
   
}

public class SS_AS_Slide : BaseEntity
{
    public string Url { get; set; }
     public string Alt { get; set; }
        public bool Visible { get; set; }
    public int DisplayOrder { get; set; }
     public int PictureId { get; set; }
   public int SliderId { get; set; }
     public int MobilePictureId { get; set; }
    public string SystemName { get; set; }
    public int SlideType { get; set; }
    public string Content { get; set; }
}






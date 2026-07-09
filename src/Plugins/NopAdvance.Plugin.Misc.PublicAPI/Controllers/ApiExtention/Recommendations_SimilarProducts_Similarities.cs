using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention;
public class Recommendations_SimilarProducts_Similarities : BaseEntity
{
    public int ProductId { get; set; }
    public int SimilarProductId { get; set; }
    public double Similarity { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
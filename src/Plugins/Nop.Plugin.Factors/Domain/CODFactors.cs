using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace Nop.Plugin.Factors.Domain;
public class CODFactors : BaseEntity
{
   
   
    public int CountryID { get; set; }
    public string Name { get; set; }
    public decimal CODFactor { get; set; }
    public int FactorID { get; set; }
   




}

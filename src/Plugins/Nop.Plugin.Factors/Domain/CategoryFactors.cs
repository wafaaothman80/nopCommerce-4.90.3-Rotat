using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace Nop.Plugin.Factors.Domain;
public class CategoryFactors : BaseEntity
{
   
   
    public int CategoryID { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    
    public decimal Factor { get; set; }
   
   


}

using DAL.SolidEdge.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class AsmModel : PartModel
    {
        public List<AsmOccurrence>? Occurrences { get; set; }
    }
}
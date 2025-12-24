using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOZON.Models
{
    internal class Report
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string Period { get; set; }
        public string Status { get; set; }
    }
}

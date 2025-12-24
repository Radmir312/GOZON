using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOZON.Models
{
    internal class Movement
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string FromWarehouse { get; set; }
        public string ToWarehouse { get; set; }
        public string SupplierName { get; set; }
        public int Quantity { get; set; }
        public string MovementType { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TypeDisplay { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace GOZON.Models
{
    public class User
    {
        public int Id { get; set; }
        public int Login { get; set; }
        public int PasswordHash { get; set; }
        public int FullName { get; set; }
        public int Email { get; set; }
        public int Role { get; set; }
        public int CreatedAt { get; set; }
    }
}

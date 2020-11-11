using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inzynierka.DatabaseAccess
{
    public class DatabaseVirtualFolder
    {
        public string Path { get; set; }
        public int Id { get; set; }
        public bool IsRoot { get; set; } = false;
    }
}

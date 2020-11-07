using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Inzynierka.Models;

namespace Inzynierka.Models
{
    public class TreeViewItemSelectedEventArgs : EventArgs
    {
        public FolderItem Parameter;

        public TreeViewItemSelectedEventArgs(FolderItem param) => Parameter = param;

    }
}

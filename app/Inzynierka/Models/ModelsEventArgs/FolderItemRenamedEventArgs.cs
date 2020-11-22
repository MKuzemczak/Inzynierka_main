using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inzynierka.Models.ModelsEventArgs
{
    public class FolderItemRenamedEventArgs : EventArgs
    {
        string OldName { get; }
        string NewName { get; }

        public FolderItemRenamedEventArgs(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }
    }
}

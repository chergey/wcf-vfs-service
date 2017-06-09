using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Emroy.Vfs.Service.Dto
{

    /// <summary>
    /// File or directory description
    /// </summary>
    [DebuggerDisplay("Name={" + nameof(Name) + ("}, IsDirectory = {" + nameof(IsDirectory) + "}"))]
    public class Entry
    {
        public Dictionary<string, bool> Locks { get; set; }

        public bool IsLocked(string userName) => 
            !string.IsNullOrEmpty(userName) ? Locks[userName] : 
            Locks.Sum(f => f.Value ? 1 : 0) > 0;


        public bool IsDirectory { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public uint Size { get; set; }

        public ulong AccessTime { get; set; }

        public ulong ModifyTime { get; set; }

        public ulong CreationTime { get; set; }

        public uint Flags { get; set; }


    }
}

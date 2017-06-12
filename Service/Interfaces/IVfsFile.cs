using System.Collections.Generic;

namespace Emroy.Vfs.Service.Interfaces
{
    
    public interface IVfsFile
    {
        List<string> Locks { get; }

        bool IsLocked(string name = null);
        void LockFile(string label);
        void UnLockFile(string label);
    }
}
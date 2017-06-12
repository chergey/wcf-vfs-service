using System.Collections.Generic;

namespace Emroy.Vfs.Service.Interfaces
{
    
    public interface IVfsFile
    {
        List<string> Locks { get; }

        /// <summary>
        /// Returns true if file locked
        /// </summary>
        /// <param name="name">user name of the user, if null - any user</param>
        bool IsLocked(string name = null);
        /// <summary>
        /// Lock file 
        /// </summary>
        /// <param name="label"></param>
        void LockFile(string label);

        /// <summary>
        /// Unlock file
        /// </summary>
        /// <param name="label"></param>
        void UnLockFile(string label);
    }
}
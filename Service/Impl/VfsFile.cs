using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Interfaces;

namespace Emroy.Vfs.Service.Impl
{
    /// <summary>
    /// Vfs file implementation
    /// </summary>
    public class VfsFile : VfsEntity, IVfsFile
    {
        public VfsFile(string name)
        {
            Name = name;
        }

        private List<string> _locks=new List<string>();

        private byte[] _contents;

        public bool IsLocked(string name = null)
        {
            lock (_locks)
            {
                return _locks.Any(f => f == (name ?? f));
            }
        }

        public List<string> Locks
        {
            get
            {
                lock (_locks)
                    return _locks;
            }
        }


        public void LockFile(string label)
        {
            lock (_locks)
            {
                if (_locks.Contains(label))
                {
                    throw new VfsException($"File {Name} is already locked by {label}!");
                }
                _locks.Add(label);
            }
        }

        public void UnLockFile(string label)
        {
            lock (_locks)
            {
                if (!_locks.Contains(label))
                {
                    throw new VfsException($"File {Name} is not locked!");
                }
                _locks.Remove(label);
            }
        }

    }

}


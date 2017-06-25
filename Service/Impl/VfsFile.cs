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
        public VfsFile(string name, VfsDirectory parent) : base(name, parent)
        {
        }

        private readonly List<string> _locks = new List<string>();

        private byte[] _contents = new byte[0];

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
                DateModified = DateTime.Now;
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
                DateModified = DateTime.Now;
            }
        }

        private void WriteToFile(byte[] data)
        {
            int oldLength = _contents.Length;
            Array.Resize(ref _contents, _contents.Length + data.Length);
            Array.Copy(data, 0, _contents, oldLength, data.Length);

            DateModified=DateTime.Now;
        }

    }

}


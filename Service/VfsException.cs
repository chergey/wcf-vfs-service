using System;

namespace Emroy.Vfs.Service
{
    public class VfsException : Exception
    {
        public VfsException()
        {

        }

        public VfsException(string message) : base(message)
        {
        }
    }
}

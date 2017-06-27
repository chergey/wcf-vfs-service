using System;
using Service;

namespace Emroy.Vfs.Service
{
    public class VfsException : Exception
    {
        public VfsExceptionType ExType;

        public VfsException(VfsExceptionType type, string message ) : base(message)
        {
            ExType=type;
        }
    }
}

using Emroy.Vfs.Service.Impl;
using Emroy.Vfs.Service.Interfaces;

namespace Emroy.Vfs.Service.Dto
{
    /// <summary>
    /// Description of user that is connected 
    /// </summary>
    public class VfsUser
    {
        public string CurDir;
        public string Sid;
        public string Name;

        public IVfsServiceCallback Callback;
    }
}
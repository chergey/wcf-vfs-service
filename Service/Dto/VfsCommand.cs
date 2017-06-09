using Emroy.Vfs.Service.Enums;

namespace Emroy.Vfs.Service.Dto
{
    /// <summary>
    /// Command description
    /// </summary>
    public struct VfsCommand
    {
        public VfsCommandType Type;

        public string UserName;

        public string[] Arguments;


    }
}
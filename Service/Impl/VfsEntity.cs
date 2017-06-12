using System.Diagnostics;

namespace Emroy.Vfs.Service.Impl
{
    [DebuggerDisplay("Name={" + nameof(Name) + "}, Parent = {" + nameof (Parent) + "}")]
    public abstract class VfsEntity
    {
        public string Name;

        public VfsDirectory Parent;

    }
}
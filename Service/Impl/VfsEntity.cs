using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Emroy.Vfs.Service.Impl
{
    /// <summary>
    /// Base class for all objects in the file system
    /// </summary>
    [DebuggerDisplay("Name={" + nameof(Name) + "}")]
    public abstract class VfsEntity
    {

        /// <summary>
        /// Entity name
        /// </summary>
        public string Name;

        /// <summary>
        /// Directory where entity is located
        /// </summary>
        public VfsDirectory Parent;


   

        /// <summary>
        /// List of parents beginning with the highest
        /// </summary>
        protected List<VfsDirectory> Parents
        {
            get
            {
                var list = new List<VfsDirectory>();
                var p = Parent;
                while (p != null)
                {
                    list.Add(p);
                    p = p.Parent;
                }
                list.Reverse();
                return list;
            }
        }

        /// <summary>
        /// Full path
        /// </summary>
        public string Path => string.Join(VfsDirectory.Separator,
                                  Parents.Select(f => f.Name)) +VfsDirectory.Separator + Name;
    }
}
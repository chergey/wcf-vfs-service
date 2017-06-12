using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Impl;

namespace Emroy.Vfs.Service.Interfaces
{
    public interface IVfsDirectory
    {
        /// <summary>
        /// Returns true if current directory contains file or directory with a given name
        /// </summary>
        bool Contains(string name);


        void CopyEntity(string srcPath, string destPath);

        /// <summary>
        /// Creates file
        /// </summary>
        /// <param name="path">name or path (relative to the root)</param>
        VfsFile CreateFile(string path);

        VfsDirectory FindSubDir(string path, out string newPath, int skip = 1);

        VfsDirectory CreateSubDirectory(string path);
        void DeleteFile(string path);
        void DeleteSubDirectory(string path);
        void MoveEntity(string srcPath, string destPath);
    }
}
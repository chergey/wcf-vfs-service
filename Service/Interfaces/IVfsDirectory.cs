using System.Collections.Generic;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Impl;

namespace Emroy.Vfs.Service.Interfaces
{
    /// <summary>
    /// Main interface : all the manipulations to VFS are accomplished through it
    /// </summary>
    public interface IVfsDirectory
    {

        /// <summary>
        /// Returns true if current directory contains specified files or directories
        /// </summary>
        bool Contains(params string[] paths);

        /// <summary>
        /// Copies file or directory
        /// </summary>
        /// <param name="srcPath">name or path (relative to the root) of the source file or directory</param>
        /// <param name="destPath">name or path (relative to the root) to the destination directory (empty ("") means current path)</param>
        /// <exception cref="VfsException">if can't copy entity</exception>
        void CopyEntity(string srcPath, string destPath);

        /// <summary>
        /// Moves file or directory
        /// </summary>
        /// <param name="srcPath">name or path (relative to the root) of the source file or directory</param>
        /// <param name="destPath">name or path (relative to the root) to the destination directory (empty ("") means current path) </param>
        /// <exception cref="VfsException">if can't move entity</exception>
        void MoveEntity(string srcPath, string destPath);

        /// <summary>
        /// Creates file
        /// </summary>
        /// <param name="path">name or path (relative to the root)</param>
        /// <param name="userName">name of user who is creating file</param>
        /// <returns>created file</returns>
        /// <exception cref="VfsException">if can't create file</exception>
        IVfsFile CreateFile(string path, string userName);

        /// <summary>
        /// Returns directory contents
        /// /// </summary>
        /// <param name="path"></param>
        /// <returns>list of tuple [directory name, list of locking users] </returns>
        List<(string, List<string>)> GetContents(string path);
        /// <summary>
        /// Creates subdir
        /// </summary>
        /// <param name="path">name or path (relative to the root)</param>
        /// <param name="userName">name of user who is creating directory</param>
        /// <returns></returns>
        IVfsDirectory CreateSubDirectory(string path, string userName);

        /// <summary>
        /// Deletes file
        /// </summary>
        /// <param name="path">name or path (relative to the root)</param>
        /// <exception cref="VfsException">if can't delete file</exception>
        void DeleteFile(string path);

        /// <summary>
        /// Deletes subdir
        /// </summary>
        /// <param name="path">name or path (relative to the root)</param>
        /// <param name="canDeleteSubDir"></param>
        /// <exception cref="VfsException">if can't delete subdirectory(contains subdirectories or locked files or does not exist)</exception>
        void DeleteSubDirectory(string path, bool canDeleteSubDir);

        /// <summary>
        /// Locks/unlocks file
        /// </summary>
        /// <param name="path">name or path (relative to the root) of the target file</param>
        /// <param name="label">user name to mark the file</param>
        /// <param name="value">true - lock, false - unlock </param>
        /// <exception cref="VfsException">if attempting to lock twice or unlock not locked file</exception>
        void LockFile(string path, string label, bool value);


        /// <summary>
        /// Looks for file or directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns>found file or directory</returns>
        VfsEntity FindEntity(string path);

        /// <summary>
        /// Delete all directories (for testing purposes)
        /// </summary>
         void Clean();
    }
}
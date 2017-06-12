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
        /// Returns true if current directory contains file or directory with a given name
        /// </summary>
        bool Contains(string name);

        /// <summary>
        /// Copies file or directory
        /// </summary>
        /// <param name="srcPath">name or path (relative to the root) of the source file or directory</param>
        /// <param name="destPath">name or path (relative to the root) to the destination directory</param>
        void CopyEntity(string srcPath, string destPath);

        /// <summary>
        /// Creates file
        /// </summary>
        /// <param name="path">name or path (relative to the root)</param>
        IVfsFile CreateFile(string path);


        /// <summary>
        /// Retrieves subdirectory in the current directory
        /// </summary>
        /// <param name="path">path of file or directory (relative to the current directory)</param>
        /// <param name="newPath">path of file or directory (relative to the returned directory)</param>
        /// <param name="skip"></param>
        /// <returns></returns>
        IVfsDirectory FindSubDir(string path, out string newPath, int skip = 1);


        /// <summary>
        /// Returns directory contents
        /// </summary>
        /// <param name="path"></param>
        /// <returns>list of tuple directory name, list of locking users </returns>
         List<(string, List<string>)> GetContents(string path);
        /// <summary>
        /// Creates subdir
        /// </summary>
        /// <param name="path">name or path (relative to the root)</param>
        /// <returns></returns>
        IVfsDirectory CreateSubDirectory(string path);

        /// <summary>
        /// Deletes file
        /// </summary>
        /// <param name="path">name or path (relative to the root)</param>
        void DeleteFile(string path);

        /// <summary>
        /// Deletes subdir
        /// </summary>
        /// <param name="path">name or path (relative to the root)</param>
        /// <param name="canDeleteSubDir"></param>
        void DeleteSubDirectory(string path, bool canDeleteSubDir);

        /// <summary>
        /// Moves file or directory
        /// </summary>
        /// <param name="srcPath">name or path (relative to the root) of the source file or directory</param>
        /// <param name="destPath">name or path (relative to the root) to the destination directory</param>
        void MoveEntity(string srcPath, string destPath);

        /// <summary>
        /// Locks/unlocks file
        /// </summary>
        /// <param name="path">name or path (relative to the root) of the target file</param>
        /// <param name="label">user name to mark the file</param>
        /// <param name="value">true - lock, false - unlock </param>
        void LockFile(string path, string label, bool value);


        /// <summary>
        /// Goes all the way down subdirectories and throws if objet is not found
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        VfsEntity TraverseSubdirs(string path);

        /// <summary>
        /// Delete all directories
        /// </summary>
         void Clean();
    }
}
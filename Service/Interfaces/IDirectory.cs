using System.Collections.Generic;
using Emroy.Vfs.Service.Dto;

namespace Emroy.Vfs.Service.Interfaces
{
    ///// <summary>
    ///// Directory class
    ///// </summary>
    //public interface IDirectory
    //{
    //    /// <summary>
    //    /// Directory path
    //    /// </summary>
    //    string Path { get; }

    //    /// <summary>
    //    /// Directory name
    //    /// </summary>
    //    string Name { get; }

    //    /// <summary>
    //    /// Parent directory
    //    /// </summary>
    //    IDirectory Parent { get; }

    //    /// <summary>
    //    /// Checks if directory name contains 
    //    /// </summary>
    //    /// <param name="name"></param>
    //    /// <returns></returns>
    //    bool Contains(string name);


    //    /// <summary>
    //    /// Creates directory using relative path
    //    /// </summary>
    //    /// <param name="path"></param>
    //    void CreateSubDirectoryInPath(string path);


    //    /// <summary>
    //    /// Creates directory
    //    /// </summary>
    //    /// <param name="name"></param>
    //    void CreateSubDirectory(string name);


    //    /// <summary>
    //    /// Deletes directory and its subdirectories
    //    /// </summary>
    //    /// <param name="path"></param>
    //    /// <param name="ignoreLock">true -ignore locks</param>
    //    /// <param name="ignoreSubdirs">true - ignore subdirs</param>
    //    /// <param name="dir">true - directory</param>
    //    void Delete(string path, bool ignoreLock, bool ignoreSubdirs, bool dir);


    //    /// <summary>
    //    ///Retrieves directory contents
    //    /// </summary>
    //    /// <returns></returns>
    //    IEnumerable<Entry> GetDirectoryContents();

    //    /// <summary>
    //    /// Renames dir
    //    /// </summary>
    //    /// <param name="oldName"></param>
    //    /// <param name="newName"></param>
    //    void Rename(string oldName, string newName);

    //    /// <summary>
    //    /// Throw exception if directory can't be moved or deleted
    //    /// </summary>
    //    void Check(string path, bool locks, bool subDirs);


    //}
}
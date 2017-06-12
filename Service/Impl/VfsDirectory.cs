using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Emroy.Vfs.Service.Dto;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Interfaces;

namespace Emroy.Vfs.Service.Impl
{

    
    public class VfsDirectory : VfsEntity, IVfsDirectory
    {

        /// <summary>
        /// VFS path separator
        /// </summary>
        public const string Separator = "\\";

        /// <summary>
        /// the same as char
        /// </summary>
        public static char SeparatorChar => Separator.ToCharArray()[0];

        /// <summary>
        /// VFS root
        /// </summary>
        public const string DiskRoot = "c:";

   

        public static VfsDirectory Root = new VfsDirectory(DiskRoot);

        private List<VfsEntity> _entities = new List<VfsEntity>();

        public VfsDirectory(string name)
        {
            Name = name;
        }

        public bool Contains(string name)
        {
            var dir = _entities.FirstOrDefault(f => f.Name == name);
            return dir != null;
        }


        public VfsFile CreateFile(string path)
        {
            if (path.Contains(VfsDirectory.SeparatorChar))
            {
                var dir = FindSubDir(path, out string newPath);
                return dir.CreateFile(newPath);
            }

            AssertFileExists(path);
            var file = new VfsFile(path) { Parent = this };
            _entities.Add(file);

            return file;
        }



        /// <summary>
        /// Creates subdir
        /// </summary>
        /// <param name="path">name or path (relative to the root)</param>
        /// <returns></returns>
        public VfsDirectory CreateSubDirectory(string path)
        {
            if (path.Contains(VfsDirectory.SeparatorChar))
            {
                var dir = FindSubDir(path, out string newPath);
                return dir.CreateSubDirectory(newPath);
            }
            AssertDirExists(path);
            var file = new VfsDirectory(path) { Parent = this };
            _entities.Add(file);
            return file;
        }


        /// <summary>
        /// Deletes subdir
        /// </summary>
        /// <param name="path">name or path (relative to the root)</param>
        public void DeleteSubDirectory(string path)
        {
            if (path.Contains(VfsDirectory.SeparatorChar))
            {
                var dir = FindSubDir(path, out string newPath);
                dir.DeleteSubDirectory(newPath);
            }
            AssertDeleteDir(path);
            _entities.RemoveAll(f => f.Name == path);
        }



        /// <summary>
        /// Deletes file
        /// </summary>
        /// <param name="path">name or path (relative to the root)</param>
        public void DeleteFile(string path)
        {
            if (path.Contains(VfsDirectory.SeparatorChar))
            {
                var dir = FindSubDir(path, out string newPath);
                dir.DeleteSubDirectory(newPath);
            }
            AssertDeleteFile(path);
            _entities.RemoveAll(f => f.Name == path);
        }

      

        /// <summary>
        /// Moves file or directory
        /// </summary>
        /// <param name="srcPath">name or path (relative to the root) of the source file or directory</param>
        /// <param name="destPath">name or path (relative to the root) to the destination directory</param>
        public void MoveEntity(string srcPath, string destPath)
        {
            if (srcPath.Contains(VfsDirectory.SeparatorChar))
            {
                var dir = FindSubDir(srcPath, out string newPath);
                dir.CopyEntity(newPath, destPath);
                return;
            }

            var obj = _entities.FirstOrDefault(f => f.Name == srcPath);
            if (obj == null)
            {
                throw new VfsException($"Object {srcPath} does not exist!");
            }

            if (obj is VfsFile file && file.IsLocked())
            {
                throw new VfsException($"Can't move locked file {srcPath}!");
            }

            if (obj is VfsDirectory directory)
            {
                directory.CheckRestrictionsLock();
            }

            // destination
            var skip = destPath.Count(f => f == VfsDirectory.SeparatorChar) + 1;
            Root.CopyEntity(obj, destPath, skip);

            _entities.RemoveAll(f => f.Name == srcPath);

        }

        /// <summary>
        /// Copies file or directory
        /// </summary>
        /// <param name="srcPath">name or path (relative to the root) of the source file or directory</param>
        /// <param name="destPath">name or path (relative to the root) to the destination directory</param>
        public void CopyEntity(string srcPath, string destPath)
        {
            if (srcPath.Contains(VfsDirectory.SeparatorChar))
            {
                var dir = FindSubDir(srcPath, out string newPath);
                dir.CopyEntity(newPath, destPath);
                return;
            }

            var obj = _entities.FirstOrDefault(f => f.Name == srcPath);
            if (obj == null)
            {
                throw new VfsException($"Object {srcPath} does not exist!");
            }
            // destination

            Root.CopyEntity(obj, destPath, destPath.Count(f => f == VfsDirectory.SeparatorChar) + 1);


        }

        public VfsEntity TraverseSubdirs(string path)
        {
            if (path.Contains(VfsDirectory.SeparatorChar))
            {
                var dir = FindSubDir(path, out string newPath);
                return dir.TraverseSubdirs(newPath);
            }
            var obj = _entities.FirstOrDefault(f => f.Name == path);
            if (obj == null)
            {
                throw new VfsException($"Object {path} does not exist!");
            }
            return obj;
        }

        /// <summary>
        /// Retrieves subdirectory in the current directory
        /// </summary>
        /// <param name="path">path of file or directory (relative to the current directory)</param>
        /// <param name="newPath">path of file or directory (relative to the returned directory)</param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public VfsDirectory FindSubDir(string path, out string newPath, int skip = 1)
        {

            var dirs = path.Split(VfsDirectory.SeparatorChar);
            var dir = _entities.FirstOrDefault(f => f.Name == dirs[0]);
            AssertDirIsNull(dirs[0], dir);
            newPath = dirs.Skip(skip).Aggregate(string.Empty, (cur, s) => cur + VfsDirectory.Separator + s).Substring(VfsDirectory.Separator.Length);
            return dir as VfsDirectory;


        }
        /// <summary>
        /// Copies file or directory 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="destPath"></param>
        /// <param name="depth"></param>
        private void CopyEntity(VfsEntity entity, string destPath, int depth)
        {
            if (depth > 0)
            {
                var dir = FindSubDir(destPath, out string newPath, depth == 1 ? 0 : 1);
                dir.CopyEntity(entity, newPath, depth - 1);
                return;
            }
            var destObj = _entities.FirstOrDefault(f => f.Name == destPath);

            if (destObj != null)
            {
                throw new VfsException($"Object {destPath} already exists!");
            }
            var obj = entity.Copy();
            entity.Parent = this;
            _entities.Add(obj);

        }

        public List<string> GetContents(string path)
        {
            if (path.Contains(VfsDirectory.SeparatorChar))
            {
                var dir = FindSubDir(path, out string newPath);
                dir.CopyEntity(newPath, path);
                return GetContents(newPath);
            }
            var contents = new List<string>();
            contents.AddRange(_entities.Where(f => f is VfsFile).Select(f => f.Parent.Name + VfsDirectory.Separator + f.Name));
            foreach (var en in _entities.Where(f => f is VfsDirectory).Cast<VfsDirectory>())
            {
                contents.AddRange(en.GetContents(en.Name));
            }
            return contents;
        }


        public void LockFile(string path, string label, bool value)
        {
            if (path.Contains(VfsDirectory.SeparatorChar))
            {
                var dir = FindSubDir(path, out string newPath);
                dir.LockFile(newPath, label, value);
                return;
            }

            var obj = _entities.FirstOrDefault(f => f.Name == path);
            if (obj == null)
            {
                throw new VfsException($"Object {path} does not exist!");
            }

            if (obj is VfsFile file)
            {

                if (value)
                {
                    file.LockFile(label);
                }
                else
                {
                    file.UnLockFile(label);
                }
               
            }
            else
            {
                throw new VfsException("Directory locking is not provided!");
            }
  



        }

        #region Assertions

        private void AssertDirExists(string path)
        {
            if (_entities.FirstOrDefault(f => f is VfsDirectory && f.Name == path) != null)
            {
                throw new VfsException($"Directory {path} already exists!");
            }
        }

        /// <summary>
        /// Throws if directory can't be deleted
        /// </summary>
        /// <param name="path"></param>
        private void AssertDeleteDir(string path)
        {
            var dir = _entities.FirstOrDefault(f => f is VfsDirectory && f.Name == path) as VfsDirectory;
            if (dir == null)
            {
                throw new VfsException($"Directory {path} does not exist!");
            }
            dir.CheckRestrictionsSubDir();

        }

        /// <summary>
        /// Throws if file can't be deleted
        /// </summary>
        /// <param name="path"></param>
        private void AssertDeleteFile(string path)
        {
            var file = _entities.FirstOrDefault(f => f is VfsFile && f.Name == path) as VfsFile;

            if (file == null)
            {
                throw new VfsException($"File {path} does not exist!");
            }
            if (file.IsLocked())
            {
                throw new VfsException($"Can't delete locked file {path}!");
            }
        }


        private void AssertFileExists(string path)
        {
            if (_entities.FirstOrDefault(f => f is VfsFile && f.Name == path) != null)
            {
                throw new VfsException($"File {path} already exists!");
            }
        }


        private static void AssertDirIsNull(string path, VfsEntity entity)
        {
            if (entity == null)
            {
                throw new VfsException($"Directory {path} does not exist!");
            }
            if (entity is VfsFile)
            {
                throw new VfsException($"Directory {path} corresponds to a file!");
            }
        }


   
        /// <summary>
        /// Throws if directory contains locked files
        /// </summary>
        private void CheckRestrictionsLock()
        {
            var locks = _entities.Where(f => f is VfsFile).Cast<VfsFile>().ToArray();
            if (locks.Length > 0)
            {
                throw new VfsException("Can't move or delete directories with locked files!");
            }


        }

        /// <summary>
        /// Thros if directory contains subdirectories
        /// </summary>
        private void CheckRestrictionsSubDir()
        {
            var dirs = _entities.Where(f => f is VfsDirectory).Cast<VfsDirectory>().ToArray();
            if (dirs.Length > 0)
            {
                throw new VfsException("Can't move or delete directories with subdirectories!");
            }
        }




        #endregion


        /// <summary>
        /// Retrieves 'file.txt' from '/dir/file.txt'
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathName(string path)
        {

            var pos = path.LastIndexOf(SeparatorChar);
            return path.Substring(pos + 1);
        }

        /// <summary>
        /// Retrieves '/dir1/' from '/dir/file.txt'
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathDirectory(string path)
        {

            var pos = path.LastIndexOf(SeparatorChar);
            return path.Substring(0, pos + 1);
        }

        public static string GetFileName(string path)
        {
            var pos = path.LastIndexOf(SeparatorChar);
            return pos == -1 ? path : path.Substring(pos + 1);
        }


    }
}


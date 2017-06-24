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
    /// <summary>
    /// Vfs main interface implementation
    /// </summary>

    public class VfsDirectory : VfsEntity, IVfsDirectory
    {
        #region VFS constants
   
        /// <summary>
        /// VFS path separator
        /// </summary>
        public const string Separator = "\\";

        /// <summary>
        /// the same as char
        /// </summary>
        public static char SeparatorChar => Separator.ToCharArray()[0];

        /// <summary>
        /// VFS root name
        /// </summary>
        public const string DiskRoot = "c:";

        #endregion

        /// <summary>
        /// Parent directory for all items
        /// </summary>
        public static VfsDirectory Root = new VfsDirectory(DiskRoot);

        private readonly List<VfsEntity> _entities = new List<VfsEntity>();

        public VfsDirectory(string name)
        {
            Name = name;
        }

        public bool Contains(string name)
        {
            return GetEntity(name,false);
        }

        private VfsEntity GetEntity(string name, bool needThrow)
        {
            lock (_entities)
            {
                var entity = _entities.FirstOrDefault(f => f.Name == name);
                if (needThrow && entity == null)
                {
                    throw new VfsException($"Object {name} does not exist!");
                }
                return entity;
            }
        }


        public IVfsFile CreateFile(string path)
        {
            if (path.Contains(SeparatorChar))
            {
                var subdir = FindSubDir(path, out string newPath);
                return subdir.CreateFile(newPath);
            }
            lock (_entities)
            {
                AssertFileExists(path);

                var fileCreated = new VfsFile(path) { Parent = this };
                _entities.Add(fileCreated);

                return fileCreated;
            }
        }



        public IVfsDirectory CreateSubDirectory(string path)
        {
            if (path.Contains(SeparatorChar))
            {
                var subdir = FindSubDir(path, out string newPath);
                return subdir.CreateSubDirectory(newPath);
            }
            lock (_entities)
            {
                AssertDirExists(path);
                var dirCreated = new VfsDirectory(path) { Parent = this };
                _entities.Add(dirCreated);
                return dirCreated;
            }
        }



        public void DeleteSubDirectory(string path, bool canDeleteSubDir)
        {
            if (path.Contains(SeparatorChar))
            {
                var dir = FindSubDir(path, out string newPath);
                dir.DeleteSubDirectory(newPath, canDeleteSubDir);
            }
            lock (_entities)
            {
                var subdir = _entities.FirstOrDefault(f => f is VfsDirectory && f.Name == path) as VfsDirectory;
                if (subdir == null)
                {
                    throw new VfsException($"Directory {path} does not exist!");
                }
                if (!canDeleteSubDir)
                {
                    if (_entities.Any(f => f is VfsDirectory))
                    {
                        throw new VfsException($"Can't delete directory {path} as it contains subdirectories!");
                    }
                }

                subdir.CheckRestrictionsLock();
                _entities.RemoveAll(f => f.Name == path);
            }
        }




        public void DeleteFile(string path)
        {
            if (path.Contains(SeparatorChar))
            {
                var subdir = FindSubDir(path, out string newPath);
                subdir.DeleteFile(newPath);
            }
            lock (_entities)
            {
                AssertDeleteFile(path);
                _entities.RemoveAll(f => f.Name == path);
            }
        }




        public void MoveEntity(string srcPath, string destPath)
        {
            if (srcPath.Contains(SeparatorChar))
            {
                var subdir = FindSubDir(srcPath, out string newPath);
                subdir.CopyEntity(newPath, destPath);
                return;
            }
            lock (_entities)
            {
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
                var skip = destPath.Count(f => f == SeparatorChar) + 1;
                Root.CopyEntity(obj, destPath, skip);

                _entities.RemoveAll(f => f.Name == srcPath);
            }

        }


        public void CopyEntity(string srcPath, string destPath)
        {
            if (srcPath.Contains(SeparatorChar))
            {
                var subdir = FindSubDir(srcPath, out string newPath);
                subdir.CopyEntity(newPath, destPath);
                return;
            }

            lock (_entities)
            {
                var obj = GetEntity(srcPath, true);
                Root.CopyEntity(obj, destPath, destPath.Count(f => f == SeparatorChar) + 1);
            }

        }

        public VfsEntity TraverseSubdirs(string path)
        {
            if (path.Contains(SeparatorChar))
            {
                var dir = FindSubDir(path, out string newPath);
                return dir.TraverseSubdirs(newPath);
            }

            return GetEntity(path,true);

        }


        public IVfsDirectory FindSubDir(string path, out string newPath, int skip = 1)
        {

            var dirs = path.Split(SeparatorChar);
            var dir = _entities.FirstOrDefault(f => f.Name == dirs[0]);
            AssertDirIsNullOrIsFile(dirs[0], dir);
            newPath = dirs.Skip(skip).Aggregate(string.Empty, (cur, s) => cur + Separator + s).Substring(VfsDirectory.Separator.Length);
            return dir as IVfsDirectory;


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
                var dir = FindSubDir(destPath, out string newPath, depth == 1 ? 0 : 1) as VfsDirectory;
                dir.CopyEntity(entity, newPath, depth - 1);
                return;
            }
            lock (_entities)
            {
                var destObj = _entities.FirstOrDefault(f => f.Name == entity.Name);

                if (destObj != null)
                {
                    throw new VfsException($"Object {entity.Name} already exists!");
                }
                var obj = entity.Copy();
                obj.Parent = this;
                _entities.Add(obj);
            }

        }
        /// <summary>
        /// Returns directory contents
        /// /// </summary>
        /// <param name="path"></param>
        /// <returns>list of tuple [directory name, list of locking users] </returns>
        //TODO: if multiple users are performing commands, this method make take a while
        public List<(string, List<string>)> GetContents(string path=null)
        {
            if (!string.IsNullOrEmpty(path) && path.Contains(SeparatorChar))
            {
                var dir = FindSubDir(path, out string newPath);
                return dir.GetContents(newPath);
            }

            lock (_entities)
            {
                var contents = new List<(string, List<string>)>();

                contents.AddRange(_entities.Where(f => f is VfsFile)
                        .Cast<VfsFile>()
                        .OrderBy(f => f.Name)
                        .Select(f => (f.Path, f.Locks))
                        );

                var dirs = _entities.OrderBy(f => f.Name).Where(f => f is VfsDirectory).Cast<VfsDirectory>();
                foreach (var en in dirs)
                {
                    contents.Add((en.Path, new List<string>()));
                    contents.AddRange(en.GetContents(en.Name));
                }
                return contents;
            }
        }



        public void LockFile(string path, string label, bool value)
        {
            if (path.Contains(SeparatorChar))
            {
                var dir = FindSubDir(path, out string newPath);
                dir.LockFile(newPath, label, value);
                return;
            }

            var obj = GetEntity(path, true);
  
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
                throw new VfsException("Directory locking is not allowed!");
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
                throw new VfsException($"Can't delete locked file {file.Path}!");
            }
        }


        private void AssertFileExists(string path)
        {
            if (_entities.FirstOrDefault(f => f is VfsFile && f.Name == path) != null)
            {
                throw new VfsException($"File {path} already exists!");
            }
        }


        private static void AssertDirIsNullOrIsFile(string path, VfsEntity entity)
        {
            if (entity == null)
            {
                throw new VfsException($"Directory {path} does not exist!");
            }
            if (entity is VfsFile)
            {
                throw new VfsException($"Directory {entity.Path} corresponds to a file!");
            }
        }



        /// <summary>
        /// Throws if directory contains locked files
        /// </summary>
        private void CheckRestrictionsLock()
        {
            var locks = _entities.Where(f => f is VfsFile).Cast<VfsFile>().Where(f => f.Locks.Any());
            if (locks.Any())
            {
                throw new VfsException("Can't move or delete directories with locked files!");
            }
            var dirs = _entities.Where(f => f is VfsDirectory).Cast<VfsDirectory>().Where(f => f._entities.Any());

            foreach (var dir in dirs)
            {
                dir.CheckRestrictionsLock();
            }
        }

        public void Clean()
        {
            _entities.Clear();
        }



        #endregion

    

    }
}


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
        /// the same as previous, only as a char
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
        public static VfsDirectory Root = new VfsDirectory(DiskRoot, null);

        private readonly List<VfsEntity> _entities = new List<VfsEntity>();

        protected VfsDirectory(string name, VfsDirectory parent) : base(name, parent)
        {
        }

        public bool Contains(params string[] paths)
        {
            var foundItems = paths.ToList().Sum(path => Contains(path)  ? 1 : 0);
            return foundItems == paths.Length;

        }

        private bool Contains(string path)
        {
            if (path.Contains(SeparatorChar))
            {
                var subdir = FindSubDir(path, out string newPath);
                return subdir.Contains(newPath);
            }
            return GetEntity(path, false) != null;
        }

        /// <summary>
        /// Finds entity in current directory 
        /// </summary>
        /// <param name="name">name of entity</param>
        /// <param name="needThrow">if true, throw VfsException if entity is not found </param>
        /// <returns>found entity (or null if entity is not found and needThrow = false)</returns>
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
                AssertExists(path);

                var fileCreated = new VfsFile(path, this);
                _entities.Add(fileCreated);
                DateModified = DateModified = DateLastAccessed = DateTime.Now;

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
                AssertExists(path);
                var dirCreated = new VfsDirectory(path, this);
                _entities.Add(dirCreated);
                DateModified = DateTime.Now;
                return dirCreated;
            }
        }

        public void DeleteSubDirectory(string path, bool canDeleteSubDir)
        {
            if (path.Contains(SeparatorChar))
            {
                var subdir = FindSubDir(path, out string newPath);
                subdir.DeleteSubDirectory(newPath, canDeleteSubDir);
            }
            lock (_entities)
            {
                var obj = GetEntity(path, true);

                if (obj is VfsDirectory dir)
                {
                    if (!canDeleteSubDir)
                    {
                        if (dir._entities.Any(f => f is VfsDirectory))
                        {
                            throw new VfsException($"Can't delete directory {path} as it contains subdirectories!");
                        }

                    }

                    dir.CheckRestrictionsLock();
                    _entities.RemoveAll(f => f.Name == path);
                    DateModified = DateTime.Now;
                }
                else
                {
                    throw new VfsException($"Directory {path} correspond to file!!");
                }

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
                DateModified = DateTime.Now;
            }
        }


        public void MoveEntity(string srcPath, string destPath)
        {
            if (srcPath.Contains(SeparatorChar))
            {
                var subdir = FindSubDir(srcPath, out string newPath);
                subdir.MoveEntity(newPath, destPath);
                return;
            }
            lock (_entities)
            {
                var obj = GetEntity(srcPath, true);

                if (obj is VfsFile file && file.IsLocked())
                {
                    throw new VfsException($"Can't move locked file {srcPath}!");
                }

                if (obj is VfsDirectory directory)
                {
                    directory.CheckRestrictionsLock();
                }


                // destination
                var depth = GetDepth(destPath);
                Root.CopyEntity(obj, destPath, depth);

                //get rid of all object
                _entities.RemoveAll(f => f.Name == srcPath);
                DateModified = DateTime.Now;
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
                var depth = GetDepth(destPath);
                Root.CopyEntity(obj, destPath, depth);
            }
            DateLastAccessed=DateTime.Now;


        }

        private static int GetDepth(string destPath)
        {
            return destPath == string.Empty ? 0 : destPath.Count(f => f == SeparatorChar) + 1;
        }

        public VfsEntity FindEntity(string path)
        {
            if (path.Contains(SeparatorChar))
            {
                var subdir = FindSubDir(path, out string newPath);
                return subdir.FindEntity(newPath);
            }

            return GetEntity(path, true);

        }


        private IVfsDirectory FindSubDir(string path, out string newPath, int skip = 1)
        {

            var dirs = path.Split(SeparatorChar);
            var dir = _entities.FirstOrDefault(f => f.Name == dirs[0]);
            AssertDirIsNullOrIsFile(dirs[0], dir);
            newPath = dirs.Skip(skip).Aggregate(string.Empty, (cur, s) =>
                cur + Separator + s).Substring(Separator.Length);
            return dir as IVfsDirectory;


        }
        /// <summary>
        /// Copies file or directory 
        ///<param name="depth">destination path depth</param>
        /// </summary>
        private void CopyEntity(VfsEntity entity, string destPath, int depth)
        {
            if (depth > 0)
            {
                var subdir = FindSubDir(destPath, out string newPath, depth == 1 ? 0 : 1) as VfsDirectory;
                subdir.CopyEntity(entity, newPath, depth - 1);
                return;
            }
            lock (_entities)
            {
                var destObj = _entities.FirstOrDefault(f => f.Name == entity.Name);

                if (destObj != null)
                {
                    throw new VfsException($"Object {entity.Name} already exists!");
                }
                var copiedObj= entity.Copy();
                copiedObj.Parent = this;
                _entities.Add(copiedObj);
                DateModified = DateTime.Now;
            }

        }

        //TODO: if multiple users are performing commands, this method will wait for them to complete
        public List<(string, List<string>)> GetContents(string path = null)
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
                var subdir = FindSubDir(path, out string newPath);
                subdir.LockFile(newPath, label, value);
                return;
            }

            var fileToLock = GetEntity(path, true);

            if (fileToLock is VfsFile file)
            {

                if (value)
                {
                    file.LockFile(label);
                }
                else
                {
                    file.UnLockFile(label);
                }
                DateModified = DateTime.Now;

            }
            else
            {
                throw new VfsException("Directory locking is not allowed!");
            }


        }

        #region Assertions

        /// <summary>
        /// Throws VfsException if file can't be deleted
        /// </summary>
        private void AssertDeleteFile(string path)
        {
            var supposedFile = GetEntity(path, true);

            if (supposedFile is VfsFile file)
            {
                if (file.IsLocked())
                {
                    throw new VfsException($"Can't delete locked file {file.Path}!");
                }
            }
            else
            {
                throw new VfsException($"File {path} does not exist!");
            }
        }

        /// <summary>
        /// Throws VfsException if file or directory does not exist in path
        /// </summary>
        private void AssertExists(string name)
        {
            if (_entities.FirstOrDefault(f => f.Name == name) != null)
            {
                throw new VfsException($"Object {name} already exists!");
            }
        }

        /// <summary>
        /// Throws VfsException if entity is not directory or does not exist
        /// </summary>
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
        /// Throws VfsException if current directory contains locked files
        /// </summary>
        private void CheckRestrictionsLock()
        {
            var locks = _entities.Where(f => f is VfsFile).Cast<VfsFile>().Where(f => f.Locks.Any());
            if (locks.Any())
            {
                throw new VfsException($"Can't move or delete directory {Name} as it contains locked files!");
            }
            var dirs = _entities.Where(f => f is VfsDirectory).Cast<VfsDirectory>().Where(f => f._entities.Any());

            foreach (var dir in dirs)
            {
                dir.CheckRestrictionsLock();
            }
        }

         #endregion

        public void Clean()
        {
            _entities.Clear();
        }


    }
}


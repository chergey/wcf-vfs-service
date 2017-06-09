using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Emroy.Vfs.Service.Dto;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Interfaces;

namespace Emroy.Vfs.Service.Impl
{


    public abstract class VfsEntity
    {
        public string Name;
    }

    public class VfsFile : VfsEntity
    {
        public VfsFile(string name)
        {
            Name = name;
        }

        private List<string> _locks;





        public bool IsLocked(string name = null)
        {
            lock (_locks)
            {
                return _locks.Any(f => f == (name ?? f));
            }
        }

        public List<string> Locks
        {
            get
            {
                lock (_locks)
                    return _locks;
            }
        }


        public void LockFile(string label)
        {
            lock (_locks)
            {
                if (_locks.Contains(label))
                {
                    throw new VfsException("File is already locked!");
                }
                _locks.Add(label);
            }
        }

        public void UnLockFile(string label)
        {
            lock (_locks)
            {
                if (!_locks.Contains(label))
                {
                    throw new VfsException("File is not locked!");
                }
                _locks.Remove(label);
            }
        }

    }

    public class Directory : VfsEntity
    {

        public static Directory Root = new Directory(VfsSystem.DiskRoot);
        private readonly string _path;

        private List<VfsEntity> _entities = new List<VfsEntity>();

        /// <summary>
        /// Creates file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public VfsFile CreateFile(string path, VfsFileMode mode)
        {
            if (path.Contains(VfsSystem.SeparatorChar))
            {
                var dirs = path.Split(VfsSystem.SeparatorChar);

                var dir = _entities.FirstOrDefault(f => f.Name == dirs[0]) as Directory;
                AssertDirIsNull(path, dir);
                return dir.CreateFile(dirs[1], mode);
            }

            AssertFileExists(path);
            var file = new VfsFile(path);
            _entities.Add(file);
            return file;
        }



        /// <summary>
        /// Creates subdir
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Directory CreateSubDirectory(string path)
        {
            if (path.Contains(VfsSystem.SeparatorChar))
            {
                var dirs = path.Split(VfsSystem.SeparatorChar);

                var dir = _entities.FirstOrDefault(f => f.Name == dirs[0]) as Directory;
                AssertDirIsNull(path, dir);
                return dir.CreateSubDirectory(dirs[1]);
            }
            AssertDirExists(path);
            var file = new Directory(path);
            _entities.Add(file);
            return file;
        }



        /// <summary>
        /// Deletes subdir
        /// </summary>
        /// <param name="path"></param>
        public void DeleteSubDirectory(string path)
        {
            if (path.Contains(VfsSystem.SeparatorChar))
            {
                var dirs = path.Split(VfsSystem.SeparatorChar);
                AssertPathInvalid(path, dirs);
                var dir = _entities.FirstOrDefault(f => f.Name == path) as Directory;
                AssertDirIsNull(path, dir);
                dir.DeleteSubDirectory(dirs[2]);
            }
            AssertDirNotExists(path);
            _entities.RemoveAll(f => f.Name == path);
        }



        /// <summary>
        /// Deletes file
        /// </summary>
        /// <param name="path"></param>
        public void DeleteFile(string path)
        {
            if (path.Contains(VfsSystem.SeparatorChar))
            {
                var dirs = path.Split(VfsSystem.SeparatorChar);
                AssertPathInvalid(path, dirs);
                var dir = _entities.FirstOrDefault(f => f.Name == path) as Directory;
                AssertDirIsNull(path, dir);
                dir.DeleteSubDirectory(dirs[2]);
            }
            AssertFileNotExists(path);
            _entities.RemoveAll(f => f.Name == path);
        }



        /// <summary>
        /// Copies entity
        /// </summary>
        /// <param name="srcPath">file to copy</param>
        /// <param name="destPath">destination directory</param>
        public void CopyEntity(string srcPath, string destPath)
        {
            if (srcPath.Contains(VfsSystem.SeparatorChar))
            {
                var dirs = srcPath.Split(VfsSystem.SeparatorChar);
                var dir = _entities.FirstOrDefault(f => f.Name == dirs[0]) as Directory;
                AssertDirIsNull(srcPath, dir);
                dir.CopyEntity(dirs[1], destPath);
            }

            var file = _entities.FirstOrDefault(f => f.Name == srcPath);
            if (file == null)
            {
                throw new VfsException($"Object {srcPath} does not exist!");
            }
            CopyEntity(file, destPath);
        }


        /// <summary>
        /// Copies entity from this dir into dest
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="destPath"></param>
        private void CopyEntity(VfsEntity entity, string destPath)
        {
            if (destPath.Contains(VfsSystem.SeparatorChar))
            {
                var dirs = destPath.Split(VfsSystem.SeparatorChar);
                
                var dir = _entities.FirstOrDefault(f => f.Name == dirs[0]) as Directory;
                AssertDirIsNull(destPath, dir);
                dir.CopyEntity(entity, dirs[1]);
            }
            var destDir = _entities.FirstOrDefault(f => f.Name == destPath);
            if (destDir == null)
            {
                throw new VfsException($"Directory {destPath} does not exist!");
            }
            if (destDir is VfsFile )
            {
                throw new VfsException($"{destPath} can't point to file name!");
            }
            if (entity is VfsFile)
            {
                var obj = entity.Copy();
                _entities.Add(obj);
            }
            else
            {
                var dir = entity as Directory;
                dir._entities.ForEach( e => dir.CopyEntity(e, destPath ));
            }

        }

        #region Assertions

        private void AssertDirExists(string path)
        {
            if (_entities.FirstOrDefault(f => f is Directory && f.Name == path) != null)
            {
                throw new VfsException($"Directory {path} already exists!");
            }
        }
        private void AssertDirNotExists(string path)
        {
            if (_entities.FirstOrDefault(f => f is Directory && f.Name == path) == null)
            {
                throw new VfsException($"Directory {path} does not exist!");
            }
        }



        private void AssertFileNotExists(string path)
        {
            if (_entities.FirstOrDefault(f => f is VfsFile && f.Name == path) == null)
            {
                throw new VfsException($"File {path} does not exist!");
            }
        }


        private void AssertFileExists(string path)
        {
            if (_entities.FirstOrDefault(f => f is VfsFile && f.Name == path) != null)
            {
                throw new VfsException($"File {path} already exists!");
            }
        }


        private static void AssertDirIsNull(string path, Directory dir)
        {
            if (dir == null)
            {
                throw new VfsException($"Directory {path} does not exist");
            }
        }

        private static void AssertPathInvalid(string path, string[] dirs)
        {
            if (dirs.Length < 3)
            {
                throw new VfsException($"Incorrect path : {path}");
            }
        }

        #endregion


        public Directory(string name)
        {
            Name = name;
        }


        public string Path => _path;

        public Directory Parent
        {
            get
            {
                var path = _path;
                if (VfsSystem.IsRootedDir(path))
                {
                    path = path.TrimEnd(VfsSystem.SeparatorChar);
                }
                else if (path.EndsWith(VfsSystem.Separator))
                {
                    return null;
                }
                path = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal) + 1);
                return new Directory(path);
            }
        }



        public bool Contains(string name)
        {
            var dir = _entities.FirstOrDefault(f => f.Name == name);
            return dir != null;
        }


        public IEnumerable<VfsEntity> GetDirectoryContents()
        {
            return _entities;
        }

        //public IEnumerable<Entry> GetDirectoryContents()
        //{
        //    return GetDirectoryContentsInternal()
        //        .Where(f => f.Path != (VfsSystem.Separator + ".") && !f.Path.Contains(".."));
        //}


        //private IEnumerable<Entry> GetDirectoryContentsInternal()
        //{

        //    var entries = _dir.List();
        //    return (from entry in entries
        //            let node = Node.Load(_engine, entry.Value)
        //            select new Entry
        //            {
        //                Locks = node.Locks,
        //                IsDirectory = node.IsDirectory,
        //                AccessTime = node.Data.AccessTime,
        //                CreationTime = node.Data.CreationTime,
        //                Flags = node.Data.Flags,
        //                ModifyTime = node.Data.ModifyTime,
        //                Name = entry.Key,
        //                Path = _path + entry.Key,
        //                Size = node.Data.SizeByte,
        //                Data = node.Data,
        //                NodeIndex = node.Index
        //            }).ToList();
        //}




        //public void Rename(string oldName, string newName)
        //{
        //    VfsSystem.AssertNameValid(oldName);
        //    VfsSystem.AssertNameValid(newName);

        //    if (!_dir.Contains(oldName))
        //    {
        //        throw new DirectoryNotFoundException("No such dir!");
        //    }
        //    if (_dir.Contains(newName))
        //    {
        //        throw new IOException("Directory with the same name exists!");
        //    }
        //    if (!_dir.Rename(oldName, newName))
        //    {
        //        throw new VfsException("Could not rename directory!");
        //    }
        //}


        public void Check(string path, bool locks, bool subDirs)
        {
            if (path.Contains(VfsSystem.Separator))
            {
                var pathArray = path.Split(VfsSystem.SeparatorChar);

                var subDir = GetChildDirectory(pathArray[0]);
                var newPath = pathArray.Skip(1).Aggregate(string.Empty, (cur, s) => cur + VfsSystem.Separator + s).Substring(1);
                subDir.Check(newPath, locks, subDirs);
            }
            else
            {
                if (locks)
                    CheckRestrictionLock(path);

                if (subDirs)
                    CheckRestrictionSubdirs(path);

            }
        }

        private void CheckRestrictionSubdirs(string name)
        {
            VfsSystem.AssertNameValid(name);

            //if (!_dir.Contains(name))
            //{
            //    throw new VfsException($"Directory {Name} does not contain {name}");
            //}
            var dir = GetChildDirectory(name);

            if (dir != null)
            {
                var contents = dir.GetDirectoryContents();

                if (contents.Any(f => f is Directory))
                {
                    throw new InvalidOperationException("Directory contains subdirectories");
                }


            }
        }

        private void CheckRestrictionLock(string name)
        {
            VfsSystem.AssertNameValid(name);

            //if (!_dir.Contains(name))
            //{
            //    throw new VfsException($"Directory {Name} does not contain {name}");
            //}
            var dir = GetChildDirectory(name);

            if (dir != null)
            {
                var contents = dir.GetDirectoryContents();


                var file = contents.FirstOrDefault(f => f is VfsFile) as VfsFile;
                if (file != null && file.IsLocked())
                {

                    throw new InvalidOperationException("Directory contains locked files!");
                }
            }
        }




        //public void Delete(string path, bool ignoreLock, bool ignoreSubdirs, bool dir)
        //{

        //    Check(path, !ignoreLock, !ignoreSubdirs);

        //    if (path.Contains(VfsSystem.Separator))
        //    {
        //        string newPath;
        //        var subDir = GetChildDirectory(path, out newPath);
        //        subDir.Delete(newPath, ignoreLock, ignoreSubdirs, dir);
        //    }
        //    else
        //    {
        //        if (!ignoreLock && !ignoreSubdirs)
        //        {
        //            DeleteWithRestrictions(path, dir);
        //        }
        //        else
        //        {
        //            DeleteNoRestrictionsInternal(path, dir);
        //        }
        //    }
        //}


        /// <summary>
        /// Delete element in dir
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dir"></param>
        private void DeleteWithRestrictions(string name, bool dir)
        {
            CheckRestrictionSubdirs(name);
            CheckRestrictionLock(name);
            //delete it!
            //if (!_dir.Delete(name))
            //{
            //    throw new VfsException("Could not delete " + (dir ? "directory" : "file" + "!"));
            //}
        }

        //private void DeleteNoRestrictionsInternal(string name, bool dir)
        //{

        //    VfsSystem.AssertNameValid(name);

        //    if (!_dir.Contains(name))
        //    {
        //        throw new VfsException("Could not find " + (dir ? "directory" : "file" + "!"));
        //    }
        //    if (!_dir.Delete(name))
        //    {
        //        throw new VfsException("Could not delete " + (dir ? "directory" : "file" + "!"));
        //    }
        //}

        private Directory GetChildDirectory(string name)
        {
            var dir = _entities.FirstOrDefault(f => f.Name == name);

            return dir as Directory;
        }

    }
}


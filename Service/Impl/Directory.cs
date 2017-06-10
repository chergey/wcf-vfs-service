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

        public Directory Parent;

    }

    public class VfsFile : VfsEntity
    {
        public VfsFile(string name)
        {
            Name = name;
        }

        private List<string> _locks=new List<string>();





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
                var dir = TraverseSubDirs(path, out string newPath);
                return dir.CreateFile(newPath, mode);
            }

            AssertFileExists(path);
            var file = new VfsFile(path) { Parent = this };
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
                var dir = TraverseSubDirs(path, out string newPath);
                return dir.CreateSubDirectory(newPath);
            }
            AssertDirExists(path);
            var file = new Directory(path) { Parent = this };
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
                var dir = TraverseSubDirs(path, out string newPath);
                dir.DeleteSubDirectory(newPath);
            }
            AssertDeleteDir(path);
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
                var dir = TraverseSubDirs(path, out string newPath);
                dir.DeleteSubDirectory(newPath);
            }
            AssertFileDelete(path);
            _entities.RemoveAll(f => f.Name == path);
        }

        private Directory TraverseSubDirs(string path, out string newPath, int skip = 1)
        {

            var dirs = path.Split(VfsSystem.SeparatorChar);
            var dir = _entities.FirstOrDefault(f => f.Name == dirs[0]);
            AssertDirIsNull(dirs[0], dir);
            newPath = dirs.Skip(skip).Aggregate(string.Empty, (cur, s) => cur + VfsSystem.Separator + s).Substring(VfsSystem.Separator.Length);
            return dir as Directory;


        }

        /// <summary>
        /// Moves entity
        /// </summary>
        /// <param name="srcPath">file or directory to move </param>
        /// <param name="destPath">destination directory </param>
        public void MoveEntity(string srcPath, string destPath)
        {

            if (srcPath.Contains(VfsSystem.SeparatorChar))
            {
                var dir = TraverseSubDirs(srcPath, out string newPath);
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

            if (obj is Directory directory)
            {
                directory.CheckRestrictionsLock();
            }

            // destination
            var skip = destPath.Count(f => f == VfsSystem.SeparatorChar) + 1;
            Root.CopyEntity(obj, destPath, skip);


        }




        /// <summary>
        /// Copies entity
        /// </summary>
        /// <param name="srcPath">file  or directory to copy </param>
        /// <param name="destPath">destination directory (absolute path)</param>
        public void CopyEntity(string srcPath, string destPath)
        {
            if (srcPath.Contains(VfsSystem.SeparatorChar))
            {
                var dir = TraverseSubDirs(srcPath, out string newPath);
                dir.CopyEntity(newPath, destPath);
                return;
            }

            var obj = _entities.FirstOrDefault(f => f.Name == srcPath);
            if (obj == null)
            {
                throw new VfsException($"Object {srcPath} does not exist!");
            }
            // destination

            Root.CopyEntity(obj, destPath, destPath.Count(f => f == VfsSystem.SeparatorChar) + 1);


        }




        private void CopyEntity(VfsEntity entity, string destPath, int depth)
        {
            if (depth > 0)
            {
                var dir = TraverseSubDirs(destPath, out string newPath, depth == 1 ? 0 : 1);
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

        #region Assertions

        private void AssertDirExists(string path)
        {
            if (_entities.FirstOrDefault(f => f is Directory && f.Name == path) != null)
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
            var dir = _entities.FirstOrDefault(f => f is Directory && f.Name == path) as Directory;
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

        private void AssertFileDelete(string path)
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


        #endregion


        public Directory(string name)
        {
            Name = name;
        }



        public bool Contains(string name)
        {
            var dir = _entities.FirstOrDefault(f => f.Name == name);
            return dir != null;
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
            var dirs = _entities.Where(f => f is Directory).Cast<Directory>().ToArray();
            if (dirs.Length > 0)
            {
                throw new VfsException("Can't move or delete directories with subdirectories!");
            }
        }




      


        /// <summary>
        /// Delete element in dir
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dir"></param>
        private void DeleteWithRestrictions(string name, bool dir)
        {
            //  CheckRestrictionSubdirs(name);
            // CheckRestrictionLock(name);
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


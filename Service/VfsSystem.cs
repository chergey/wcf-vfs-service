using System.Collections.Generic;
using System.Linq;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Impl;
using Emroy.Vfs.Service.Interfaces;



namespace Emroy.Vfs.Service
{
    public class VfsSystem
    {
        /// <summary>
        /// VFS path separator
        /// </summary>
        public const string Separator = "/";

        /// <summary>
        /// the same as char
        /// </summary>
        public static char SeparatorChar => Separator.ToCharArray()[0];

        /// <summary>
        /// VFS root
        /// </summary>
        public const string DiskRoot = "/";


        /// <summary>
        /// Finds directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
       // public IDirectory FindDirectory(string path) => new Directory(_engine, path);

        /// <summary>
        /// Gets root
        /// </summary>
        public static Directory Root =>  Directory.Root;

        /*
        /// <summary>
        /// Get directories structure in the form:
        /// /
        /// DIR1
        ///   | DIR2
        ///   |  | file1.txt
        ///   |  | file2.txt [Locked by Me]
        /// </summary>
        /// <param name="curUserName"></param>
        /// <returns></returns>
        public string GetTextualRepresentation(string curUserName)
        {
            var dirList = GetRecursiveDirs();
            var strList = dirList.Aggregate(string.Empty, (current, s) =>
            current + new string('|', s.Key.Split(SeparatorChar).Length - 1)
            + s.Key.Substring(s.Key.LastIndexOf(SeparatorChar) + 1)
            + (s.Value.Any() ? " [Locked by " + string.Join(",", s.Value.Select(f => f == curUserName ? "Me" : f)) + "]" : "") + "\n");
            return strList;
        }
        
        public IDictionary<string, IEnumerable<string>> GetRecursiveDirs(string path = Separator)
        {
            var dirNames = new Dictionary<string, IEnumerable<string>> { [path] = { } }
                .Union(GetRecursiveDirsInternal(path))
                .ToDictionary(k => k.Key, v => v.Value);
            return dirNames;

        }
        */
        public static bool IsRootedDir(string path)
        {
            return path.EndsWith(Separator) && path.Length > 1;
        }




        /// <summary>
        /// Deletes directory (not used)
        /// <see cref="IDirectory.Delete"/>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /* 
        public void DeleteDirectory(string path)
        {
            var dir = FindDirectory(path);

            var contents = dir.GetDirectoryContents();
            if (contents.Any(f => f.IsDirectory))
            {
                throw new InvalidOperationException("Directory contains subdirectories");
            }

            if (contents.Any(f => f.LockCount > 0))
            {
                throw new InvalidOperationException("Directory contains locked files!");
            }

            dir.Parent.Delete(dir.Name);

        }
        */

        /*
               private IDictionary<string, IEnumerable<string>> GetRecursiveDirsInternal(string path = Separator)
               {
                   var dir = new Directory( path);
                   var list = dir.GetDirectoryContents();
                   var dirNames = new Dictionary<string, IEnumerable<string>>();
                   foreach (var a in list)
                   {
                       var locks = new List<string>(); //a.Locks.Where(f => f.Value).Select(f => f.Key);
                       if (a is VfsFile)
                           locks.AddRange(((VfsFile) a).Locks);

                    //   dirNames.Add(a.Path, locks);
                       if (a is Directory)
                       {
                           //var childDirs = GetRecursiveDirsInternal(a.Path);
                       //    dirNames = dirNames.Union(childDirs).ToDictionary(k => k.Key, v => v.Value);

                       }
                   }
                   return dirNames;

               }

               /// <summary>
               /// Gets file (creates or opens)
               /// </summary>
               /// <param name="path"></param>
               /// <param name="vfsFileMode"></param>
               /// <returns></returns>
              // public IFile CreateFile(string path, VfsFileMode vfsFileMode) => new VfsFile(_engine, path, vfsFileMode);

               /// <summary>
               /// Moves directory or file
               /// </summary>
               public void Move(string srcPath, string destPath)
               {
                   if (srcPath == DiskRoot)
                       throw new VfsException("Root directory can't be moved!");


                   // Copy(srcPath, destPath);
                   if (Register.Resolve(_engine, MakeDir(srcPath)) != null)
                   {
                       var parentDir = FindDirectory(srcPath).Parent;
                       var objectName = GetPathName(srcPath);
                       parentDir.Check(objectName, true, false);
                       CopyInternal(srcPath, destPath);
                       parentDir.Delete(objectName, false, true, true);
                   }
                  // else
                   {
                       var dir = FindDirectory(GetPathDirectory(srcPath));
                       var objectName = GetPathName(srcPath);
                       dir.Check(objectName, true, false);
                       CopyFile(srcPath, destPath);
                       dir.Delete(objectName, false, true, false);

                   }

               }

               /// <summary>
               /// Copies directory or file and creates destPath
               /// </summary>
               public void CopyWithCreation(string srcPath, string destPath)
               {
                   var dirPath = GetPathDirectory(destPath);
                   var parentDir = FindDirectory(dirPath);
                   var objectName = GetPathName(destPath);
                   parentDir.CreateSubDirectory(objectName);
                   CopyInternal(srcPath, destPath);
               }

               /// <summary>
               /// Copies directory or file
               /// </summary>
               public void Copy(string srcPath, string destPath)
               {

                   //if (Register.Resolve(_engine, MakeDir(srcPath)) != null)
                   {
                       CopyInternal(srcPath, destPath);
                   }
                 //  else
                   {
                       CopyFile(srcPath, destPath);

                   }
               }


               private void CopyInternal(string srcPath, string destPath)
               {
                   var srcDir = FindDirectory(srcPath);

                   var list = srcDir.GetDirectoryContents();

                   foreach (var a in list)
                   {
                       if (a.IsDirectory)
                       {
                           var subDir = a.Name;

                           var destDir = FindDirectory(destPath);
                           destDir.CreateSubDirectory(subDir);
                           var dir = destPath + Separator + subDir;
                           CopyInternal(a.Path, dir);
                       }
                       else
                       {
                           CopyFile(a.Path, destPath);
                       }
                   }
               }

               private void CopyFile(string srcPath, string destPath)
               {
                   var name = GetPathName(srcPath);
                   var destFile = new VfsFile( (destPath == Separator ? "" : destPath) + Separator + name, VfsFileMode.CreateNew);
                   var srcFile = new VfsFile( srcPath, VfsFileMode.Open);

                   srcFile.CopyTo(destFile);
               }
               */

        #region Helper methods for paths

        /// <summary>
        /// Checks if path is valid
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsPathValid(string path)
        {
            if (path.Length == 0)
            {
                return false;
            }
            if (path.ElementAt(0) != SeparatorChar)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if name is valid
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsNameValid(string name)
        {
            if (name.Length == 0)
            {
                return false;
            }
            if (name.Contains(Separator))
            {
                return false;
            }
            if (name == "." || name == "..")
            {
                return false;
            }
            if (name.EndsWith("."))
            {
                return false;
            }

            return true;
        }


        public static void AssertNameValid(string name)
        {
            if (!IsNameValid(name))
            {
                throw new VfsException("Name is invalid!");
            }
        }


        public static void AssertPathValid(string path)
        {
            if (!IsPathValid(path))
            {
                throw new VfsException("Path is invalid!");
            }
        }

        /// <summary>
        /// Retrieves 'file.txt' from '/dir/file.txt'
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathName(string path)
        {
            AssertPathValid(path);
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
            AssertPathValid(path);
            var pos = path.LastIndexOf(SeparatorChar);
            return path.Substring(0, pos + 1);
        }

        public static string GetFileName(string path)
        {
            var pos = path.LastIndexOf(SeparatorChar);
            return pos == -1 ? path : path.Substring(pos + 1);
        }

        private static string MakeDir(string srcPath)
        {
            return srcPath + Separator;
        }



        #endregion

    }
}

using System.Collections.Generic;
using System.Linq;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Impl;
using Emroy.Vfs.Service.Interfaces;



namespace Emroy.Vfs.Service
{
    public class VfsSystem
    {


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
       
        public static bool IsRootedDir(string path)
        {
            return path.EndsWith(Separator) && path.Length > 1;
        }

     

        #region Helper methods for paths




     



#endregion
 */

    }
}

using System.Linq;
using Emroy.Vfs.Service.Dto;

namespace Emroy.Vfs.Service.Impl
{
    public partial class VfsService 
    {
        /// <summary>
        /// Inner class for command processing
        /// TODO: not sure we need a separate class for each command
        /// </summary>

        internal class CommandImpl
        {

            public string SetCurDir(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);
                var path = command.Arguments[0];
                _users.Single(f => f.Name == command.UserName).CurDir = path;
                return $"Directory { AddLabel(path)} saved!";
            }

            public string CreateDir(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);
                var path = GetDir(command);

                VfsDirectory.Root.CreateSubDirectory(path);
                return $"Directory { AddLabel(path)} created!";
            }

            public string CreateFile(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);
                var path = GetDir(command);
                VfsDirectory.Root.CreateFile(path);
                return $"File { AddLabel(path)} created!";
            }

            /// <summary>
            /// Get directory structure in the form:
            /// ROOT:
            /// DIR1
            ///   | DIR2
            ///   |  | file1.txt
            ///   |  | file2.txt [Locked by Me]
            /// </summary>
           
            public string Print(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 0);
                var contents = VfsDirectory.Root.GetContents();
                var list = contents
                    .Aggregate(string.Empty, (cur, s)=> 
                cur + new string('|',s.Item1.Split(VfsDirectory.SeparatorChar).Length-1)
                + s.Item1.Substring(s.Item1.LastIndexOf(VfsDirectory.SeparatorChar) + 1)
                + (s.Item2.Any() ? " [Locked by " + 
                string.Join(",", s.Item2.Select(f => f == command.UserName ? "Me" : f)) + "]" : "") + "\n");

                return $"Disk structure:\n{VfsDirectory.DiskRoot}:\n" + list;

            }

           
            public string Move(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 2);
                AssertCurDir(command);

                GetDir(command, out var srcDir, out var destDir);

                VfsDirectory.Root.MoveEntity(srcDir, destDir);
                return $"Object {AddLabel(srcDir)} moved to {AddLabel(destDir)}";
            }

          
            public string Copy(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 2);
                GetDir(command, out var srcDir, out var destDir);

                VfsDirectory.Root.CopyEntity(srcDir, destDir);
                return $"Object {AddLabel(srcDir)} copied to {AddLabel(destDir)}!";
            }


            public string DeleteTree(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);
                AssertCurDir(command);

                var path = GetDir(command);

                VfsDirectory.Root.DeleteSubDirectory(path, true);

                return $"Directory {AddLabel(path)} deleted with subdirectories!";
            }

        
            public string DeleteDirectory(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);
                AssertCurDir(command);

                var path = GetDir(command);

                VfsDirectory.Root.DeleteSubDirectory(path, false);

                return $"Directory {AddLabel(path)} deleted!";
            }
           
            public string Lock(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);

                var path = GetDir(command);
                VfsDirectory.Root.LockFile(path, command.UserName, true);

                return $"File {AddLabel(path)} locked!";
            }

          
            public string UnLock(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);

                var path = GetDir(command);
                VfsDirectory.Root.LockFile(path, command.UserName, false);

                return $"File {AddLabel(path)} unlocked!";
            }

         
            public string DeleteFile(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);
                var path = GetDir(command);
                VfsDirectory.Root.DeleteFile(path);

                return $"File {AddLabel(path)} deleted!";

            }

            #region Helper methods

            /// <summary>
            /// Adds root directory to the path
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string AddLabel(string str)
            {
                return VfsDirectory.DiskRoot + VfsDirectory.Separator + str;
            }
          

            /// <summary>
            /// Returns current user directory
            /// </summary>
            /// <param name="userName">user name</param>
            public static string GetCurDir(string userName)
            {
                string dir;
                lock (_users)
                {
                    dir = _users.Single(f => f.Name == userName).CurDir;
                }
                return dir;
            }

            /// <summary>
            /// Returns directory in which command is to be performed
            /// </summary>
            /// <param name="command"></param>
            private static string GetDir(VfsCommand command)
            {
                // if full path
                if (command.Arguments[0].StartsWith(VfsDirectory.DiskRoot + VfsDirectory.Separator))
                {
                    return command.Arguments[0].TrimStart((VfsDirectory.DiskRoot + VfsDirectory.Separator).ToCharArray());

                }
                string dir = GetCurDir(command.UserName);

                if (dir == VfsDirectory.DiskRoot)
                {
                    return command.Arguments[0];
                }
                return dir + VfsDirectory.Separator + command.Arguments[0];
            }

            /// <summary>
            /// Returns two directories in which command (such as MOVE or COPY) is to be performed
            /// </summary>
            /// <param name="command"></param>
            /// <param name="srcDir"></param>
            /// <param name="destDir"></param>
            private static void GetDir(VfsCommand command, out string srcDir, out string destDir)
            {
                // if full path
                if (command.Arguments[0].StartsWith(VfsDirectory.DiskRoot + VfsDirectory.Separator) ||
                    command.Arguments[1].StartsWith(VfsDirectory.DiskRoot + VfsDirectory.Separator))
                {
                    srcDir = command.Arguments[0].TrimStart((VfsDirectory.DiskRoot + VfsDirectory.Separator).ToCharArray());
                    destDir = command.Arguments[1].TrimStart((VfsDirectory.DiskRoot + VfsDirectory.Separator).ToCharArray());
                    return;
                }

                string dir = GetCurDir(command.UserName);

                if (dir == VfsDirectory.DiskRoot)
                {
                    srcDir = command.Arguments[0];
                    destDir = command.Arguments[1];
                    return;
                }
                srcDir = dir + VfsDirectory.Separator + command.Arguments[0];
                destDir = dir + VfsDirectory.Separator + command.Arguments[1];
            }

            /// <summary>
            /// Throws if trying to move/delete current directory
            /// </summary>
            /// <param name="command"></param>
            private static void AssertCurDir(VfsCommand command)
            {
                if (GetCurDir(command.UserName) == command.Arguments[0])
                {
                    throw new VfsException("Deleting/moving current directory is not allowed!");
                }
            }

            /// <summary>
            /// Throws if number arguments doesn't match
            /// </summary>
            /// <param name="commandArguments"></param>
            /// <param name="count"></param>

            private void ValidateArguments(string[] commandArguments, int count)
            {
                if (commandArguments.Length != count)
                {
                    throw new VfsException($"Wrong number of arguments: expected {count}, got {commandArguments.Length}");
                }
            }

            #endregion
        }
    }
}
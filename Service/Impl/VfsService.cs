using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using Emroy.Vfs.Service.Dto;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Interfaces;

using KingAOP;
using Ninject;
using NLog;

namespace Emroy.Vfs.Service.Impl
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]

    public class VfsService : IVfsService
    {

        //initialize action list
        static readonly FuncList funcList = new FuncList();

        private readonly Dictionary<VfsCommandType, Func<VfsCommand, string>> _cmdFuncs
            = new Dictionary<VfsCommandType, Func<VfsCommand, string>>
            {
                [VfsCommandType.MD] = c => funcList.CreateDir(c),
                [VfsCommandType.CD] = c => funcList.SetCurDir(c),
                [VfsCommandType.PRINT] = c => funcList.Print(c),
                [VfsCommandType.MF] = c => funcList.CreateFile(c),
                [VfsCommandType.MOVE] = c => funcList.Move(c),
                [VfsCommandType.COPY] = c => funcList.Copy(c),
                [VfsCommandType.DEL] = c => funcList.DeleteFile(c),
                [VfsCommandType.RD] = c => funcList.DeleteDirectory(c),
                [VfsCommandType.DELTREE] = c => funcList.DeleteTree(c),
                [VfsCommandType.LOCK] = c => funcList.Lock(c),
                [VfsCommandType.UNLOCK] = c => funcList.UnLock(c),
            };




        static readonly List<VfsUser> _users = new List<VfsUser>();

        public static Logger AppLogger = LogManager.GetCurrentClassLogger();



        public static void DebugInfo(string info)
        {
            Console.WriteLine(info);
            AppLogger.Info(info);
        }

        /// <summary>
        /// Inner class for command processing
        /// TODO: not sure we need a separate class for each command
        /// </summary>

        internal class FuncList
        {

            public string SetCurDir(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);

                var dir = command.Arguments[0];
                if (dir.StartsWith(VfsDirectory.DiskRoot))
                {
                    dir = dir.TrimStart(VfsDirectory.DiskRoot.ToCharArray());
                }
                _users.Single(f => f.Name == command.UserName).CurDir = dir;

                return $"Directory {command.Arguments[0]} saved!";
            }



            [Intercept]
            public string CreateDir(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);
                var dir = GetDir(command);

                VfsDirectory.Root.CreateSubDirectory(dir);
                return $"Directory {command.Arguments[0]} created!";
            }

            [Intercept]

            public string CreateFile(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);
                var dir = GetDir(command);
                VfsDirectory.Root.CreateFile(dir);
                return
                    $"File {command.Arguments[0]} created!";
            }

            [Intercept]
            public string Print(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 0);
                var path = "";
                var list = VfsDirectory.Root.GetContents(path)
                    .Aggregate(string.Empty, (cur, s)=> 
                cur + new string('|',s.Item1.Split(VfsDirectory.SeparatorChar).Length-1)
                + s.Item1.Substring(s.Item1.LastIndexOf(VfsDirectory.SeparatorChar) + 1)
                + (s.Item2.Any() ? " [Locked by " + 
                string.Join(",", s.Item2.Select(f => f == command.UserName ? "Me" : f)) + "]" : "") + "\n");

                return "Disk structure:\nc:\n" + list;

            }

            [Intercept]
            public string Move(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 2);
                GetDir(command, out var srcDir, out var destDir);

                if (srcDir == command.Arguments[0])
                {
                    throw new VfsException("Moving current dir is not allowed!");
                }

                VfsDirectory.Root.MoveEntity(srcDir, destDir);
                return $"Object {command.Arguments[0]} moved to {command.Arguments[1]}";
            }

            [Intercept]
            public string Copy(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 2);
                GetDir(command, out var srcDir, out var destDir);


                VfsDirectory.Root.CopyEntity(srcDir, destDir);
                return $"Object {command.Arguments[0]} copied to {command.Arguments[1]}!";
            }



            [Intercept]
            public string DeleteTree(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);
                var dir = GetDir(command);

                if (dir == command.Arguments[0])
                {
                    throw new VfsException("Deleting current dir is not allowed!");
                }
                VfsDirectory.Root.DeleteSubDirectory(dir, true);


                return $"Directory {command.Arguments[0]} deleted with subdirectories!";
            }

            [Intercept]
            public string DeleteDirectory(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);

                var dir = GetDir(command);

                if (dir == command.Arguments[0])
                {
                    throw new VfsException("Deleting current dir is not allowed!");
                }

                VfsDirectory.Root.DeleteSubDirectory(dir, false);

                return $"Directory {command.Arguments[0]} deleted!";
            }



            [Intercept]
            public string Lock(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);

                var path = GetDir(command);
                VfsDirectory.Root.LockFile(path, command.UserName, true);

                return $"File {command.Arguments[0]} locked!";
            }

            [Intercept]
            public string UnLock(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);

                var path = GetDir(command);
                VfsDirectory.Root.LockFile(path, command.UserName, false);

                return $"File {command.Arguments[0]} unlocked!";
            }

            [Intercept]
            public string DeleteFile(VfsCommand command)
            {
                ValidateArguments(command.Arguments, 1);
                var dir = GetDir(command);
                VfsDirectory.Root.DeleteFile(dir);

                return $"File {command.Arguments[0]} deleted!";

            }

            private static string GetDir(VfsCommand command)
            {
                // if full path
                if (command.Arguments[0].StartsWith(VfsDirectory.DiskRoot + VfsDirectory.Separator))
                {
                    return command.Arguments[0].TrimStart((VfsDirectory.DiskRoot + VfsDirectory.Separator).ToCharArray());

                }
                string dir;
                lock (_users)
                {
                    dir = _users.Single(f => f.Name == command.UserName).CurDir;
                }

                if (dir == VfsDirectory.DiskRoot)
                {
                    return command.Arguments[0];
                }
                return dir + VfsDirectory.Separator + command.Arguments[0];
            }

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

                string dir;
                lock (_users)
                {
                    dir = _users.Single(f => f.Name == command.UserName).CurDir;
                }
                if (dir == VfsDirectory.DiskRoot)
                {
                    srcDir = command.Arguments[0];
                    destDir = command.Arguments[1];
                    return;
                }
                srcDir = dir + VfsDirectory.Separator + command.Arguments[0];
                destDir = dir + VfsDirectory.Separator + command.Arguments[1];
            }


            #region Helper methods

      
            private void ValidateArguments(string[] commandArguments, int count)
            {
                if (commandArguments.Length != count)
                {
                    throw new VfsException($"Wrong number of arguments: expected {count}, got {commandArguments.Length}");
                }
            }

            #endregion
        }


        [Intercept]
        public Response Connect(string userName)
        {


            lock (_users)
            {
                if (_users.Any(f => f.Name == userName))
                {
                    return new Response { Message = "User already exists!", Fail = true };
                }
                var user = new VfsUser
                {
                    Sid = OperationContext.Current.SessionId,
                    CurDir = VfsDirectory.DiskRoot,
                    Name = userName,
                    Callback = OperationContext.Current.GetCallbackChannel<IVfsServiceCallback>()
                };

                _users.Add(user);
                OperationContext.Current.Channel.Faulted += Channel_Close;
                OperationContext.Current.Channel.Closed += Channel_Close;

            }

            return new Response { Message = $"You [{userName}] are connected now!", Fail = false };
        }

        private void Channel_Close(object sender, EventArgs e)
        {
            lock (_users)
            {

                _users.RemoveAll(f => f.Sid == OperationContext.Current.SessionId);

            }
        }

        [Intercept]
        public Response PerformCommand(VfsCommand command)
        {
            Func<VfsCommand, string> action;
            if (_cmdFuncs.TryGetValue(command.Type, out action))
            {


                VfsUser user;
                lock (_users)
                {
                    user = _users.FirstOrDefault(f => f.Name == command.UserName);
                    if (user == null)
                    {
                        return new Response
                        {
                            Message = $"Internal error. User {command.UserName} is not registerd in VFS service!",
                            Fail = true
                        };
                        ;
                    }
                }

                var resp = new Response();
                try
                {
                    resp.Message = action(command);
                }
                catch (Exception ex)
                {
                    resp.Message = ex.Message;
                    resp.Fail = true;
                }

                _users.ToList()
                    .ForEach(u =>
                        u.Callback.Feedback(user.Name, $"User {user.Name} performs command:" +
                                                       $" {command.Type} {string.Join(" ", command.Arguments)}"
                        ));
                return resp;
            }

            return new Response
            {
                Message = $"Internal error. Server did not recognize command {command.Type}!",
                Fail = true
            };
        }

        [Intercept]
        public Response Disconnect(string userName)
        {
            lock (_users)
            {

                _users.RemoveAll(f => f.Name == userName);

            }

            return new Response { Message = $"You [{userName}] are disconnected now !", Fail = false };
        }



    }
}
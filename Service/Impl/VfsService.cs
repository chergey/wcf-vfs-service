using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using Emroy.Vfs.Service.Dto;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Interfaces;
/*
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
                [VfsCommandType.MF] = c => funcList.NewFile(c),
                [VfsCommandType.MOVE] = c => funcList.Move(c),
                [VfsCommandType.COPY] = c => funcList.Copy(c),
                [VfsCommandType.DEL] = c => funcList.DeleteFile(c),
                [VfsCommandType.RD] = c => funcList.DeleteDirectory(c),
                [VfsCommandType.DELTREE] = c => funcList.DeleteTree(c),
                [VfsCommandType.LOCK] = c => funcList.Lock(c),
                [VfsCommandType.UNLOCK] = c => funcList.UnLock(c),
            };


        static IStorage _storage;
        static VfsSystem _system;

        private const uint DeviceSize = 1 << 10 << 10 << 9;

        static readonly List<VfsUser> _users = new List<VfsUser>();

        public static Logger AppLogger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructor
        /// </summary>
        public VfsService()
        {
            //init storage
            var kernel = new StandardKernel();
            kernel.Bind<IStorage>()
                .To<MemoryStorage>()
                .WithConstructorArgument("sizeInBytes", DeviceSize);

            _storage= kernel.Get<IStorage>();

            _system = new VfsSystem(_storage);
            _system.Format(DeviceSize >> 10, 4);


        }

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
                var dir = GetCurDir(command);

                var directory=_system.FindDirectory(dir.Path + command.Arguments[0]);

                lock (_users)
                {
                    _users.Single(f => f.Name == command.UserName).CurDir = directory.Path;
                }
                return $"Directory {command.Arguments[0].BringToExternalForm()} saved!";
            }

            [Intercept]
            public string CreateDir(VfsCommand command)
            {
                var dir = GetCurDir(command);

                dir.CreateSubDirectoryInPath(command.Arguments[0]);
                return $"Directory {command.Arguments[0].BringToExternalForm()} created!";
            }

            [Intercept]

            public string NewFile(VfsCommand command)
            {
                var dir = GetUserDir(command.UserName);
                _system.CreateFile(dir + command.Arguments[0], VfsFileMode.CreateNew);
                return 
                    $"File {VfsSystem.GetFileName(command.Arguments[0]).BringToExternalForm()} created!";
            }

            [Intercept]
            public string Print(VfsCommand command)
            {
                return "Disk structure:\nc:\n"+ 
                    _system.GetTextualRepresentation(command.UserName).BringToExternalForm();
            }

            [Intercept]
            public string Move(VfsCommand command)
            {
                var dir = GetUserDir(command.UserName);

                if (dir == command.Arguments[0])
                {
                    throw new VfsException("Moving current dir is not allowed!");
                }
                SetSrcAndDestPaths(command, dir, out var srcPath, out var destPath);
                _system.Move(srcPath, destPath);
                return $"Object {command.Arguments[0].BringToExternalForm()} moved to {command.Arguments[1].BringToExternalForm()}";
            }

            [Intercept]
            public string Copy(VfsCommand command)
            {
                var dir = GetUserDir(command.UserName);

                SetSrcAndDestPaths( command, dir, out var srcPath, out var destPath);
                _system.Copy(srcPath, destPath);
                return $"Object {command.Arguments[0].BringToExternalForm()} copied to {command.Arguments[1].BringToExternalForm()}!";
            }

         

            [Intercept]
            public string DeleteTree(VfsCommand command)
            {
                var dir = GetUserDir(command.UserName);

                if (dir == command.Arguments[0])
                {
                    throw new VfsException("Deleting current dir is not allowed!");
                }

                var curDir = GetCurDir(command);

                curDir.Delete(command.Arguments[0], false, true, true);

                return $"Directory {command.Arguments[0].BringToExternalForm()} deleted with subdirectories!";
            }

            [Intercept]
            public string DeleteDirectory(VfsCommand command)
            {
                var dir = GetUserDir(command.UserName);

                if (dir == command.Arguments[0])
                {
                    throw new VfsException("Deleting current dir is not allowed!");
                }

                var curDir = GetCurDir(command);

                curDir.Delete(command.Arguments[0], false, false, true);

                return $"Directory {command.Arguments[0].BringToExternalForm()} deleted!";
            }

         

            [Intercept]
            public string Lock(VfsCommand command)
            {

                string dir = GetUserDir(command.UserName);
                var file = _system.CreateFile(dir + command.Arguments[0], VfsFileMode.Open);
                file.Lock(command.UserName, true);
                return $"File {command.Arguments[0]} locked!";
            }

            private static string GetUserDir(string userName)
            {
                string dir;
                lock (_users)
                {
                    dir = _users.Single(f => f.Name == userName).CurDir;
                }
                return dir;
            }
            [Intercept]
            public string UnLock(VfsCommand command)
            {

                var dir = GetUserDir(command.UserName);
                var file = _system.CreateFile(dir + command.Arguments[0], VfsFileMode.Open);
                file.Lock(command.UserName, false);

                return $"File {command.Arguments[0]} unlocked!";
            }

            [Intercept]
            public string DeleteFile(VfsCommand command)
            {
                var dir = GetCurDir(command);

                dir.Delete(command.Arguments[0], false, false, false);

                return $"File {command.Arguments[0]} deleted!";

            }

            #region Helper methods

            private static void SetSrcAndDestPaths(VfsCommand command, string dir, out string srcPath, out string destPath)
            {
                srcPath = command.Arguments[0];

                destPath = command.Arguments[1];

                if (!command.Arguments[0].StartsWith(VfsSystem.DiskRoot))
                {
                    srcPath = dir + command.Arguments[0];
                }
                if (!command.Arguments[1].StartsWith(VfsSystem.DiskRoot))
                {
                    destPath = dir + command.Arguments[1];
                }
            }


            private static IDirectory GetCurDir(VfsCommand command)
            {

                if (!command.Arguments[0].StartsWith(VfsSystem.DiskRoot))
                {  //if relative path

                    var dir = GetUserDir(command.UserName);
                    {
                        return _system.FindDirectory(dir);
                    }
                }
                command.Arguments[0] = command.Arguments[0].Substring(1);
                return _system.Root;
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
                    CurDir = "/",
                    Name = userName,
                    Callback = OperationContext.Current.GetCallbackChannel<IVfsServiceCallback>()
                };

                _users.Add(user);

            }

            return new Response { Message = $"You [{userName}] are connected now!", Fail = false };

        }

        [Intercept]
        public Response PerformCommand(VfsCommand command)
        {
            Func<VfsCommand, string> action;
            if (_cmdFuncs.TryGetValue(command.Type, out  action))
            {
                //normalize to internal form
                command.Arguments = command.Arguments.Select(f => f.BringToInternalForm())
                    .ToArray();

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
                        }; ;
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

                _users.ToList().ForEach(u =>
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
*/
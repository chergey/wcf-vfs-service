using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using Emroy.Vfs.Service.Dto;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Interfaces;

using NConcern;
using NLog;

namespace Emroy.Vfs.Service.Impl
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]

    public partial class VfsService : IVfsService
    {

        //initialize action list
       private static readonly CommandImpl CmdImpl = new CommandImpl();

        private readonly Dictionary<VfsCommandType, Func<VfsCommand, string>> _cmdFuncs
            = new Dictionary<VfsCommandType, Func<VfsCommand, string>>
            {
                [VfsCommandType.MD] = c => CmdImpl.CreateDir(c),
                [VfsCommandType.CD] = c => CmdImpl.SetCurDir(c),
                [VfsCommandType.PRINT] = c => CmdImpl.Print(c),
                [VfsCommandType.MF] = c => CmdImpl.CreateFile(c),
                [VfsCommandType.MOVE] = c => CmdImpl.Move(c),
                [VfsCommandType.COPY] = c => CmdImpl.Copy(c),
                [VfsCommandType.DEL] = c => CmdImpl.DeleteFile(c),
                [VfsCommandType.RD] = c => CmdImpl.DeleteDirectory(c),
                [VfsCommandType.DELTREE] = c => CmdImpl.DeleteTree(c),
                [VfsCommandType.LOCK] = c => CmdImpl.Lock(c),
                [VfsCommandType.UNLOCK] = c => CmdImpl.UnLock(c),
            };




        static readonly List<VfsUser> _users = new List<VfsUser>();

        public static Logger AppLogger = LogManager.GetCurrentClassLogger();


        public VfsService()
        {
            //add logging
            Aspect.Weave<Injector>(typeof(VfsService));
        }


    
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
            var channel = sender as IContextChannel;
            lock (_users)
            {
                _users.RemoveAll(f => f.Sid == channel?.SessionId);

            }
        }

     
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
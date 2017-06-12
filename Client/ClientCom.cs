using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Xml;
using Emroy.Vfs.Service.Dto;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Impl;
using Emroy.Vfs.Service.Interfaces;

namespace Emroy.Vfs.Client
{


    public class ClientCom : IClientCom
    {
        /// <summary>
        /// service uri for debug (to avoid typing it every time)
        /// </summary>
        private const string ServiceUriDefault = "http://localhost:9099";

        private DuplexChannelFactory<IVfsService> _channelFactory;

        private IVfsService _service;

        public delegate void NotificationHandler(object sender, NotificationEventArgs e);


        private string _userName;

        /// <summary>
        /// command map
        /// </summary>
        private readonly Dictionary<string, VfsCommandType> _commandMap = new Dictionary<string, VfsCommandType>
        {
            ["MOVE"] = VfsCommandType.MOVE,
            ["CD"] = VfsCommandType.CD,
            ["PRINT"] = VfsCommandType.PRINT,
            ["MF"] = VfsCommandType.MF,
            ["MD"] = VfsCommandType.MD,
            ["LOCK"] = VfsCommandType.LOCK,
            ["UNLOCK"] = VfsCommandType.UNLOCK,
            ["DELTREE"] = VfsCommandType.DELTREE,
            ["DEL"] = VfsCommandType.DEL,
            ["RD"] = VfsCommandType.RD,
            ["COPY"] = VfsCommandType.COPY
        };


        public void Process()
        {

            bool connected = false;
            while (true)
            {
                string cmd = GetInput();
                var inputCommand = cmd.Split(' ');
                if (string.Equals(inputCommand[0], "CONNECT", StringComparison.InvariantCultureIgnoreCase))
                {

                    if (inputCommand.Length < 3)
                    {
                        PrintHelp();
                        continue;
                    }

                    if (string.Equals(inputCommand[1], "test", StringComparison.InvariantCultureIgnoreCase))
                    {
                        inputCommand[1] = ServiceUriDefault;
                    }
                    if (connected)
                    {
                        Console.WriteLine("You are already connected!");
                        continue;
                    }
                    var error = ConnectToServer(inputCommand[1]);

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Could not connect to server. " + error);
                        continue;
                    }
                    VfsServiceCallback.NotificationEvent += VfsServiceCallback_NotificationEvent;
                    //User.Name = inputCommand[2];
                    _userName = inputCommand[2];
                    var response = _service.Connect(_userName);
                    Console.WriteLine(response.Message);

                    if (!response.Fail)
                    {
                        connected = true;
                    }

                }
                else if (string.Equals(inputCommand[0], "QUIT", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!connected)
                    {
                        Console.WriteLine("You are not connected.");
                        continue;
                    }


                    Thread.Sleep(200);
                    break;
                }
                //other commands
                else
                {

                    VfsCommandType type;
                    if (_commandMap.TryGetValue(inputCommand[0], out type) || _commandMap.TryGetValue(inputCommand[0].ToUpper(), out type))
                    {
                        if (!connected)
                        {
                            Console.WriteLine("You are not connected.");
                            continue;
                        }

                        try
                        {
                            Response response = _service.PerformCommand(new VfsCommand
                            {
                                Type = type,
                                UserName = _userName,
                                Arguments = inputCommand.Skip(1).ToArray()
                            });
                            Console.WriteLine(response.Message);
                        }
                        catch (CommunicationObjectFaultedException ex)
                        {
                            Console.WriteLine("Comminication lost:" + ex.Message);
                            break;
                        }
                        catch (ProtocolException ex)
                        {
                            Console.WriteLine("Protocol error: " + ex.Message);
                            break;
                        }


                    }

                    else if (string.Equals(inputCommand[0], "HELP", StringComparison.InvariantCultureIgnoreCase))
                    {
                        PrintHelp();
                    }
                    else
                    {
                        Console.WriteLine("Incorrect command: " + inputCommand[0]);
                        PrintHelp();
                    }
                }
            }

            Console.WriteLine("Leaving...");
            if (_userName != null)
            {
                _service.Disconnect(_userName);
                VfsServiceCallback.NotificationEvent -= VfsServiceCallback_NotificationEvent;
            }

        }

        private static string GetInput()
        {
            Console.Write(">>");
            return Console.ReadLine();
        }

        private void VfsServiceCallback_NotificationEvent(object sender, NotificationEventArgs e)
        {
            if (e.UserName != _userName)
            {
                Console.WriteLine(e.Message);
            }
        }


        public void PrintHelp()
        {
            Console.WriteLine(
                "Available commands:\n" +
                "Command name - Parameters - Description" +
                "CONNECT address user_name - Connect to VFS service \n" +
                "CD dir_name - Set current directory  \n " +
                "MD dir_name - Create directory \n " +
                "MF file_name - Create file \n " +
                "DEL file_name - Delete file \n " +
                "RD dir_name - Delete dir without subdirectories \n " +
                "DELTREE dir_name - Delete dir with subdirectories \n " +
                "COPY from_dir to_dir - Copy directory or files \n " +
                "MOVE from_dir to_dir - Move directory or files \n " +
                "LOCK file_name - Lock directory file \n " +
                "UNLOCK file_name - Unlock directory file \n " +
                "PRINT - Print VFS structure \n " +
                 "QUIT - Quits \n " +
                 "HELP - Prints help \n "
                 );
        }

        public string ConnectToServer(string serviceUrl)
        {
            try
            {
                var endpointAddress = new EndpointAddress(serviceUrl); 
                var instanceContext = new InstanceContext(new VfsServiceCallback());
                var binding = new WSDualHttpBinding
                {
                    SendTimeout = TimeSpan.FromSeconds(150),
                    ReceiveTimeout = TimeSpan.FromSeconds(150),
                    CloseTimeout = TimeSpan.FromSeconds(150),
                    OpenTimeout = TimeSpan.FromSeconds(1000),

                    MaxReceivedMessageSize = int.MaxValue,
                    MaxBufferPoolSize = int.MaxValue,

                    //MaxBufferSize = int.MaxValue,
                    ReaderQuotas = new XmlDictionaryReaderQuotas { MaxArrayLength = int.MaxValue, MaxBytesPerRead = int.MaxValue }
                };
                _channelFactory = new DuplexChannelFactory<IVfsService>(instanceContext, binding, endpointAddress);

                _service = _channelFactory.CreateChannel();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return null;
        }

        public class NotificationEventArgs : EventArgs
        {
            public string UserName;
            public string Message;
        }

        public class VfsServiceCallback : IVfsServiceCallback
        {
            public static event ClientCom.NotificationHandler NotificationEvent;

            public void Feedback(string login, string message)
            {
                NotificationEvent?.Invoke(this, new NotificationEventArgs { UserName = login, Message = message });
            }
        }
    }


}
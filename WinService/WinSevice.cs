using System.ServiceModel;
using System.ServiceProcess;
using Emroy.Vfs.Service.Impl;

namespace Emroy.Vfs.WinService
{
    public partial class WinSevice : ServiceBase
    {
        private ServiceHost _host;
        public WinSevice()
        {
            InitializeComponent();
            ServiceName = "VFS Service";
        }

        protected override void OnStart(string[] args)
        {
           // _host = new ServiceHost(typeof(VfsService));
          //  _host.Open();
        }

        protected override void OnStop()
        {
            _host.Close();
        }
    }
}

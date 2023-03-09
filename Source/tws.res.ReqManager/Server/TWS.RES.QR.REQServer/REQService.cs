using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using TWS.Networking;

/*
 * Ver de hacer esta clase más genérica, y que para utilizar el ReqInstance necesario, 
 * baste con definir un Facttory que lo devuelva, y ese Factory bien puede ser levantado
 * automáticamente buscando en los assemblies que deriven de una clase predefinida 
 * (Ej: IREQInstanceFactory) y por cada uno de ellos levante un listener.
 * Por ahora por simplicidad vas a usar la clase TWS.RES.MP.ReqInstance directamente 
 * dentro de este REQServer
 */
namespace TWS.RES.REQServer
{
    public partial class REQService : ServiceBase
    {
        private static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        private TServer mTServer;
        private List<string> mIPConnectedList = new List<string>();
        private DateTime mCurrentDate = DateTime.Now.Date;
        private int mMaxLicensedQty = 0;
        private string mLogFileName = Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName + ".log";

        public REQService()
        {
            InitializeComponent();

            string path = System.AppDomain.CurrentDomain.BaseDirectory;

            //ConfigMgr.Instance.Initialize(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName) + "\\TWS.MP.config");

            /*if (!LicenseMgr.Instance.ValidateLicense(LicenseMgr.GLOBAL_LICENSE, false, out mMaxLicensedQty))
            {
                LOG.Instance.LogEvent(mLogFileName, "RES REQ Service NOT LICENSED", VERBOSITY_LEVEL.INFO);
                mMaxLicensedQty = -1;
            }
            else
                LOG.Instance.LogEvent(mLogFileName, "RES REQ Service LICENSED, #Terminals: " + mMaxLicensedQty.ToString(), VERBOSITY_LEVEL.INFO);
            */
            
            this.ServiceName = "QR Promotion RES REQ Service";
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                mTServer = new TServer(null);
                mTServer.EventNewConnection += new NewConnectionEventHandler(OnTWSTServerEventNewConnection);                
                mTServer.StartServer();
                base.OnStart(args);

                LOG.Info("{Message}", $"RES REQ Service Started, listening on port {mTServer.ServerTcpPort}");
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }
        }

        protected override void OnStop()
        {
            try
            {
                mTServer.StopServer();
                base.OnStop();
                LOG.Info("{Message}", $"RES REQ Service Stopped");
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        protected override void OnPause()
        {
            try
            {
                mTServer?.StopServer();
                base.OnPause();
                LOG.Info("{Message}", $"RES REQ Service Paused");
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }
        }

        protected override void OnContinue()
        {
            try
            {
                mTServer.StartServer();
                base.OnContinue();
                LOG.Info("{Message}", $"RES REQ Service Continued");
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }
        }

        void OnTWSTServerEventNewConnection(object sender, NewConnectionEventArgs e)
        {
            try
            {
                //New Socket arrived
                LOG.Info("{Message}", $"RES REQ Service: New connection arrived from {e.Socket.RemoteEndPoint}");

                if (mCurrentDate.Date != DateTime.Now.Date)
                {
                    mIPConnectedList.Clear();
                    mCurrentDate = DateTime.Now.Date;
                }

                IPEndPoint remoteEndPoint = (IPEndPoint)e.Socket.RemoteEndPoint;

                if (!mIPConnectedList.Contains(remoteEndPoint.Address.ToString()) && mIPConnectedList.Count < mMaxLicensedQty)
                    mIPConnectedList.Add(remoteEndPoint.Address.ToString());

                //var path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase.ToString()).LocalPath);                
                List<IReqInstance> reqInstanceList = TWS.Helpers.AssemblyHelper.GetInterface<IReqInstance>(AppDomain.CurrentDomain.BaseDirectory);

                if (reqInstanceList.Count == 0)
                {
                    throw new Exception("No IReqInstance assembly found!");
                }
                //var reqInstance = new RESGeoITDReqInstance(e.Socket);

                foreach(var reqInstance in reqInstanceList)
                {
                    reqInstance.StartProcess(e.Socket);
                    new Thread(reqInstance.StartProcess).Start();
                }
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }
        }
    }
}

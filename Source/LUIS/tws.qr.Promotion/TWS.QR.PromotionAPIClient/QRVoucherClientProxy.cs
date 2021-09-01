using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWS.QR.PromotionClient
{
    public class QRVoucherClientProxy
    {
        private static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        public QRVoucherClientProxy()
        {
            string servicePath = AppDomain.CurrentDomain.BaseDirectory + "bin";
            //ConfigMgr.Instance.Initialize(servicePath + "\\TWS.Ordering.config");

            //Reinitialize NLOG confiuration with the read one from FI Config file.
            NLog.LogManager.Configuration = NLog.LogManager.LoadConfiguration(servicePath + "\\TWS.Ordering.config").Configuration;
            NLog.LogManager.ReconfigExistingLoggers();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using TWS.QR.PromotionClient.Interface;
using TWS.QR.Properties;

namespace TWS.QR.PromotionClient
{
    public class QRVoucherClientProxy
    {
        private static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        private IQRVoucherService mQRVoucherService = null;

        public QRVoucherClientProxy()
        {
            string servicePath = AppDomain.CurrentDomain.BaseDirectory + "bin";
            mQRVoucherService = QRServiceClient();

            //Reinitialize NLOG confiuration with the read one from FI Config file.
            //NLog.LogManager.Configuration = NLog.LogManager.LoadConfiguration(servicePath + "\\TWS.QR.config").Configuration;
            //NLog.LogManager.ReconfigExistingLoggers();
        }

        public ResponseMessage ValidateVoucher(string qrcode_)
        {
            ResponseMessage retVal = new ResponseMessage();

            LOG.Trace("ENTER");

            try
            {
                retVal = mQRVoucherService.ValidateVoucher(new RequestMessage() { VoucherCode = qrcode_ });
            }
            catch (EndpointNotFoundException ex)
            {
                retVal.Status = StatusCode.WS_ENDPOINT_ERROR;
                retVal.Message = ex.Message + "|" + ex.StackTrace;
                LOG.Fatal(ex, "{Message}", $"{ex.Message}");
            }
            catch (Exception ex)
            {
                retVal.Status = StatusCode.FAIL;
                retVal.Message = ex.Message + "|" + ex.StackTrace;
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }

            return retVal;
        }

        public ResponseMessage RedeemVoucher(string qrcode_, string reference_)
        {
            ResponseMessage retVal = new ResponseMessage();

            LOG.Trace("ENTER");

            try
            {
                retVal = mQRVoucherService.RedeemVoucher(new RequestMessage() { VoucherCode = qrcode_, Reference = reference_ });
            }
            catch (Exception ex)
            {
                retVal.Message = ex.Message + "|" + ex.StackTrace;
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }

            return retVal;
        }

        private IQRVoucherService QRServiceClient()
        {
            IQRVoucherService retVal = null;

            LOG.Trace("ENTER");

            try
            {
                string uri = Settings.Default.QR_PROMOTION_URL;
                WebChannelFactory<IQRVoucherService> channelFactory = new WebChannelFactory<IQRVoucherService>(new Uri(uri));
                retVal = channelFactory.CreateChannel();
            }
            finally
            {
                LOG.Trace("EXIT");
            }

            return retVal;
        }
    }
}

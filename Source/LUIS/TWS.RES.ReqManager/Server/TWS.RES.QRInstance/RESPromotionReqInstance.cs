using System;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using TWS.Marshaling;
using TWS.Networking;
using TWS.QR.PromotionClient;
using TWS.QR.PromotionClient.Interface;

namespace TWS.RES.QR
{
    public class RESPromotionReqInstance : ReqInstance
    {
        private static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();
        enum QR_MSG { VALIDATE = 1, REDEEM = 2 };

        public RESPromotionReqInstance(Socket socket_) : base(socket_)
        {
        }

        public RESPromotionReqInstance()
        {
        }

        protected override ReqMessage ProcessIncomingRequest(ReqMessage msg_)
        {
            ReqMessage retVal = new ReqMessage(null);

            switch (msg_.MessageType)
            {
                case (int)QR_MSG.VALIDATE:
                    retVal = ProcessQRValidate(msg_);
                    break;

                case (int)QR_MSG.REDEEM:
                    retVal = ProcessQRRedeem(msg_);
                    break;

                default:
                    LOG.Info("{Message}", $"Unknown Message type -> { msg_.MessageType} ID received.");
                    break;
            }

            return retVal;
        }
        
        private ReqMessage ProcessQRValidate(ReqMessage reqMsg_)
        {
            LOG.Trace("ENTER");

            ReqMessage retVal = new ReqMessage(null);

            try
            {
                string qrCode = ByteStream.PByteToPrimitive(reqMsg_.Body, 0, typeof(string)).ToString();


                LOG.Info("{Message}", $"Before calling ProcessQRValidate(), REQUEST:\r\nQR CODE = {qrCode}");

                QRVoucherClientProxy client = new QRVoucherClientProxy();
                var response = client.ValidateVoucher(qrCode);

                string strAux = $"response_code={response.Status}|{response.Message ?? ""}";

                retVal.MessageType = reqMsg_.MessageType;
                retVal.Body = ByteStream.ToPByte(strAux);
                retVal.BodySize = retVal.Body.Length;
                retVal.Checksum = retVal.GenerateChecksum();

                LOG.Info("{Message}", $"After calling Purchase(), RESPONSE:\r\nSTATUS={response.Status}, MSG={response.Message??"null"}");
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }

            return retVal;
        }

        private ReqMessage ProcessQRRedeem(ReqMessage reqMsg_)
        {
            LOG.Trace("ENTER");

            ReqMessage retVal = new ReqMessage(null);

            try
            {
                string bodyString = ByteStream.PByteToPrimitive(reqMsg_.Body, 0, typeof(string)).ToString();
                string[] bodyFiedsStr = bodyString.Split('|');
                string qrCode = bodyFiedsStr.Length > 0 ? bodyFiedsStr[0]: "";
                string reference = bodyFiedsStr.Length > 1 ? bodyFiedsStr[1]: "";

                LOG.Info("{Message}", $"Before calling ProcessQRRedeem(), REQUEST:\r\n{bodyString}");

                QRVoucherClientProxy client = new QRVoucherClientProxy();
                var response = client.RedeemVoucher(qrCode, reference);

                string strAux = $"response_code={response.Status}|{response.Message ?? ""}";

                retVal.MessageType = reqMsg_.MessageType;
                retVal.Body = ByteStream.ToPByte(strAux);
                retVal.BodySize = retVal.Body.Length;
                retVal.Checksum = retVal.GenerateChecksum();

                LOG.Info("{Message}", $"After calling Purchase(), RESPONSE:\r\nSTATUS={response.Status}, MSG={response.Message ?? "null"}");
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }

            return retVal;
        }
    }

    public class SaleId
    {
        public string collector_id { get; set; }
        public string pos_id { get; set; }
    }
}

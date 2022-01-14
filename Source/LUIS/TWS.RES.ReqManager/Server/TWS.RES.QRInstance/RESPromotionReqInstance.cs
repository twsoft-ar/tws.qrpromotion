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
        enum QRPROMOTION_MSG { VALIDATE = 1, REDEEM = 2 };

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
                case (int)QRPROMOTION_MSG.VALIDATE:
                    retVal = ProcessQRValidate(msg_);
                    break;

                case (int)QRPROMOTION_MSG.REDEEM:
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
            string strAux = "";

            try
            {
                string qrCode = ByteStream.PByteToPrimitive(reqMsg_.Body, 0, typeof(string)).ToString();

                LOG.Info("{Message}", $"Before calling ProcessQRValidate(), REQUEST:\r\nQR CODE = {qrCode}");

                QRVoucherClientProxy client = new QRVoucherClientProxy();
                ResponseMessage response = client.ValidateVoucher(qrCode);

                strAux = $"{(int)response.Status}|{response.Message}";
                if (response.Status == StatusCode.FAIL)
                {
                    strAux = $"{((int)response.Status)}|Fallo al validar QR"; //web client execution fail
                    LOG.Error("{Message}", $"Failed to validate qr code");
                }
                else if (response.Status == StatusCode.WS_ENDPOINT_ERROR)
                {
                    strAux = $"{(int)response.Status}|Sin conexion con Servicio QR"; //web client execution fail
                    LOG.Error("{Message}", $"No connection to QR WS EndPoint");
                }
                /*else if (response.Status == StatusCode.NOT_FOUND)
                {
                    strAux = $"{(int)response.Status}|QR No encontrado";
                    LOG.Error("{Message}", $"{strAux}");
                }*/
                else if (response.Status == StatusCode.OK)
                {
                    foreach (var item in response.Items)
                    {
                        strAux += $"|{((int)item.Type)}|{item.ObjectNumber}|{item.Quantity}";
                    }
                }
                LOG.Info("{Message}", $"After calling ValidateVoucher(), RESPONSE:\r\nSTATUS={response.Status}, MSG={response.Message??"null"}");
            }
            catch (Exception ex)
            {
                strAux = $"{StatusCode.FAIL}|Exception caught"; //web client execution fail
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }

            retVal.MessageType = reqMsg_.MessageType;
            retVal.Body = ByteStream.ToPByte(strAux);
            retVal.BodySize = retVal.Body.Length;
            retVal.Checksum = retVal.GenerateChecksum();

            return retVal;
        }

        private ReqMessage ProcessQRRedeem(ReqMessage reqMsg_)
        {
            LOG.Trace("ENTER");

            ReqMessage retVal = new ReqMessage(null);
            string strAux = "";

            try
            {
                string bodyStr = ByteStream.PByteToPrimitive(reqMsg_.Body, 0, typeof(string)).ToString();
                var bodyFields = bodyStr.Split('|');

                if (bodyFields.Length < 2)
                {
                    strAux = $"{StatusCode.FAIL}|Bad parameters";
                    //strAux = $"{-3}"; //bad parameters
                }
                else
                {
                    string qrCode = bodyFields[0].Trim();
                    string reference = bodyFields[1].Trim();

                    LOG.Info("{Message}", $"Before calling ProcessQRRedeem(), REQUEST:\r\nQR CODE = {qrCode}");

                    QRVoucherClientProxy client = new QRVoucherClientProxy();
                    ResponseMessage response = client.RedeemVoucher(qrCode, reference);

                    strAux = $"{response.Status}|{response.Message}";
                    if (response.Status != StatusCode.OK)
                    {
                        LOG.Error("{Message}", $"Failed to validate qr code. {response.Message}");
                    }
                    else
                    {
                        strAux += $"|{response.Detail.Status}|{response.Detail.Id}|{response.Voucher.Id}";
                    }

                    LOG.Info("{Message}", $"After calling ValidateVoucher(), RESPONSE:\r\nSTATUS={response.Status}, MSG={response.Message ?? "null"}");
                }
            }
            catch (Exception ex)
            {
                strAux = $"{StatusCode.FAIL}"; //exception
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }

            retVal.MessageType = reqMsg_.MessageType;
            retVal.Body = ByteStream.ToPByte(strAux);
            retVal.BodySize = retVal.Body.Length;
            retVal.Checksum = retVal.GenerateChecksum();

            return retVal;
        }
    }
}

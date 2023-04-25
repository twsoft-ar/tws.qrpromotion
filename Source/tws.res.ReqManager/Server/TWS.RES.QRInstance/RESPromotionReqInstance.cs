using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using TWS.Marshaling;
using TWS.Networking;
using TWS.QR.PromotionClient;
//using TWS.QR.PromotionClient.Interface;

namespace TWS.RES.QR
{
    public class RESPromotionReqInstance : ReqInstance
    {
        private static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();
        private string mDBFileName = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName) + "\\Transaction.db3";

        enum QRPROMOTION_MSG { VALIDATE = 1, REDEEM = 2, VOIDQR = 3 };

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

                case (int)QRPROMOTION_MSG.VOIDQR:
                    retVal = ProcessVoidQR(msg_);
                    break;

                default:
                    LOG.Info("{Message}", $"Unknown Message type -> { msg_.MessageType} ID received.");
                    break;
            }

            return retVal;
        }

        //Validation ReqInstance Handler
        private ReqMessage ProcessQRValidate(ReqMessage reqMsg_)
        {
            LOG.Trace("ENTER");

            ReqMessage retVal = new ReqMessage(null);

            try
            {
                //create DB file if not exists
                CreateTransactionsDatabase();

                string qrCode = ByteStream.PByteToPrimitive(reqMsg_.Body, 0, typeof(string)).ToString();

                LOG.Info("{Message}", $"Before calling ValidateQRFromDB(), REQUEST:\r\nQR CODE = {qrCode}");

                //look in DB
                var (terminalId, status) = ValidateQRFromDB(qrCode);
                var terminalSocket = ((System.Net.IPEndPoint)Socket.RemoteEndPoint).Address.Address;

                if (status == (int)StatusCode.BURNED || (terminalId != 0 && terminalId != terminalSocket))
                {
                    var strAux = $"{(int)StatusCode.BURNED}|{"Código ya utilizado"}";

                    retVal.MessageType = reqMsg_.MessageType;
                    retVal.Body = ByteStream.ToPByte(strAux);
                    retVal.BodySize = retVal.Body.Length;
                    retVal.Checksum = retVal.GenerateChecksum();

                    return retVal;
                }

                retVal = ValidateQRFromAPI(reqMsg_, out ResponseMessage webResponse);

                if (webResponse.status == (int)StatusCode.OK)
                {
                    TransactionDTO transactionDTO = new TransactionDTO()
                    {
                        ExpirationDate = webResponse.voucher.vencimiento,
                        PromotionType = (PromotionType)webResponse.voucher.tipoCodigo,
                        PromQty = (int)webResponse.voucher.cantidad,
                        QRCode = qrCode,
                        TerminalId = terminalSocket,
                        Timestamp = DateTime.Now,
                        State = PromotionStatus.Reserved,
                        Processed = false,
                        ExtraData1 = ""
                    };

                    var insertedRecords = InsertQRIntoDB(transactionDTO);

                    LOG.Info("{Message}", $"Inserted {insertedRecords} record{(insertedRecords == 1? "":"s")}");
                }
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                DoSynchronizeTransactions();
                LOG.Trace("EXIT");
            }

            return retVal;
        }

        //Redeem ReqInstance Handler
        private ReqMessage ProcessQRRedeem(ReqMessage reqMsg_)
        {
            ReqMessage retVal = new ReqMessage(null);

            try
            {
                string qrCode = ByteStream.PByteToPrimitive(reqMsg_.Body, 0, typeof(string)).ToString();
                var terminalSocket = ((System.Net.IPEndPoint)Socket.RemoteEndPoint).Address.Address;
                TransactionDTO transactionDTO = new TransactionDTO()
                {
                    QRCode = $"{ByteStream.PByteToPrimitive(reqMsg_.Body, 0, typeof(string))}".Split('|')[0],
                    State = PromotionStatus.Burned,
                    ExtraData1 = $"{ByteStream.PByteToPrimitive(reqMsg_.Body, 0, typeof(string))}",
                    TerminalId = terminalSocket
                };

                var count = SetQRStatusAndExtraData(transactionDTO);
                
                //override retval to return always succed operation. POS should never take care about redeem response
                retVal.MessageType = reqMsg_.MessageType;
                retVal.Body = ByteStream.ToPByte("0||1|0|0");
                retVal.BodySize = retVal.Body.Length;
                retVal.Checksum = retVal.GenerateChecksum();
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                DoSynchronizeTransactions();
                LOG.Trace("EXIT");
            }

            return retVal;
        }

        //Void ReqInstance Handler
        private ReqMessage ProcessVoidQR(ReqMessage reqMsg_)
        {
            ReqMessage retVal = new ReqMessage(null);
            var respBpdy = "";
            string[] bodyFields = { };

            try
            {
                bodyFields = ByteStream.PByteToPrimitive(reqMsg_.Body, 0, typeof(string)).ToString().Split('|');

                respBpdy = $"{(int)StatusCode.OK}";
                if (bodyFields.Length < 2)
                {
                    respBpdy = $"{(int)StatusCode.FAIL}|Bad parameters";
                }
                else
                {
                    var qrCode = bodyFields[0];
                    var terminal = ((System.Net.IPEndPoint)Socket.RemoteEndPoint).Address.Address;

                    TransactionDTO transactionDTO = new TransactionDTO()
                    {
                        QRCode = qrCode,
                        TerminalId = terminal,
                    };

                    var deletedRecords = DeleteQR(transactionDTO);
                    LOG.Info("{Message}", $"Deleted {deletedRecords} record{(deletedRecords == 1 ? "" : "s")}");
                }
            }
            catch (Exception ex)
            {
                respBpdy = $"{(int)StatusCode.FAIL}|Falla in eliminar promoción";
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                DoSynchronizeTransactions();
                LOG.Trace("EXIT");
            }

            retVal.MessageType = reqMsg_.MessageType;
            retVal.Body = ByteStream.ToPByte(respBpdy);
            retVal.BodySize = retVal.Body.Length;
            retVal.Checksum = retVal.GenerateChecksum();

            return retVal;
        }

        //API Validation way
        private ReqMessage ValidateQRFromAPI(ReqMessage reqMsg_, out ResponseMessage webResponse_)
        {
            LOG.Trace("ENTER");

            ReqMessage retVal = new ReqMessage(null);
            string responseBodyString = "";
            webResponse_ = null;

            try
            {
                string qrCode = ByteStream.PByteToPrimitive(reqMsg_.Body, 0, typeof(string)).ToString();

                APIValidateQRCode(qrCode, out webResponse_, out responseBodyString);
            }
            catch (Exception ex)
            {
                responseBodyString = $"{(int)StatusCode.FAIL}|Exception caught"; //web client execution fail
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }

            retVal.MessageType = reqMsg_.MessageType;
            retVal.Body = ByteStream.ToPByte(responseBodyString);
            retVal.BodySize = retVal.Body.Length;
            retVal.Checksum = retVal.GenerateChecksum();

            return retVal;
        }
        //DB Validation way
        private (long, long) ValidateQRFromDB(string qrCode_)
        {
            LOG.Trace("ENTER");

            (long, long) retVal = (0, 0);

            try
            {
                using (SQLiteConnection sqlConn = new SQLiteConnection($"Data Source={mDBFileName}"))
                {
                    sqlConn.Open();
                    using (SQLiteCommand sqlCmd = new SQLiteCommand(sqlConn))
                    {
                        sqlCmd.CommandText = "SELECT  `TerminalId`           AS TERMINAL_ID, \r\n" +
                                             "        `Status`               AS STATUS       \r\n" +
                                             "FROM    `Transaction`                          \r\n" +
                                             "WHERE   `QRCode`   =  @qrCode   AND            \r\n" +
                                             "        `PromType` <> @promType                \r\n";

                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@qrCode", DbType = System.Data.DbType.String, Value = qrCode_ });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@promType", DbType = System.Data.DbType.Int32, Value = 1 });

                        sqlCmd.CommandType = System.Data.CommandType.Text;
                        sqlCmd.CommandTimeout = 300;

                        using (SQLiteDataReader dr = sqlCmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
                        {
                            if (dr.Read())
                            {
                                retVal = ((long)dr["TERMINAL_ID"], (long)dr["STATUS"]);
                            }
                        }
                    }
                    sqlConn.Close();
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

            return retVal;
        }
        //API Call Validation Function
        private void APIValidateQRCode(string qrCode_, out ResponseMessage webResponse_, out string responseBodyString_)
        {
            LOG.Info("{Message}", $"Before calling ValidateVoucher(), REQUEST:\r\nQR CODE = {qrCode_}");

            QRVoucherWebClient client = new QRVoucherWebClient();
            webResponse_ = client.ValidateVoucher(qrCode_);

            responseBodyString_ = $"{(int)webResponse_.status}|{webResponse_.message}";
            if (webResponse_.status == (int)StatusCode.FAIL)
            {
                responseBodyString_ = $"{((int)webResponse_.status)}|Fallo al validar QR"; //web client execution fail
                LOG.Error("{Message}", $"Failed to validate qr code");
            }
            else if (webResponse_.status == (int)StatusCode.WS_ENDPOINT_ERROR)
            {
                responseBodyString_ = $"{(int)webResponse_.status}|Sin conexion con Servicio QR"; //web client execution fail
                LOG.Error("{Message}", $"No connection to QR WS EndPoint");
            }
            else if (webResponse_.status == (int)StatusCode.OK)
            {
                foreach (var item in webResponse_.items)
                {
                    responseBodyString_ += $"|{((int)item.tipo)}|{item.plu}|{item.cantidad}";
                }
            }
            LOG.Info("{Message}", $"After calling ValidateVoucher(), RESPONSE:\r\nSTATUS={webResponse_.status}, MSG={webResponse_.message ?? "null"}");
        }
        //API  Redeem way
        private ReqMessage RedeemQRFromAPI(ReqMessage reqMsg_, out ResponseMessage webResponse_)
        {
            LOG.Trace("ENTER");

            ReqMessage retVal = new ReqMessage(null);
            string responseBodyString = "";
            webResponse_ = null;

            try
            {
                string bodyStr = ByteStream.PByteToPrimitive(reqMsg_.Body, 0, typeof(string)).ToString();
                APIRedeemQRCode(bodyStr, out webResponse_, out responseBodyString);
            }
            catch (Exception ex)
            {
                responseBodyString = $"{(int)StatusCode.FAIL}"; //exception
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }

            retVal.MessageType = reqMsg_.MessageType;
            retVal.Body = ByteStream.ToPByte(responseBodyString);
            retVal.BodySize = retVal.Body.Length;
            retVal.Checksum = retVal.GenerateChecksum();

            return retVal;
        }
        //API Call Redeem Function
        private void APIRedeemQRCode(string requestBodyString_, out ResponseMessage webResponse_, out string responseBodyString_)
        {
            webResponse_ = null;
      
            var bodyFields = requestBodyString_.Split('|');

            if (bodyFields.Length < 5)
            {
                responseBodyString_ = $"{(int)StatusCode.FAIL}|Bad parameters";
            }
            else
            {
                string qrCode = bodyFields[0].Trim();
                string reference = bodyFields[1].Trim();
                decimal amount = Convert.ToDecimal(bodyFields[2]) / 100.0m;
                string store = bodyFields[3].Trim();
                string terminal = bodyFields[4].Trim();

                LOG.Info("{Message}", $"Before calling ProcessQRRedeem(), REQUEST:\r\nQR CODE = {qrCode}");

                QRVoucherWebClient client = new QRVoucherWebClient();
                webResponse_ = client.RedeemVoucher(qrCode,
                                                                reference,
                                                                (amount == 0.00m ? null : (decimal?)amount),
                                                                (store == "" ? null : store),
                                                                (terminal == "" ? null : terminal));

                responseBodyString_ = $"{webResponse_.status}|{webResponse_.message ?? ""}";
                if (webResponse_.status != (int)StatusCode.OK)
                {
                    LOG.Error("{Message}", $"Failed to validate qr code. {webResponse_.message}");
                }
                else
                {
                    responseBodyString_ += $"|{webResponse_.voucher.estado}|{webResponse_.detalle.id}|{webResponse_.voucher.id}";
                }

                LOG.Info("{Message}", $"After calling ValidateVoucher(), RESPONSE:\r\nSTATUS={webResponse_.status}, MSG={webResponse_.message ?? "null"}");
            }
        }
        private int PurgeTable()
        {
            LOG.Trace("ENTER");

            int retVal = 0;

            try
            {
                using (SQLiteConnection sqlConn = new SQLiteConnection($"Data Source={mDBFileName}"))
                {
                    sqlConn.Open();
                    using (SQLiteCommand sqlCmd = new SQLiteCommand(sqlConn))
                    {
                        sqlCmd.CommandText = "DELETE                               \r\n" +
                                             "FROM    `Transaction`                \r\n" +
                                             "WHERE   `Processed` = @processed OR  \r\n" +
                                             "        (`Timestamp` < @date     AND \r\n" +
                                             "         `Status`    = @status)";

                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@processed", DbType = System.Data.DbType.Boolean, Value = true});
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@date", DbType = System.Data.DbType.Date, Value = DateTime.Now.AddDays(-1) });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@status", DbType = System.Data.DbType.Int32, Value = 1 });

                        sqlCmd.CommandType = System.Data.CommandType.Text;
                        sqlCmd.CommandTimeout = 300;

                        retVal = sqlCmd.ExecuteNonQuery(System.Data.CommandBehavior.CloseConnection);
                    }
                    sqlConn.Close();
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

            return retVal;
        }
        private int SynchronizeBurnedRecords()
        {
            LOG.Trace("ENTER");

            int retVal = 0;

            try
            {
                SetQRLock();
                var burnedList = GetBurnedRecords(true);
                foreach (var voucher in burnedList)
                {
                    try
                    {
                        APIRedeemQRCode(voucher.ExtraData1, out ResponseMessage webResponse, out string responseBodyString);

                        if (webResponse.status == (int)StatusCode.OK || webResponse.status == (int)StatusCode.BURNED)
                        {
                            SetQRProcessedStatusFromDB(new TransactionDTO() 
                            { 
                                QRCode = voucher.QRCode, 
                                TerminalId = voucher.TerminalId,
                                Timestamp = voucher.Timestamp,
                                Processed = true,
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        LOG.Fatal(ex, "{Message}", "Exception caught.");
                    }

                    Thread.Sleep(0);
                }
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                SetQRLock(false);
                LOG.Trace("EXIT");
            }

            return retVal;
        }
        private List<TransactionDTO> GetBurnedRecords(bool locked_ = false)
        {
            LOG.Trace("ENTER");

            List<TransactionDTO> retVal  = new List<TransactionDTO>();

            try
            {
                using (SQLiteConnection sqlConn = new SQLiteConnection($"Data Source={mDBFileName}"))
                {
                    sqlConn.Open();
                    using (SQLiteCommand sqlCmd = new SQLiteCommand(sqlConn))
                    {
                        sqlCmd.CommandText = "SELECT  `QRCode`           AS QR_CODE,   \r\n" +
                                             "        `PromType`         AS PROM_TYPE, \r\n" +
                                             "        `TerminalId`       AS TERMINAL,  \r\n" +
                                             "        `Timestamp`        AS TIMESTAMP, \r\n" +
                                             "        `ExtraData1`       AS BURN_DATA  \r\n" +
                                             "FROM    `Transaction`                    \r\n" +
                                             "WHERE   `Processed` = @processed   AND   \r\n" +
                                             "        `LockId`    = @lockId      AND   \r\n" +
                                             "        `Status`    = @status            \r\n";

                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@processed", DbType = System.Data.DbType.Boolean, Value = false });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@lockId", DbType = System.Data.DbType.Int32, Value = locked_? Thread.CurrentThread.ManagedThreadId : 0});
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@status", DbType = System.Data.DbType.Int32, Value = (int)PromotionStatus.Burned });

                        sqlCmd.CommandType = System.Data.CommandType.Text;
                        sqlCmd.CommandTimeout = 300;


                        using (SQLiteDataReader dr = sqlCmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
                        {
                            while (dr.Read())
                            {
                                retVal.Add(new TransactionDTO()
                                {
                                    QRCode = (string)dr["QR_CODE"],
                                    ExtraData1 = (string)dr["BURN_DATA"],
                                    Timestamp = DateTime.ParseExact((string)dr["TIMESTAMP"], "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                                    PromotionType = (PromotionType)(long)dr["PROM_TYPE"],
                                    TerminalId = (long)dr["TERMINAL"]
                                });
                            }                               
                        }
                    }
                    sqlConn.Close();
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

            return retVal;
        }
        private void CreateTransactionsDatabase()
        {
            if (!File.Exists(mDBFileName))
            {
                SQLiteConnection.CreateFile(mDBFileName);

                using (SQLiteConnection sqlConn = new SQLiteConnection($"Data Source={mDBFileName}"))
                {
                    sqlConn.Open();
                    using (SQLiteCommand sqlCmd = new SQLiteCommand(sqlConn))
                    {
                        sqlCmd.CommandText = "CREATE TABLE `Transaction` " +
                                                            "(" +
                                                                "`TerminalId`	        BIGINT   NOT NULL    DEFAULT 0,\r\n" +
                                                                "`PromType`	            INTEGER  NOT NULL    DEFAULT 0,\r\n" +
                                                                "`QRCode`	            TEXT,                          \r\n" +
                                                                "`PromExpirationDate`   TEXT,                          \r\n" +
                                                                "`PromQty`	            INTEGER  NOT NULL    DEFAULT 0,\r\n" +
                                                                "`Timestamp`            TEXT,                          \r\n" +
                                                                "`ExtraData1`           TEXT,                          \r\n" +
                                                                "`ExtraData2`           TEXT,                          \r\n" +
                                                                "`ExtraData3`           TEXT,                          \r\n" +
                                                                "`ExtraData4`           TEXT,                          \r\n" +
                                                                "`Status`	            INTEGER  NOT NULL    DEFAULT 0,\r\n" +
                                                                "`LockId`	            BIGINT   NOT NULL    DEFAULT 0,\r\n" +
                                                                "`Processed`	        INTEGER  NOT NULL    DEFAULT 0 \r\n" +
                                                            ");                                                        \r\n";

                        sqlCmd.CommandType = System.Data.CommandType.Text;
                        sqlCmd.CommandTimeout = 300;
                        
                        int qty = sqlCmd.ExecuteNonQuery(System.Data.CommandBehavior.CloseConnection);
                    }
                    sqlConn.Close();
                }
            }
        }
        private int InsertQRIntoDB(TransactionDTO trans_)
        {
            LOG.Trace("ENTER");

            int retVal = 0;
            SQLiteConnection sqlConn = null;

            try
            {
                LOG.Info("{Message}", $"Inserting transaction: {trans_.Serialize()}");

                using (sqlConn = new SQLiteConnection($"Data Source={mDBFileName}"))
                {
                    sqlConn.Open();
                    using (SQLiteCommand sqlCmd = new SQLiteCommand(sqlConn))
                    {
                        sqlCmd.CommandText = "INSERT INTO `Transaction`              \r\n" +
                                             "           (`TerminalId`,              \r\n" +
                                             "            `PromType`,                \r\n" +
                                             "            `QRCode`,                  \r\n" +
                                             "            `PromExpirationDate`,      \r\n" +
                                             "            `PromQty`,                 \r\n" +
                                             "            `Timestamp`,               \r\n" +
                                             "            `Status`,                  \r\n" +
                                             "            `Processed`)               \r\n" +
                                             "SELECT      @terminalId,               \r\n" +
                                             "            @promType,                 \r\n" +
                                             "            @qrCode,                   \r\n" +
                                             "            @promExpirationDate,       \r\n" +
                                             "            @promQty,                  \r\n" +
                                             "            @timeStamp,                \r\n" +
                                             "            @state,                    \r\n" +
                                             "            @processed                 \r\n" +
                                             "WHERE NOT EXISTS (SELECT * FROM `Transaction` WHERE `QRCode`   = @qrCode AND" +
                                             "                                                    `PromType` = @promTypePerVoucher); \r\n";

                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@terminalId", DbType = System.Data.DbType.Int64, Value = trans_.TerminalId });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@promType", DbType = System.Data.DbType.Int32, Value = trans_.PromotionType });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@promTypePerVoucher", DbType = System.Data.DbType.Int32, Value = PromotionType.PerVoucher});
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@qrCode", DbType = System.Data.DbType.String, Value = trans_.QRCode });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@promExpirationDate", DbType = System.Data.DbType.DateTime, Value = trans_.ExpirationDate });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@promQty", DbType = System.Data.DbType.Int32, Value = trans_.PromQty });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@timeStamp", DbType = System.Data.DbType.String, Value = trans_.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff") });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@state", DbType = System.Data.DbType.Int32, Value = trans_.State });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@processed", DbType = System.Data.DbType.Boolean, Value = false });

                        sqlCmd.CommandType = System.Data.CommandType.Text;
                        sqlCmd.CommandTimeout = 300;

                        retVal = sqlCmd.ExecuteNonQuery(System.Data.CommandBehavior.CloseConnection);
                    }
                    sqlConn.Close();
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

            return retVal;
        }
        private int SetQRLock(bool lock_ = true)
        {
            LOG.Trace("ENTER");

            int retVal = 0;

            try
            {
                using (SQLiteConnection sqlConn = new SQLiteConnection($"Data Source={mDBFileName}"))
                {
                    sqlConn.Open();
                    using (SQLiteCommand sqlCmd = new SQLiteCommand(sqlConn))
                    {
                        sqlCmd.CommandText = "UPDATE `Transaction`           \r\n" +
                                             "SET    `LockId` = @lockIdVal   \r\n" +
                                             "WHERE  `Status` = @status AND  \r\n" +
                                             "       `lockId` = @lockId;     \r\n";

                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@status", DbType = System.Data.DbType.Int32, Value = PromotionStatus.Burned });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@lockIdVal", DbType = System.Data.DbType.Int32, Value = (lock_ ? Thread.CurrentThread.ManagedThreadId : 0) });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@lockId", DbType = System.Data.DbType.Int32, Value = (lock_ ? 0 : Thread.CurrentThread.ManagedThreadId) });

                        sqlCmd.CommandType = System.Data.CommandType.Text;
                        sqlCmd.CommandTimeout = 300;

                        retVal = sqlCmd.ExecuteNonQuery(System.Data.CommandBehavior.CloseConnection);
                    }
                    sqlConn.Close();
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

            return retVal;
        }
        private int SetQRStatusAndExtraData(TransactionDTO trans_)
        {
            LOG.Trace("ENTER");

            int retVal = 0;
            SQLiteConnection sqlConn = null;

            try
            {
                LOG.Info("{Message}", $"Changing transaction state: {trans_.Serialize()}");

                using (sqlConn = new SQLiteConnection($"Data Source={mDBFileName}"))
                {
                    sqlConn.Open();
                    using (SQLiteCommand sqlCmd = new SQLiteCommand(sqlConn))
                    {
                        sqlCmd.CommandText = "UPDATE `Transaction`                                     \r\n" +
                                             "SET    `Status`     = @status,                           \r\n" +
                                             "       `ExtraData1` = @extraData1                        \r\n" +
                                             "WHERE  `QRCode`     = @qrCode     AND                    \r\n" +
                                             "       `Status`    <> @status     AND                    \r\n" +
                                             "       `TerminalId` = @terminal   AND                    \r\n" +
                                             "       `Timestamp`  = (SELECT MIN(Timestamp)             \r\n" +
                                             "                      FROM  `Transaction`                \r\n" +
                                             "                      WHERE `QRCode`     = @qrCode   AND \r\n" +
                                             "                            `TerminalId` = @terminal AND \r\n" +
                                             "                            `Status`    <> @status)      \r\n" +
                                             "; \r\n";

                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@qrCode", DbType = System.Data.DbType.String, Value = trans_.QRCode });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@terminal", DbType = System.Data.DbType.Int64, Value = trans_.TerminalId });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@status", DbType = System.Data.DbType.Int32, Value = trans_.State });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@extraData1", DbType = System.Data.DbType.String, Value = trans_.ExtraData1 });

                        sqlCmd.CommandType = System.Data.CommandType.Text;
                        sqlCmd.CommandTimeout = 300;

                        retVal = sqlCmd.ExecuteNonQuery(System.Data.CommandBehavior.CloseConnection);
                    }
                    sqlConn.Close();
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

            return retVal;
        }
        private int DeleteQR(TransactionDTO trans_)
        {
            LOG.Trace("ENTER");

            int retVal = 0;
            SQLiteConnection sqlConn = null;

            try
            {
                LOG.Info("{Message}", $"Changing transaction state: {trans_.Serialize()}");

                using (sqlConn = new SQLiteConnection($"Data Source={mDBFileName}"))
                {
                    sqlConn.Open();
                    using (SQLiteCommand sqlCmd = new SQLiteCommand(sqlConn))
                    {
                        //delete only the oldest QRCODE (this way for PerPromotion codes)
                        sqlCmd.CommandText = "DELETE                                                   \r\n" +
                                             "FROM   `Transaction`                                     \r\n" +
                                             "WHERE  `QRCode`     = @qrCode   AND                      \r\n" +
                                             "       `TerminalId` = @terminal AND                      \r\n" +
                                             "       `Timestamp`  = (SELECT MIN(Timestamp)             \r\n" +
                                             "                      FROM  `Transaction`                \r\n" +
                                             "                      WHERE `QRCode`     = @qrCode   AND \r\n" +
                                             "                            `TerminalId` = @terminal)    \r\n" +
                                             "; \r\n";

                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@qrCode", DbType = System.Data.DbType.String, Value = trans_.QRCode });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@terminal", DbType = System.Data.DbType.Int64, Value = trans_.TerminalId });

                        sqlCmd.CommandType = System.Data.CommandType.Text;
                        sqlCmd.CommandTimeout = 300;

                        retVal = sqlCmd.ExecuteNonQuery(System.Data.CommandBehavior.CloseConnection);
                    }
                    sqlConn.Close();
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

            return retVal;
        }
        private int SetQRProcessedStatusFromDB(TransactionDTO trans_)
        {
            LOG.Trace("ENTER");

            int retVal = 0;
            SQLiteConnection sqlConn = null;

            try
            {
                LOG.Info("{Message}", $"Changing transaction state: {trans_.Serialize()}");

                using (sqlConn = new SQLiteConnection($"Data Source={mDBFileName}"))
                {
                    sqlConn.Open();
                    using (SQLiteCommand sqlCmd = new SQLiteCommand(sqlConn))
                    {
                        sqlCmd.CommandText = "UPDATE `Transaction`                 \r\n" +
                                             "SET    `Processed`  = @state         \r\n" +
                                             "WHERE  `QRCode`     = @qrCode   AND  \r\n" +
                                             "       `TerminalId` = @terminal AND  \r\n" +
                                             "       `Timestamp`  = @timestamp;    \r\n";

                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@qrCode", DbType = System.Data.DbType.String, Value = trans_.QRCode });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@state", DbType = System.Data.DbType.Boolean, Value = trans_.Processed });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@terminal", DbType = System.Data.DbType.Int64, Value = trans_.TerminalId });
                        sqlCmd.Parameters.Add(new SQLiteParameter() { ParameterName = "@timestamp", DbType = System.Data.DbType.String, Value = trans_.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff") });

                        sqlCmd.CommandType = System.Data.CommandType.Text;
                        sqlCmd.CommandTimeout = 300;

                        retVal = sqlCmd.ExecuteNonQuery(System.Data.CommandBehavior.CloseConnection);
                    }
                    sqlConn.Close();
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

            return retVal;
        }
        private void DoSynchronizeTransactions()
        {
            new Thread(() => { SynchronizeBurnedRecords(); PurgeTable(); }).Start();
        }
    }
    internal enum PromotionType
    {
        PerPromotion = 1,
        PerVoucher = 2,        
    }
    internal enum PromotionStatus
    {
        Reserved = 1,
        Burned = 5
    }
    internal struct TransactionDTO
    {
        public long TerminalId { get; set; }
        public PromotionType PromotionType { get; set; }
        public string QRCode { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int PromQty { get; set; }
        public string ExtraData1 { get; set; }
        public string ExtraData2 { get; set; }
        public string ExtraData3 { get; set; }
        public string ExtraData4 { get; set; }
        public DateTime Timestamp { get; set; }
        public PromotionStatus State { get; set; }
        public bool Processed { get; set; }
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    };
}

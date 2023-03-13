using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TWS.QR.VoucherWebClient.Properties;

namespace TWS.QR.PromotionClient
{
    public class QRVoucherWebClient
    {
        private static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        public ResponseMessage ValidateVoucher(string qrcode_)
        {
            ResponseMessage retVal = new ResponseMessage();

            LOG.Trace("ENTER");

            try
            {
                retVal = CallFiservAPI<ResponseMessage>(new RequestMessage() { VoucherCode = qrcode_ }, Settings.Default.QR_VALIDATE_RESOURCE);
            }
            catch (EndpointNotFoundException ex)
            {
                retVal.status = (int)StatusCode.WS_ENDPOINT_ERROR;
                retVal.message = ex.Message + "|" + ex.StackTrace;
                LOG.Fatal(ex, "{Message}", $"{ex.Message}");
            }
            catch (Exception ex)
            {
                retVal.status = (int)StatusCode.FAIL;
                retVal.message = ex.Message + "|" + ex.StackTrace;
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }

            return retVal;
        }

        public ResponseMessage RedeemVoucher(string qrcode_, string reference_, decimal? amount_ = null, string store_ = null, string terminal_ = null)
        {
            ResponseMessage retVal = new ResponseMessage();

            LOG.Trace("ENTER");

            try
            {
                var request = new RequestMessage()
                {
                    VoucherCode = qrcode_,
                    Reference = reference_,
                    Amount = amount_,
                    Store = store_,
                    Terminal = terminal_
                };

                retVal = CallFiservAPI<ResponseMessage>(request, Settings.Default.QR_REDEEM_RESOURCE);
            }
            catch (Exception ex)
            {
                retVal.message = ex.Message + "|" + ex.StackTrace;
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Trace("EXIT");
            }

            return retVal;
        }

        public T CallFiservAPI<T>(object body_, string resource_) where T : ResponseMessage, new()
        {
            T retVal = default(T);

            LOG.Debug("ENTER");

            try
            {
                //create Headers
                var headers = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Accept", "application/json"),
                    new KeyValuePair<string, string>("Content-Type", "application/json")
                };

                //create Body
                JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                };
                var body = JsonConvert.SerializeObject(body_, jsonSettings);

                LOG.Info("{Message}", $"API REQUEST = {body}");

                //get JSON response
                var jsonResponse = HttpCall(Method.POST, Settings.Default.QR_PROMOTION_URL, resource_, headers, null, body, out string errMsg);

                LOG.Info("{Message}", $"API RESPONSE = {jsonResponse}");

                if (jsonResponse != "")
                    retVal = JsonConvert.DeserializeObject<T>(jsonResponse);
                else
                {
                    retVal = new T();
                    retVal.status = -1;
                    retVal.message = $"{(errMsg != "" ? errMsg : "Unknown error")}";
                }

            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                LOG.Debug("EXIT");
            }

            return retVal;
        }

        private string HttpCall(RestSharp.Method method_, string url_, string resource_, KeyValuePair<string, string>[] headers_, KeyValuePair<string, string>[] parameters_, object body_, out string errorMsg_)
        {
            string retVal = "";
            errorMsg_ = "";

            LOG.Debug("ENTER");

            try
            {
                //create Request
                RestRequest request = new RestRequest(resource_) { Method = method_ };

                //add Headers
                foreach (var header in headers_ ?? new KeyValuePair<string, string>[] { })
                    request.AddHeader(header.Key, header.Value);

                //add Parameters
                foreach (var param in parameters_ ?? new KeyValuePair<string, string>[] { })
                    request.AddQueryParameter(param.Key, param.Value);

                //Add body
                if (body_ != null)
                    request.AddParameter("application/json", body_, ParameterType.RequestBody);

                //create Rest Client and execute Http request
                var restClient = new RestClient(url_);
                var restResponse = restClient.Execute(request);

                //get JSON response
                errorMsg_ = restResponse.ErrorMessage ?? "";
                retVal = restResponse.Content;
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
                errorMsg_ = ex.Message;
            }
            finally
            {
                LOG.Debug("EXIT");
            }

            return retVal;
        }

        void Test()
        {
            var client = new RestClient("http://201.216.251.179:7506");
            client.Timeout = 10;

            var request = new RestRequest("/api/RedeemVoucher", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var body = @"{
                            " + "\n" +
                                        @"    ""VoucherCode"":""ZPBPWUZD"",
                            " + "\n" +
                                        @"    ""Reference"":""REF 123456"",
                            " + "\n" +
                                        @"    ""Amount"": 10.85,
                            " + "\n" +
                                        @"    ""Store"":""Local 123"",
                            " + "\n" +
                                        @"    ""Terminal"":""WS01""
                            " + "\n" +
            @"}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            RestResponse response = (RestResponse)client.Execute(request);
            Console.WriteLine(response.Content);
        }
    }
    public class RequestMessage
    {
        public string VoucherCode { get; set; }
        public string Reference { get; set; }
        public decimal? Amount { get; set; }
        public string Store { get; set; }
        public string Terminal { get; set; }
    }

    public class ResponseMessage
    {
        public int status { get; set; }
        public Voucher voucher { get; set; }
        public Item[] items { get; set; }
        public Detalle detalle { get; set; }
        public string message { get; set; }
    }

    public class Voucher
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public DateTime? vencimiento { get; set; }
        public int cantidad { get; set; }
        public int tipoCodigo { get; set; }
        public DateTime? fechaHoraCreacion { get; set; }
        public DateTime? fechaHoraModificacion { get; set; }
        public int estado { get; set; }
        public int tipoPromocionId { get; set; }
        public string creadoPor { get; set; }
        public object codigo { get; set; }
    }

    public class Detalle
    {
        public int id { get; set; }
        public int voucherId { get; set; }
        public string codigo { get; set; }
        public string infoQuemado { get; set; }
        public DateTime? fechaHoraQuemado { get; set; }
        public float precio { get; set; }
        public string local { get; set; }
        public string terminal { get; set; }
    }

    public class Item
    {
        public int id { get; set; }
        public int voucherId { get; set; }
        public int tipo { get; set; }
        public string plu { get; set; }
        public string precio { get; set; }
        public int cantidad { get; set; }
        public string descripcion { get; set; }
    }

    public enum ItemType
    {
        MenuItem = 1,
        Discount = 2,
    }

    public enum StatusCode
    {
        OK = 0,
        NOT_FOUND = 1,
        BURNED = 5,
        WS_ENDPOINT_ERROR = -2,
        FAIL = -1,

        /*
        WS_ENDPOINT_ERROR = -2,
        FAIL = -1,
        NOT_FOUND = 0,
        OK = 1,
        */
    }

}

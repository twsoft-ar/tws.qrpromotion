using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace TWS.QR.PromotionClient.Interface
{
    [ServiceContract]
    public interface IQRVoucherService
    {
        [WebInvoke(Method = "POST",
                    RequestFormat = WebMessageFormat.Json,
                    ResponseFormat = WebMessageFormat.Json,
                    BodyStyle = WebMessageBodyStyle.Bare,
                    UriTemplate = "ValidateVoucher")]
        //[return: MessageParameter(Name = "response_message")]
        ResponseMessage ValidateVoucher(RequestMessage msg_);
    }

    [DataContract]
    public class RequestMessage
    {
        [DataMember(IsRequired = true)]
        public string VoucherCode { get; set; }
        [DataMember(IsRequired = false)]
        public string Reference { get; set; }
    }

    public class ResponseMessage
    {
        [DataMember(Name = "status")]
        public string Status{ get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}

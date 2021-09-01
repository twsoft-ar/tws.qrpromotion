using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace TWS.QR.PromotionClient.Interface
{
    [ServiceContract]
    public interface IQRVoucherService
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
                    RequestFormat = WebMessageFormat.Json,
                    ResponseFormat = WebMessageFormat.Json,
                    BodyStyle = WebMessageBodyStyle.Bare,
                    UriTemplate = "ValidateVoucher")]
        //[return: MessageParameter(Name = "response_message")]
        ResponseMessage ValidateVoucher(RequestMessage msg_);

        [OperationContract]
        [WebInvoke(Method = "POST",
                    RequestFormat = WebMessageFormat.Json,
                    ResponseFormat = WebMessageFormat.Json,
                    BodyStyle = WebMessageBodyStyle.Bare,
                    UriTemplate = "RedeemVoucher")]
        ResponseMessage RedeemVoucher(RequestMessage msg_);
    }

    [DataContract]
    public class RequestMessage
    {
        [DataMember(IsRequired = true)]
        public string VoucherCode { get; set; }
        [DataMember(IsRequired = false)]
        public string Reference { get; set; }
    }

    [DataContract]
    public class ResponseMessage
    {
        [DataMember(Name = "status", IsRequired = false)]
        public ErrorCode Status { get; set; }
        
        [DataMember(Name = "message", IsRequired = false)]
        public string Message { get; set; }
        
        [DataMember(Name = "voucher", IsRequired = false)]
        public VoucherDef Voucher { get; set; }
        
        [DataMember(Name = "items", IsRequired = false)]
        public Item[] Items { get; set; }
        
        [DataMember(Name = "detalle", IsRequired = false)]
        public TransactionDetail Detail { get; set; }
    }

    [DataContract]
    public class VoucherDef
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "descripcion")]
        public string Description { get; set; }

        [IgnoreDataMember]
        public DateTime ExpirationDate 
        {
            get { return DateTime.Parse(ExpirationDateStr); }
        }

        [DataMember(Name = "vencimiento")]
        public string ExpirationDateStr { get; set; }

        [DataMember(Name = "imagen", IsRequired = false)]
        public object Picture { get; set; }

        [DataMember(Name = "cantidad", IsRequired = false)]
        public int Quantity { get; set; }
        
        [DataMember(Name = "qrCode")]
        public string QRCode { get; set; }
    }

    [DataContract]
    public class Item
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }
        
        [DataMember(Name = "voucherId")]
        public int VoucherId { get; set; }

        [DataMember(Name = "tipo")]
        public ItemType Type { get; set; }

        [DataMember(Name = "plu")]
        public int ObjectNumber { get; set; }

        [DataMember(Name = "precio")]
        public string Price { get; set; }

        [DataMember(Name = "cantidad", IsRequired = false)]
        public int Quantity { get; set; }
    }

    [DataContract]
    public class TransactionDetail
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "voucherId")]
        public int VoucherId { get; set; }

        [IgnoreDataMember]
        public DateTime CreationDate { get { return DateTime.Parse(CreationDateStr); } }

        [DataMember(Name = "fechaHoraGeneracion")]
        public string CreationDateStr { get; set; }

        [IgnoreDataMember]
        public DateTime BurningDate { get { return DateTime.Parse(BurningDateStr); } }

        [DataMember(Name = "fechaHoraQuemado")]
        public string BurningDateStr { get; set; }

        [DataMember(Name = "infoQuemado")]
        public string Reference { get; set; }

        [DataMember(Name = "estado")]
        public int Status { get; set; }

        [DataMember(Name = "qr")]
        public string QRCode { get; set; }
    }

    public enum ItemType
    {
        MenuItem = 1,
        Discount = 2,
    }

    public enum ErrorCode
    { 
        OK = 1,
    }
}

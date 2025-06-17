namespace PPC.Service.ModelResponse.MemberShipResponse
{
    public class MemberBuyMemberShipResponse
    {
        public string MemberShipId { get; set; }
        public string MemberId { get; set; }
        public double? Price { get; set; }
        public double? Remaining { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
    }
}
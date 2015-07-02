namespace SSO
{
    public class WebMessage
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public int ErrCode { get; set; }

        public int Count { get; set; }

        public object ApiParamObj { get; set; }



    }
}
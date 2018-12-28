namespace Web.ZhiXiao.Areas.BonusApp.Models.UI
{
    public class Response
    {
        public Response() : this(false) { }

        public Response(bool result)
        {
            this.Result = result;
        }

        public Response(bool result, string message, object data = null)
        {
            this.Result = result;
            this.Message = message;
            this.Data = data;
        }

        public bool Result { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }
    }
}
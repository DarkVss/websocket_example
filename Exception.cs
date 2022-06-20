namespace WebSocket_example{
    public class Exception : System.Exception{
        public Exception(string message) : base(message){ }
        public Exception(string message, System.Exception innerException) : base(message, innerException){ }
    }
}
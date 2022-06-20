namespace WebSocket_example.Callback{
    /// <summary>
    /// Callback on new client connection
    /// </summary>
    /// <param name="headers"></param>
    /// <returns>Return callback status and message or UID on fail and success(send null UID if need to use autogenerate)</returns>
    public delegate (bool, string) OnConnect(Headers headers);
}
using System.IO;

namespace WebSocket_example {
    public static class MemoryStreamExtensions
    {
        public static void Append(this MemoryStream stream, byte value)
        {
            stream.Append(new[] { value });
        }
        public static void Append(this MemoryStream stream, int value)
        {
            Append(stream,(byte) value);
        }

        public static void Append(this MemoryStream stream, byte[] values)
        {
            stream.Write(values, 0, values.Length);
        }
    }
}
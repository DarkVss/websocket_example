using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket_example{
    public class Headers{
        protected readonly string OriginalHeadersString;

        public readonly Dictionary<string, string> Fields = new Dictionary<string, string>();

        public readonly string Method = null;
        public readonly string Url = null;
        public readonly string HttpVersion = null;
        public readonly string UserAgent = null;
        public readonly Dictionary<string, string> Cookies = new Dictionary<string, string>();

        protected Headers(string headers){
            this.OriginalHeadersString = headers;

            string[] rows = this.OriginalHeadersString.Split("\r\n", System.StringSplitOptions.RemoveEmptyEntries);

            if (rows.Length >= 1){
                string[] urlStrings = rows[0].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                this.Method = urlStrings[0];
                this.Url = urlStrings[1];
                string[] httpProtocol = urlStrings[2].Split('/');
                this.HttpVersion = httpProtocol[1];

                if (rows.Length > 1){
                    for (int index = 1; index < rows.Length; index++){
                        string[] row = rows[index].Split(':');
                        this.Fields.Add(row[0].ToLower(), row[1].Trim());
                    }
                }

                this.UserAgent = this.Field("User-Agent");

                string[] cookies = (this.Field("cookie") ?? "").Split("; ", System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string cookiePart in cookies){
                    string[] cookie = cookiePart.Split('=', 2);
                    this.Cookies.Add(cookie[0], cookie.Length == 1 ? "" : cookie[1]);
                }
            }
        }

        public static Headers Parse(byte[] headers){
            return Parse(Encoding.UTF8.GetString(headers));
        }

        public static Headers Parse(string headers){
            return new Headers(headers);
        }

        public string Field(string key){
            key = key.ToLower();

            return this.Fields.ContainsKey(key) == true ? this.Fields[key] : null;
        }

        public string Cookie(string key){
            return this.Cookies.ContainsKey(key) == true ? this.Cookies[key] : null;
        }

        public string ToString_Original(){
            return this.OriginalHeadersString;
        }

        public override string ToString(){
            return string.IsNullOrEmpty(this.OriginalHeadersString) == true
                ? ""
                : $"{this.Method} {this.Url} HTTP/{HttpVersion}\n" +
                  $"{string.Join("\n", this.Fields.Select(pair => pair.Key + ": " + pair.Value).ToArray())}";
        }
    }
}
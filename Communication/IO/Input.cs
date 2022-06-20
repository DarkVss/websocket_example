using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebSocket_example.Communication.IO{
    public class Input{
        [JsonProperty("__isValid")]
        public bool IsValid => this._isValid;
        private bool _isValid = false;
        [JsonProperty("command")]
        public string Command => this._command;
        private string _command;
        [JsonProperty("identifier",Required = Required.Default)]
        public string Identifier => this._identifier;
        private string _identifier = null;
        [JsonProperty("params",Required = Required.Default)]
        public object Params => this._params;
        private object _params = null;

        private Input(){ }

        public static Input Parse(string input){
            Input instance = new Input();

            Dictionary<string, object> obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(input);

            if (obj != null){
                if (obj.ContainsKey("command") == true){
                    instance._command = obj["command"]?.ToString();
                    if (obj.ContainsKey("identifier") == true){
                        instance._identifier = obj["identifier"]?.ToString();
                    }

                    if (obj.ContainsKey("params") == true){
                        instance._params = obj["params"]?.ToString();
                    }

                    instance._isValid = true;
                }
            }

            return instance;
        }

        public string ToJson(){
            Dictionary<string, object> output = new Dictionary<string, object>();

            output.Add("__isValid", this._isValid);
            if (this._command != null){
                output.Add("command", this._command);
            }

            if (this._identifier != null){
                output.Add("identifier", this._identifier);
            }

            if (this._params != null){
                output.Add("params", this._params);
            }

            return JsonConvert.SerializeObject(output);
        }

        public override string ToString(){
            return this.ToJson();
        }
    }
}
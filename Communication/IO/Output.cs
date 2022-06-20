using System;
using System.Collections.Generic;
using System.Text.Json;
using Newtonsoft.Json;

namespace WebSocket_example.Communication.IO{
    public class Output{
        private string _identifier;
        private bool _status = false;
        private string _message;
        private object _data;

        public Output(){ }

        private Output _return(){
            return this;
        }

        public Output Identifier(string identifier){
            this._identifier = identifier;

            return this._return();
        }

        public Output Status(bool status){
            this._status = status;

            return this._return();
        }

        public Output Message(string message){
            this._message = message;

            return this._return();
        }

        public Output Data(object data){
            this._data = data;

            return this._return();
        }

        public string ToJson(){
            Dictionary<string, object> output = new Dictionary<string, object>();

            output.Add("status", this._status);
            if (this._identifier != null){
                output.Add("identifier", this._identifier);
            }

            if (this._message != null){
                output.Add("message", this._message);
            }

            if (this._data != null){
                output.Add("data", this._data);
            }

            output.Add("timestamp", new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString());

            return JsonConvert.SerializeObject(output);
        }

        public override string ToString(){
            return this.ToJson();
        }
    }
}
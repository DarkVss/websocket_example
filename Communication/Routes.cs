using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebSocket_example.Communication.IO;

namespace WebSocket_example.Communication{
    public class Routes{
        private static Routes _instance;

        protected readonly Dictionary<string, Type> Commands;

        protected Routes(){
            this.Commands = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => String.Equals(type.Namespace, $"{typeof(Routes).Namespace}.Command", StringComparison.Ordinal))
                .ToDictionary(
                    type => type.Name.First().ToString().ToLower() + type.Name.Substring(1),
                    type => type
                );
        }

        public static void InitInstance() => _instance ??= new Routes();

        public static Routes GetInstance() => _instance ??= new Routes();

        public void Execute(string connectionIdentifier,Input input){
            if (this.Commands.ContainsKey(input.Command) == false){
                throw new Exception(Message.COMMAND_UNKNOWN);
            }

            ((Commands) this.Commands[input.Command].GetConstructors()[0].Invoke(new object[]{connectionIdentifier,input})).Execute();
        }

        public override string ToString(){
            return this.Commands.Count == 0
                ? "Commands not registered"
                : string.Join(" ; ", this.Commands.Select(pair => $"{pair.Key}:{pair.Value.Name}").ToArray());
        }
    }
}
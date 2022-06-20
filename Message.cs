namespace WebSocket_example{
    public readonly struct Message{
        public const string COMMAND_UNKNOWN = "Unknown command";

        public const string CONNECTION_CLOSE_REGISTER_SAME_UID = "Registered connection with same Identifier";

        public const string COMMUNICATION_UNKNOWN_PROTOCOL_COMMAND = "Unknown protocol commands";

        public const string COMMUNICATION_INVALID_MESSAGE = "Invalid message (broken JSON)";
    }
}
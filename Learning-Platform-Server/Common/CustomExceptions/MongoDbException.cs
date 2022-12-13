namespace Learning_Platform_Server.Helpers.CustomExceptions
{
    [Serializable]
    public class MongoDbException : Exception
    {
        public MongoDbException()
        {
        }

        public MongoDbException(string message)
            : base(message)
        {
        }

        public MongoDbException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

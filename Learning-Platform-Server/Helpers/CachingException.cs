namespace Learning_Platform_Server.Helpers
{
    [Serializable]
    public class CachingException : Exception
    {
        public CachingException()
        {
        }

        public CachingException(string message)
            : base(message)
        {
        }

        public CachingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

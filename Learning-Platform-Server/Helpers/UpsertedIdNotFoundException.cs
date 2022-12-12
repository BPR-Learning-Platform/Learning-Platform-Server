﻿namespace Learning_Platform_Server.Helpers
{
    [Serializable]
    public class UpsertedIdNotFoundException : Exception
    {
        public UpsertedIdNotFoundException()
        {
        }

        public UpsertedIdNotFoundException(string message)
            : base(message)
        {
        }

        public UpsertedIdNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

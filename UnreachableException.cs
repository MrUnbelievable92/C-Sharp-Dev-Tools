using System;

namespace DevTools
{
    public class UnreachableException : Exception
    {
        public UnreachableException()
            : base($"{ Assert.ASSERTION_FAILED_TAG } Attempted to execute unreachable code.")
        {

        }
    }
}

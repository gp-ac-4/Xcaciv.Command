namespace Xcaciv.Command.Interface.Exceptions
{
    public class NoPluginFilesFoundException : Exception
    {
        public NoPluginFilesFoundException(string message) : base(message)
        {
        }

        public NoPluginFilesFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
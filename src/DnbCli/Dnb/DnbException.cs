namespace DnbCli.Dnb;

public class DnbException : Exception
{
    public DnbException(string message) : base(message) { }
    public DnbException(string message, Exception inner) : base(message, inner) { }
}

public sealed class DnbNetworkException : DnbException
{
    public DnbNetworkException(string message, Exception? inner = null)
        : base(message, inner ?? new Exception(message)) { }
}

public sealed class DnbUpstreamException : DnbException
{
    public DnbUpstreamException(string message) : base(message) { }
}

public sealed class DnbNotFoundException : DnbException
{
    public DnbNotFoundException(string message = "No records found") : base(message) { }
}

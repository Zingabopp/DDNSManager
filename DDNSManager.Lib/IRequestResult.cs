namespace DDNSManager.Lib
{
    public interface IRequestResult
    {
        ResultStatus Status { get; }
        string Message { get; }
        string RawMessage { get; }
    }

    public enum ResultStatus
    {
        None,
        Completed,
        Skipped,
        Faulted
    }
}

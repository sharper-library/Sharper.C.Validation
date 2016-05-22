namespace Sharper.C.Data.Validation
{
    public interface ValidnFail
    {
        string Message { get; }
    }

    public interface ValidnFail<A>
      : ValidnFail
    {
        A Value { get; }
    }
}

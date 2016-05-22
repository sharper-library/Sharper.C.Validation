namespace Sharper.C.Data.Validation
{
    public interface Invalid
    {
        string Message { get; }
    }

    public interface Invalid<A>
      : Invalid
    {
        A Value { get; }
    }
}

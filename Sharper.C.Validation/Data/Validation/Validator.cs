using Sharper.C.Control;

namespace Sharper.C.Data.Validation
{
    public interface Validator<A>
    {
        Validn<A> Validate(A a);
    }

}

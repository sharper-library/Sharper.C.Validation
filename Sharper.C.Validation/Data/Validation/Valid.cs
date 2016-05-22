using System;
using Sharper.C.Control;

namespace Sharper.C.Data.Validation
{
    public sealed class Valid<V, A>
      where V : Validator<A>
    {
        public A Value { get; }

        private Valid(A a)
        {   Value = a;
        }

        internal static Validn<Valid<V, A>> Validate(A a, V v)
        =>  v.Validate(a).Map(a0 => new Valid<V, A>(a0));
    }

    public static class Valid
    {
        public static Func<A, Validn<Valid<V, A>>> Dynamic<V, A>(V v)
          where V : class, Validator<A>
        =>  a => Valid<V, A>.Validate(a, v);

        public static Func<A, Validn<Valid<V, A>>> Static<V, A>
          ( V _ = default(V)
          )
          where V : struct, Validator<A>
        =>  a => Valid<V, A>.Validate(a, default(V));
    }
}

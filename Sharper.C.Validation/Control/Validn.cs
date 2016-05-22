using System;
using System.Collections.Immutable;
using System.Linq;
using Sharper.C.Data;
using Sharper.C.Data.Validation;

namespace Sharper.C.Control
{
    public abstract class Validn<A>
    {
        private Validn()
        {
        }

        public static Validn<A> Success(A a)
        =>  new SuccessCase(a);

        public static Validn<A> Failure(ImmutableList<Invalid> errs)
        =>  new FailureCase(errs);

        public X Cata<X>
          ( Func<ImmutableList<Invalid>, X> failure
          , Func<A, X> success
          )
        =>  this is FailureCase
            ? failure(((FailureCase)this).Errors)
            : success(((SuccessCase)this).Value);

        public Validn<B> Map<B>(Func<A, B> f)
        =>  Cata(Validn<B>.Failure, a => Validn<B>.Success(f(a)));

        public Validn<C> ZipWith<B, C>(Validn<B> vb, Func<A, B, C> f)
        =>  Cata
              ( ea =>
                    vb.Cata
                      ( eb => Validn<C>.Failure(ea.AddRange(eb))
                      , _ => Validn<C>.Failure(ea)
                      )
              , a =>
                    vb.Cata
                      ( Validn<C>.Failure
                      , b => Validn<C>.Success(f(a, b))
                      )
              );

        public A ValueOrThrow
        =>  Cata
              ( errs =>
                {   throw new AggregateException
                      ( errs.Select(ValidnException.Mk)
                      );
                }
              , a => a
              );

        public Or<ImmutableList<Invalid>, A> ToOr
        =>  Cata
              ( Or.Left<ImmutableList<Invalid>, A>
              , Or.Right<ImmutableList<Invalid>, A>
              );

        private sealed class SuccessCase
          : Validn<A>
        {
            public A Value { get; }

            public SuccessCase(A a)
            {   Value = a;
            }
        }

        private sealed class FailureCase
          : Validn<A>
        {
            public ImmutableList<Invalid> Errors { get; }

            public FailureCase(ImmutableList<Invalid> errors)
            {   Errors = errors;
            }
        }
    }

    public static class Validn
    {
        public static Validn<A> Success<A>(A a)
        =>  Validn<A>.Success(a);

        public static Validn<A> Failure<A>(ImmutableList<Invalid> errs)
        =>  Validn<A>.Failure(errs);

        public static Validn<A> Pure<A>(A a)
        =>  Success(a);

        public static Validn<A> Fail<A>(Invalid ex)
        =>  Validn<A>.Failure(ImmutableList.Create(ex));

        public static Validn<B> Ap<A, B>
          ( this Validn<Func<A, B>> vf
          , Validn<A> va
          )
        =>  vf.ZipWith(va, (f, a) => f(a));

        public static Validn<C> Join<A, B, _K, C>
          ( this Validn<A> va
          , Validn<B> vb
          , Func<A, _K> _
          , Func<B, _K> __
          , Func<A, B, C> f
          )
        =>  va.ZipWith(vb, f);

        public static Validn<A> ToValidn<A>(Or<ImmutableList<Invalid>, A> or)
        =>  or.Cata(Failure<A>, Success);

        public static Validn<A> ToValidn<A>(Or<Invalid, A> or)
        =>  or.Cata(Fail<A>, Success);

        public static Invalid Error(string message)
        =>  new AInvalid(message);

        public static Invalid<A> Error<A>(string message, A invalidValue)
        =>  new AInvalid<A>($"{message} {invalidValue}", invalidValue);

        private struct AInvalid
          : Invalid
        {
            public string Message { get; }

            public AInvalid(string msg)
            {   Message = msg;
            }
        }

        private struct AInvalid<A>
          : Invalid<A>
        {
            public string Message { get; }
            public A Value { get; }

            public AInvalid(string msg, A a)
            {   Message = msg;
                Value = a;
            }
        }
    }
}

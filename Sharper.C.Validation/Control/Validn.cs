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

        public static Validn<A> Failure(ImmutableList<ValidnFail> errs)
        =>  new FailureCase(errs);

        public X Cata<X>
          ( Func<ImmutableList<ValidnFail>, X> failure
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

        public Or<ImmutableList<ValidnFail>, A> ToOr
        =>  Cata
              ( Or.Left<ImmutableList<ValidnFail>, A>
              , Or.Right<ImmutableList<ValidnFail>, A>
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
            public ImmutableList<ValidnFail> Errors { get; }

            public FailureCase(ImmutableList<ValidnFail> errors)
            {   Errors = errors;
            }
        }
    }

    public static class Validn
    {
        public static Validn<A> Success<A>(A a)
        =>  Validn<A>.Success(a);

        public static Validn<A> Failure<A>(ImmutableList<ValidnFail> errs)
        =>  Validn<A>.Failure(errs);

        public static Validn<A> Pure<A>(A a)
        =>  Success(a);

        public static Validn<A> Fail<A>(ValidnFail ex)
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

        public static Validn<A> ToValidn<A>
          ( Or<ImmutableList<ValidnFail>, A> or
          )
        =>  or.Cata(Failure<A>, Success);

        public static Validn<A> ToValidn<A>(Or<ValidnFail, A> or)
        =>  or.Cata(Fail<A>, Success);

        public static ValidnFail Error(string message)
        =>  new AValidnFail(message);

        public static ValidnFail<A> Error<A>(string message, A invalidValue)
        =>  new AValidnFail<A>($"{message} {invalidValue}", invalidValue);

        private struct AValidnFail
          : ValidnFail
        {
            public string Message { get; }

            public AValidnFail(string msg)
            {   Message = msg;
            }
        }

        private struct AValidnFail<A>
          : ValidnFail<A>
        {
            public string Message { get; }
            public A Value { get; }

            public AValidnFail(string msg, A a)
            {   Message = msg;
                Value = a;
            }
        }
    }
}

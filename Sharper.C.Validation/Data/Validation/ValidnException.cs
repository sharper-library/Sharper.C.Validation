using System;

namespace Sharper.C.Data.Validation
{
    public sealed class ValidnException
      : Exception
    {
        public ValidnException(ValidnFail invalid)
          : base(invalid.Message)
        {
        }

        public static ValidnException Mk(ValidnFail invalid)
        =>  new ValidnException(invalid);
    }
}

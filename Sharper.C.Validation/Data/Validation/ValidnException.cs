using System;

namespace Sharper.C.Data.Validation
{
    public sealed class ValidnException
      : Exception
    {
        public ValidnException(Invalid invalid)
          : base(invalid.Message)
        {
        }

        public static ValidnException Mk(Invalid invalid)
        =>  new ValidnException(invalid);
    }
}

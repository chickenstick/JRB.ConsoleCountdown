using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRB.ConsoleCountdown
{
    public sealed class KeyPressedEventArgs : EventArgs
    {

        #region - Constructor -

        public KeyPressedEventArgs(ConsoleKeyInfo keyInfo)
            : base()
        {
            this.KeyInfo = keyInfo;
            this.StopTimer = false;
            this.Message = string.Empty;
        }

        #endregion

        #region - Properties -

        public ConsoleKeyInfo KeyInfo { get; private set; }
        public bool StopTimer { get; set; }
        public string Message { get; set; }

        #endregion

    }
}

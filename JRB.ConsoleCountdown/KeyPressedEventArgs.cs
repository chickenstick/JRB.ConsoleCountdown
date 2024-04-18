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
            this.TimerAction = TimerAction.None;
            this.Message = string.Empty;
        }

        #endregion

        #region - Properties -

        public ConsoleKeyInfo KeyInfo { get; private set; }
        public TimerAction TimerAction { get; set; }
        public string Message { get; set; }

        #endregion

    }
}

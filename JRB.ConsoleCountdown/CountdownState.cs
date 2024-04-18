using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRB.ConsoleCountdown
{
    public enum CountdownState
    {
        NotStarted,
        Running,
        UserStopped,
        Expired,
        Paused
    }
}

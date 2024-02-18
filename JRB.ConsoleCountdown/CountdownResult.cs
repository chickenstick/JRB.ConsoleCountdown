using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRB.ConsoleCountdown
{
    public class CountdownResult
    {

        public CountdownResult()
        {
            PromptResult = PromptResult.Unknown;
            KeyPressed = null;
            Message = string.Empty;
        }

        public PromptResult PromptResult { get; set; }
        public ConsoleKeyInfo? KeyPressed { get; set; }
        public string Message { get; set; }

    }
}

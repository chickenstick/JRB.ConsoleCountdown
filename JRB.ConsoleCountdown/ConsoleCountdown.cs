using JRB.RewriteableConsoleContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRB.ConsoleCountdown
{
    public sealed class ConsoleCountdown
    {

        #region - Constants -

        private const int DEFAULT_DELAY_MILLISECONDS = 50;
        private const int DEFAULT_FIELD_FIXED_LENGTH = 50;
        private const string DEFAULT_TIMES_UP_MESSAGE = "Time's up.";

        #endregion

        #region - Events -

        public event EventHandler<KeyPressedEventArgs>? KeyPressed;

        #endregion

        #region - Fields -

        private TimeSpan _interval;

        #endregion

        #region - Constructors -

        public ConsoleCountdown(int delayMilliseconds, int fixedFieldLength, string timesUpMessage)
        {
            this.DelayMilliseconds = delayMilliseconds;
            this.FixedFieldLength = fixedFieldLength;
            this.TimesUpMessage = timesUpMessage;

            _interval = TimeSpan.FromMilliseconds(delayMilliseconds);
        }

        public ConsoleCountdown()
            : this(DEFAULT_DELAY_MILLISECONDS, DEFAULT_FIELD_FIXED_LENGTH, DEFAULT_TIMES_UP_MESSAGE)
        {
        }

        #endregion

        #region - Properties -

        public int DelayMilliseconds { get; private set; }
        public int FixedFieldLength { get; private set; }
        public string TimesUpMessage { get; private set; }

        #endregion

        #region - Public Methods -

        public CountdownResult Prompt(int startingSeconds)
        {
            //Slug slug = Slug.CreateHere(FixedFieldLength);
            NestedTimerSlug slug = NestedTimerSlug.CreateHere(TimesUpMessage, FixedFieldLength);
            Console.WriteLine();
            TimeSpan timeRemaining = TimeSpan.FromSeconds(startingSeconds);
            slug.UpdateDisplay(timeRemaining);

            while (timeRemaining > TimeSpan.Zero)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    KeyPressedEventArgs eventArgs = new KeyPressedEventArgs(keyInfo);
                    this.KeyPressed?.Invoke(this, eventArgs);

                    if (eventArgs.StopTimer)
                    {
                        return new CountdownResult()
                        {
                            PromptResult = PromptResult.UserStopped,
                            KeyPressed = keyInfo,
                            Message = eventArgs.Message
                        };
                    }
                }
                Thread.Sleep(DelayMilliseconds);
                timeRemaining = timeRemaining.Subtract(_interval);
                slug.UpdateDisplay(timeRemaining);
            }

            return new CountdownResult()
            {
                PromptResult = PromptResult.TimerExpired
            };
        }

        #endregion

        #region - Nested Classes -

        private class NestedTimerSlug
        {

            private Slug _slug;
            private string _timesUpMessage;
            private int? _currentDisplayedSeconds;

            private NestedTimerSlug(Slug slug, string timesUpMessage)
            {
                _slug = slug;
                _timesUpMessage = timesUpMessage;
                _currentDisplayedSeconds = null;
            }

            public static NestedTimerSlug CreateHere(string timesUpMessage, int fixedLength)
            {
                Slug slug = Slug.CreateHere(fixedLength);
                return new NestedTimerSlug(slug, timesUpMessage);
            }

            public void UpdateDisplay(TimeSpan remaining)
            {
                int secondsRemaining = GetSecondsRemaining(remaining);
                if (!_currentDisplayedSeconds.HasValue || secondsRemaining != _currentDisplayedSeconds.Value)
                {
                    _currentDisplayedSeconds = secondsRemaining;
                    string s = (secondsRemaining > 0) ? secondsRemaining.ToString() : _timesUpMessage;
                    _slug.Write(s, Alignment.Left, true);
                }
            }

            private int GetSecondsRemaining(TimeSpan remaining) => (int)Math.Ceiling(remaining.TotalSeconds);

        }

        #endregion

    }
}

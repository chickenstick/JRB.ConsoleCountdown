using JRB.RewriteableConsoleContent;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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
            this.CountdownState = CountdownState.NotStarted;

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
        public CountdownState CountdownState { get; private set; }

        #endregion

        #region - Methods -

        public CountdownResult? Prompt(int startingSeconds)
        {
            NestedTimerSlug slug = NestedTimerSlug.CreateHere(TimesUpMessage, FixedFieldLength);
            TimeSpan timeRemaining = TimeSpan.FromSeconds(startingSeconds);
            slug.UpdateDisplay(timeRemaining);
            CountdownState = CountdownState.Running;

            TimerCheckEvent timerCheckEvent = new TimerCheckEvent(timeRemaining, slug);
            using (Timer timer = new Timer(CheckTimer, timerCheckEvent, DelayMilliseconds, DelayMilliseconds))
            {
                timerCheckEvent.WaitOne();
            }

            return timerCheckEvent.Result;
        }

        private void CheckTimer(object? stateInfo)
        {
            TimerCheckEvent? timerCheckEvent = stateInfo as TimerCheckEvent;
            if (timerCheckEvent == null)
            {
                throw new InvalidOperationException("The timer check event should never be null.");
            }

            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                KeyPressedEventArgs eventArgs = new KeyPressedEventArgs(keyInfo);
                this.KeyPressed?.Invoke(this, eventArgs);

                switch (eventArgs.TimerAction)
                {
                    case TimerAction.Stop:
                        StopTimer(timerCheckEvent, keyInfo, eventArgs);
                        return;
                    case TimerAction.Pause:
                        PauseTimer(timerCheckEvent, keyInfo, eventArgs);
                        return;
                    case TimerAction.Restart:
                        RestartTimer(timerCheckEvent, keyInfo, eventArgs);
                        return;
                    case TimerAction.None:
                    default:
                        break;
                }
            }

            if (CountdownState == CountdownState.Paused)
                return;

            timerCheckEvent.SubtractMilliseconds(DelayMilliseconds);
            timerCheckEvent.UpdateDisplay();
            if (timerCheckEvent.TimerExpired())
            {
                timerCheckEvent.Result = new CountdownResult()
                {
                    PromptResult = PromptResult.TimerExpired
                };
                timerCheckEvent.EndWait();
                CountdownState = CountdownState.Expired;
                return;
            }
        }

        void StopTimer(TimerCheckEvent timerCheckEvent, ConsoleKeyInfo keyInfo, KeyPressedEventArgs eventArgs)
        {
            timerCheckEvent.Result = new CountdownResult()
            {
                PromptResult = PromptResult.UserStopped,
                KeyPressed = keyInfo,
                Message = eventArgs.Message
            };
            CountdownState = CountdownState.UserStopped;

            if (!string.IsNullOrWhiteSpace(eventArgs.Message))
            {
                timerCheckEvent.OverrideText(eventArgs.Message);
            }

            // This stops the hold on the thread running the timer, and allows execution to continue.
            timerCheckEvent.EndWait();
        }

        void PauseTimer(TimerCheckEvent timerCheckEvent, ConsoleKeyInfo keyInfo, KeyPressedEventArgs eventArgs)
        {
            timerCheckEvent.Result = new CountdownResult()
            {
                PromptResult = PromptResult.UserPaused,
                KeyPressed = keyInfo,
                Message = eventArgs.Message
            };
            CountdownState = CountdownState.Paused;

            if (!string.IsNullOrWhiteSpace(eventArgs.Message))
            {
                timerCheckEvent.OverrideText(eventArgs.Message);
            }
        }

        void RestartTimer(TimerCheckEvent timerCheckEvent, ConsoleKeyInfo keyInfo, KeyPressedEventArgs eventArgs)
        {
            timerCheckEvent.Result = new CountdownResult()
            {
                PromptResult = PromptResult.UserRestarted,
                KeyPressed = keyInfo,
                Message = eventArgs.Message
            };
            CountdownState = CountdownState.Running;
            timerCheckEvent.UpdateDisplay(true);
        }

        #endregion

        #region - Nested Classes -

        private sealed class TimerCheckEvent
        {

            private EventWaitHandle _eventWaitHandle;
            private NestedTimerSlug _slug;

            private object _timeRemainingLock = new object();
            private object _slugLock = new object();

            public TimerCheckEvent(TimeSpan timeRemaining, NestedTimerSlug slug)
            {
                _eventWaitHandle = new AutoResetEvent(false);
                this.TimeRemaining = timeRemaining;
                _slug = slug;
                this.Result = null;
            }

            public TimeSpan TimeRemaining { get; private set; }
            public CountdownResult? Result { get; set; }

            public bool TimerExpired()
            {
                lock (_timeRemainingLock)
                {
                    return TimeRemaining <= TimeSpan.Zero;
                }
            }

            public bool Reset() => _eventWaitHandle.Reset();
            public bool EndWait() => _eventWaitHandle.Set();
            public bool WaitOne() => _eventWaitHandle.WaitOne();

            public void SubtractTime(TimeSpan elapsed)
            {
                lock (_timeRemainingLock)
                {
                    if (this.TimeRemaining <= TimeSpan.Zero)
                    {
                        return;
                    }

                    this.TimeRemaining -= elapsed;

                    if (this.TimeRemaining < TimeSpan.Zero)
                    {
                        this.TimeRemaining = TimeSpan.Zero;
                    }
                }
            }

            public void SubtractMilliseconds(int milliseconds)
            {
                SubtractTime(TimeSpan.FromMilliseconds(milliseconds));
            }

            public void UpdateDisplay() => UpdateDisplay(false);

            public void UpdateDisplay(bool forceUpdate)
            {
                lock (_slugLock)
                {
                    _slug.UpdateDisplay(TimeRemaining, forceUpdate);
                }
            }

            public void OverrideText(string message)
            {
                lock (_slugLock)
                {
                    _slug.OverrideText(message);
                }
            }

        }

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

            public void UpdateDisplay(TimeSpan remaining) => UpdateDisplay(remaining, false);

            public void UpdateDisplay(TimeSpan remaining, bool forceUpdate)
            {
                int secondsRemaining = GetSecondsRemaining(remaining);
                if (forceUpdate || !_currentDisplayedSeconds.HasValue || secondsRemaining != _currentDisplayedSeconds.Value)
                {
                    _currentDisplayedSeconds = secondsRemaining;
                    string s = (secondsRemaining > 0) ? secondsRemaining.ToString() : _timesUpMessage;
                    _slug.Write(s, Alignment.Left, true);
                }
            }

            public void OverrideText(string message) => _slug.Write(message, Alignment.Left, true);

            private int GetSecondsRemaining(TimeSpan remaining) => (int)Math.Ceiling(remaining.TotalSeconds);

        }

        #endregion

    }
}

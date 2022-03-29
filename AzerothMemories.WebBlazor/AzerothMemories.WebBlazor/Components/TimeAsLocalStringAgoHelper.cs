namespace AzerothMemories.WebBlazor.Components
{
    public sealed class TimeAsLocalStringAgoHelper : IDisposable
    {
        private readonly Action _tickCallbackAction;
        
        private long _time;
        private Timer _timer;
        private Duration _timerTick;

        public TimeAsLocalStringAgoHelper( Action tickCallbackAction)
        {
            _tickCallbackAction = tickCallbackAction;
        }

        public void TrySetTimer(long time)
        {
            _time = time;

            TrySetTimer();
        }

        private void TrySetTimer()
        {
            if (_time <= 0)
            {
                return;
            }

            var diffMs = SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds() - _time;
            diffMs = Math.Abs(diffMs);

            var diff = Duration.FromMilliseconds(diffMs);
            var timerTick = Duration.Zero;
            if (diff.TotalSeconds < 60)
            {
                timerTick = Duration.FromSeconds(1);
            }
            else if (diff.TotalMinutes < 60)
            {
                timerTick = Duration.FromMinutes(1);
            }
            else if (diff.TotalHours < 24)
            {
                timerTick = Duration.FromHours(1);
            }

            if (timerTick == Duration.Zero)
            {
                return;
            }

            if (_timerTick == timerTick)
            {
                return;
            }

            _timerTick = timerTick;

            if (_timer == null)
            {
                _timer = new Timer(OnTimerTick, null, 0, (long)_timerTick.TotalMilliseconds);
            }
            else
            {
                _timer.Change(0, (long)_timerTick.TotalMilliseconds);
            }
        }

        private void OnTimerTick(object state)
        {
            TrySetTimer();

            _tickCallbackAction?.Invoke();
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}
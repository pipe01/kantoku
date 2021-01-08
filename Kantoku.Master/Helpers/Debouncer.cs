using System;
using System.Threading;

namespace Kantoku.Master.Helpers
{
    // https://github.com/progietheus/debouncer/blob/master/debouncer.cs
    public class Debouncer
    {
        public event EventHandler Idled = delegate { };
        public int WaitingMilliSeconds { get; set; }

        private readonly Timer WaitingTimer;

        public Debouncer(int waitingMilliSeconds = 600)
        {
            WaitingMilliSeconds = waitingMilliSeconds;
            WaitingTimer = new Timer(p => Idled(this, EventArgs.Empty));
        }

        public void Trigger()
        {
            WaitingTimer.Change(WaitingMilliSeconds, Timeout.Infinite);
        }
    }
}

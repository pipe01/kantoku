using PropertyChanged;
using System.ComponentModel;
using System.Threading;

namespace Kantoku.Master.Frontend
{
    public interface IConnectionCounter : INotifyPropertyChanged
    {
        int Count { get; }
        void Increment();
        void Decrement();
    }

    public class ConnectionCounter : IConnectionCounter
    {
        private static readonly PropertyChangedEventArgs EventArgs = new PropertyChangedEventArgs(nameof(Count));

        public event PropertyChangedEventHandler? PropertyChanged;

        [DoNotNotify]
        public int Count { get; private set; }

        private readonly SynchronizationContext SynchronizationContext;

        public ConnectionCounter(SynchronizationContext synchronizationContext)
        {
            this.SynchronizationContext = synchronizationContext;
        }

        private void OnChanged() => SynchronizationContext.Post(() => PropertyChanged?.Invoke(this, EventArgs));

        public void Decrement()
        {
            Count--;
            OnChanged();
        }

        public void Increment()
        {
            Count++;
            OnChanged();
        }
    }
}

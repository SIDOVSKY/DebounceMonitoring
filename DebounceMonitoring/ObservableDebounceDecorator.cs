using System;

namespace DebounceMonitoring
{
    internal class ObservableDebounceDecorator<T> : IObservable<T>, IObserver<T>, IDisposable
    {
        private readonly TimeSpan _interval;
        private readonly IObservable<T> _source;

        private DateTime _lastStamp;
        private IObserver<T>? _observer;
        private IDisposable? _sourceDisposable;

        public ObservableDebounceDecorator(IObservable<T> source, TimeSpan interval)
        {
            _source = source;
            _interval = interval;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observer = observer;
            _sourceDisposable = _source.Subscribe(this);
            return this;
        }

        public void OnCompleted() => _observer?.OnCompleted();

        public void OnError(Exception error) => _observer?.OnError(error);

        public void OnNext(T value)
        {
            if (!DebounceMonitor.Disabled)
            {
                var now = DateTime.UtcNow;

                if (now < _lastStamp + _interval)
                    return;

                _lastStamp = now;
            }

            _observer?.OnNext(value);
        }
        public void Dispose()
        {
            _sourceDisposable?.Dispose();
            _sourceDisposable = null;
            _observer = null;
        }
    }
}
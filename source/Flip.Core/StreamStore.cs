namespace Flip
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class StreamStore<TIdentifier, TModel> :
        IStreamStore<TIdentifier, TModel>
        where TIdentifier : IEquatable<TIdentifier>
        where TModel : class, IModel<TIdentifier>
    {
        private readonly IStreamFilter<TModel> _filter;
        private readonly Dictionary<TIdentifier, Stream> _streams;

        public StreamStore(IStreamFilter<TModel> filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _filter = filter;
            _streams = new Dictionary<TIdentifier, Stream>();
        }

        public StreamStore()
            : this(new CompositeStreamFilter<TModel>())
        {
        }

        public IConnection<TIdentifier, TModel> Connect(TIdentifier modelId)
        {
            if (modelId == null)
            {
                throw new ArgumentNullException(nameof(modelId));
            }

            Stream stream;
            if (_streams.TryGetValue(modelId, out stream) == false)
            {
                stream = new Stream(this, modelId);
                _streams.Add(modelId, stream);
            }

            return new Connection(modelId, stream);
        }

        private sealed class Stream : ISubject<IObservable<TModel>, TModel>
        {
            private readonly StreamStore<TIdentifier, TModel> _store;
            private readonly TIdentifier _modelId;
            private readonly Subject<IObservable<TModel>> _observer;
            private readonly BehaviorSubject<TModel> _observable;

            public Stream(
                StreamStore<TIdentifier, TModel> store,
                TIdentifier modelId)
            {
                _store = store;
                _modelId = modelId;
                _observer = new Subject<IObservable<TModel>>();
                _observable = new BehaviorSubject<TModel>(default(TModel));

                _observer.Switch()
                         .Where(value => value != null)
                         .Select(Filter)
                         .Subscribe(_observable);
            }

            private TModel Filter(TModel newValue)
            {
                return _store._filter.Execute(newValue, _observable.Value);
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(IObservable<TModel> value)
            {
                _observer.OnNext(value);
            }

            public IDisposable Subscribe(IObserver<TModel> observer)
            {
                return _observable.Subscribe(observer);
            }
        }

        private sealed class Connection : IConnection<TIdentifier, TModel>
        {
            private readonly TIdentifier _modelId;
            private readonly ISubject<IObservable<TModel>, TModel> _stream;
            private readonly Subject<TModel> _observer;
            private readonly WeakSubscription<TModel> _subscription;

            public Connection(
                TIdentifier modelId,
                ISubject<IObservable<TModel>, TModel> stream)
            {
                _modelId = modelId;
                _stream = stream;
                _observer = new Subject<TModel>();
                _subscription = WeakSubscription.Create(_stream, _observer);
            }

            ~Connection()
            {
                Dispose(false);
            }

            public TIdentifier ModelId => _modelId;

            public IObservable<TModel> Stream => _observer;

            public void Emit(IObservable<TModel> source)
                => _stream.OnNext(source);

            public void Dispose() => Dispose(true);

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _subscription.Dispose();
                }
            }
        }
    }
}

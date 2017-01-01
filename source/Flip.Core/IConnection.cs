namespace Flip
{
    using System;

    public interface IConnection<TModel> : IDisposable
        where TModel : class
    {
        IObservable<TModel> Stream { get; }

        void Emit(IObservable<TModel> source);
    }

    public interface IConnection<TIdentifier, TModel> : IConnection<TModel>
        where TIdentifier : IEquatable<TIdentifier>
        where TModel : class
    {
        TIdentifier ModelId { get; }
    }
}

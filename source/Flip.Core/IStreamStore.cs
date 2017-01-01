namespace Flip
{
    using System;

    public interface IStreamStore<TIdentifier, TModel>
        where TIdentifier : IEquatable<TIdentifier>
        where TModel : class
    {
        IConnection<TIdentifier, TModel> Connect(TIdentifier modelId);
    }
}

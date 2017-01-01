namespace Flip
{
    using System;

    public interface IModel<TIdentifier>
        where TIdentifier : IEquatable<TIdentifier>
    {
        TIdentifier Id { get; }
    }
}

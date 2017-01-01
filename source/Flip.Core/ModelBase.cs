namespace Flip
{
    using System;

    public class ModelBase<TIdentifier> : IModel<TIdentifier>
        where TIdentifier : IEquatable<TIdentifier>
    {
        protected ModelBase(TIdentifier id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            Id = id;
        }

        public TIdentifier Id { get; }
    }
}

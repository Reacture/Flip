namespace Flip
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class CompositeStreamFilter<TModel> : IStreamFilter<TModel>
        where TModel : class
    {
        private readonly ReadOnlyCollection<IStreamFilter<TModel>> _filters;

        public CompositeStreamFilter(params IStreamFilter<TModel>[] filters)
            : this(filters?.AsEnumerable())
        {
        }

        public CompositeStreamFilter(IEnumerable<IStreamFilter<TModel>> filters)
        {
            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            List<IStreamFilter<TModel>> filterList = filters.ToList();
            for (int i = 0; i < filterList.Count; i++)
            {
                if (filterList[i] == null)
                {
                    throw new ArgumentException(
                        $"{nameof(filters)} cannot contain null.",
                        nameof(filters));
                }
            }

            _filters = new ReadOnlyCollection<IStreamFilter<TModel>>(filterList);
        }

        public IEnumerable<IStreamFilter<TModel>> Filters => _filters;

        public TModel Execute(TModel newValue, TModel lastValue)
        {
            if (newValue == null)
            {
                throw new ArgumentNullException(nameof(newValue));
            }

            if (lastValue == null)
            {
                throw new ArgumentNullException(nameof(lastValue));
            }

            TModel value = newValue;

            for (int i = 0; i < _filters.Count; i++)
            {
                IStreamFilter<TModel> filter = _filters[i];
                value = filter.Execute(value, lastValue);
                if (value == null)
                {
                    break;
                }
            }

            return value;
        }
    }
}

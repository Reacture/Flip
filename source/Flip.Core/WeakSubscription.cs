namespace Flip
{
    using System;

    internal static class WeakSubscription
    {
        public static WeakSubscription<T> Create<T>(
            IObservable<T> observable, IObserver<T> observer)
        {
            if (observable == null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            return new WeakSubscription<T>(observable, observer);
        }
    }
}

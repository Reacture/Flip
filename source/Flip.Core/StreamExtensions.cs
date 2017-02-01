namespace Flip
{
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;

    public static class StreamExtensions
    {
        public static void Emit<TModel>(
            this IConnection<TModel> connection, TModel model)
            where TModel : class
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            connection.Emit(Observable.Return(model));
        }

        public static void Emit<TModel>(
            this IConnection<TModel> connection, Task<TModel> task)
            where TModel : class
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            connection.Emit(task.ToObservable());
        }
    }
}

namespace Cornerstone.Database
{
    namespace Data
    {
        internal sealed class Scope<T> : IDisposable where T : class
        {
            private bool _disposed;
            private readonly bool _ownsInstance;
            private readonly T _instance;

            private readonly Scope<T> _parent;

            [ThreadStatic()]
            private static Scope<T> _head;

            public Scope(T instance) : this(instance, true)
            {
            }

            public Scope(T instance, bool ownsInstance)
            {
                _instance = instance;
                _ownsInstance = ownsInstance;

                Thread.BeginThreadAffinity();

                _parent = _head;
                _head = this;
            }

            public static T Current
            {
                get
                {
                    if (_head != null)
                    {
                        return _head._instance;
                    }
                    return default(T);
                }
            }

            public static Scope<T> CurrentScope
            {
                get
                {
                    return _head;
                }
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    this.OnDisposing(EventArgs.Empty);
                    _disposed = true;

                    _head = _parent;

                    Thread.EndThreadAffinity();

                    if (_ownsInstance)
                    {
                        IDisposable disposable = _instance as IDisposable;
                        disposable?.Dispose();
                    }
                }
            }

            private void OnDisposing(System.EventArgs e)
            {
                if (Disposing != null)
                {
                    Disposing(this, e);
                }
            }

            public event EventHandler Disposing;

        }
    }

}

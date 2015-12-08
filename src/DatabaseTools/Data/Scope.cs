
using DatabaseTools.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

using System.Threading;
using System.ComponentModel;

namespace DatabaseTools
{
	namespace Data
	{
		internal sealed class Scope<T> : IDisposable where T: class
		{
			private bool _disposed;
			private bool _ownsInstance;
			private T _instance;

			private Scope<T> _parent;

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
						if (disposable != null)
						{
							disposable.Dispose();
						}
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
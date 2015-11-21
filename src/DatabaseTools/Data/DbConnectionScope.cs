
using DatabaseTools.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseTools
{
	namespace Data
	{
		public class DbConnectionScope : System.IDisposable
		{
			private Scope<System.Data.Common.DbConnection> _scope;
			private System.Data.Common.DbConnection _connection;

			public DbConnectionScope(System.Data.Common.DbConnection connection)
			{
				this._connection = connection;
				this._scope = new Scope<System.Data.Common.DbConnection>(_connection);
			}

#region  IDisposable Support 

			private bool disposedValue = false; // To detect redundant calls
			// This code added by Visual Basic to correctly implement the disposable pattern.
			public void Dispose()
			{
				// Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			// IDisposable
			protected virtual void Dispose(bool disposing)
			{
				if (!this.disposedValue)
				{
					if (disposing)
					{
						if (this._connection != null)
						{
							if (this._connection.State != ConnectionState.Closed)
							{
								this._connection.Close();
							}
						}
						this._connection.Dispose();
						this._scope.Dispose();
					}
				}
				this.disposedValue = true;
			}
#endregion





		}
	}


}
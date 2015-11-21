
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
		public class DbTransactionScope : System.IDisposable
		{
			private Scope<System.Data.Common.DbTransaction> _scope;
			private bool _committed = false;

			public DbTransactionScope(System.Data.Common.DbTransaction transaction)
			{
				this._scope = new Scope<System.Data.Common.DbTransaction>(transaction);
				_scope.Disposing += this.Scope_Disposing;
			}

			public void Commit()
			{
				if (!(Scope<System.Data.Common.DbTransaction>.Current == null))
				{
					Scope<System.Data.Common.DbTransaction>.Current.Commit();
				}
				_committed = true;
			}

			public void Complete()
			{
				//This is used to be compatible with System.Transactions.TransactionScope 
				this.Commit();
			}

			protected void Scope_Disposing(object sender, System.EventArgs e)
			{
				if (!_committed && !(Scope<System.Data.Common.DbTransaction>.Current == null) && !(Scope<System.Data.Common.DbTransaction>.Current.Connection == null))
				{
					//If commit or rollback have not happened then rollback the transaction
					if (!(Scope<System.Data.Common.DbTransaction>.Current == null))
					{
						Scope<System.Data.Common.DbTransaction>.Current.Rollback();
					}
				}
			}

			public static System.Data.Common.DbTransaction Current
			{
				get
				{
					return Data.Scope<System.Data.Common.DbTransaction>.Current;
				}
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
						this._scope.Dispose();
					}
				}
				this.disposedValue = true;
			}
#endregion


		}
	}


}
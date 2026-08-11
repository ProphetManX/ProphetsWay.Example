using System;

namespace ProphetsWay.Example.Tests.ConventionShowcase
{
	/// <summary>
	/// The four members every class deriving from <c>BaseDataAccess</c> owes, supplied once so that each
	/// showcase below contains nothing but the one mistake it exists to demonstrate.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>None of the Data Access Layers in this folder is a working implementation, and none of them belongs
	/// in a shipped project.</b> They are deliberately mis-wired, one mistake apiece, so a reader can see what
	/// <c>DataAccessConventionException</c> is for and how legible its diagnostics are.
	/// <see cref="DataAccess.NoDB.ExampleDataAccess"/> is the implementation to copy; nothing here is.
	/// </para>
	/// <para>
	/// <c>BaseDataAccess</c> declares <c>Dispose</c> and the three transaction members abstract, so without this
	/// class every showcase would open with four irrelevant overrides before reaching its point. They are
	/// boilerplate here in the strict sense - none of these Data Access Layers holds a resource, opens a
	/// transaction, or reads or writes the <c>DataStore</c>. Because they touch no shared state, the test
	/// classes in this folder need no <c>[Collection]</c> and run in parallel with everything else in the
	/// suite.
	/// </para>
	/// </remarks>
	public abstract class ShowcaseDataAccess : BaseDataAccess.BaseDataAccess
	{
		/// <summary>
		/// Nothing is held, so there is nothing to release - but the member is still written out, because
		/// <c>IBaseDataAccess</c> extends <see cref="IDisposable"/> and <c>BaseDataAccess</c> makes no
		/// assumption about what a derived layer owns.
		/// </summary>
		public override void Dispose()
		{
		}

		/// <summary>
		/// Transactions are outside what this folder demonstrates, and a Data Access Layer that cannot honour
		/// the transaction contract should say so rather than quietly pretend to.
		/// </summary>
		public override void TransactionStart()
		{
			throw new NotSupportedException("The convention showcases do not implement transactions.");
		}

		/// <inheritdoc cref="TransactionStart"/>
		public override void TransactionCommit()
		{
			throw new NotSupportedException("The convention showcases do not implement transactions.");
		}

		/// <inheritdoc cref="TransactionStart"/>
		public override void TransactionRollBack()
		{
			throw new NotSupportedException("The convention showcases do not implement transactions.");
		}
	}
}

using System;
using System.Collections.Generic;

namespace ProphetsWay.Example.DataAccess.NoDB
{
	/// <summary>
	/// One <see cref="ExampleDataAccess"/> instance's transaction, held as a log of the actions that would undo
	/// everything written through that instance since <see cref="Start"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>Why an undo log exists at all.</b> A write in this implementation reaches the store the moment it is
	/// made, so by the time a rollback is asked for there is nothing left to cancel - the only way back is to
	/// reverse each write, newest first. That is what this log holds: one entry per write, pushed onto a stack,
	/// replayed in reverse.
	/// </para>
	/// <para>
	/// <b>Why not the two obvious alternatives.</b> Holding writes in a pending buffer until commit would give
	/// real isolation, but every read in every Dao would then have to consult that buffer before it consulted the
	/// store, and this repository is read as an explanation of the paradigm rather than run as a database.
	/// Snapshotting the whole store at <see cref="Start"/> and restoring it on rollback is simpler still, and
	/// wrong: <see cref="DataStore"/> is process-wide, so restoring it would discard writes made by other Data
	/// Access Layer instances that were never part of this transaction. An undo log reverses only what this
	/// instance did, which is exactly the scope <c>IBaseDataAccess</c> specifies - the instance, not the
	/// connection.
	/// </para>
	/// <para>
	/// <b>The accepted cost</b> is that another instance can read a row this one has not committed:
	/// <c>READ UNCOMMITTED</c>. A real provider supplies isolation; an in-memory dictionary has nowhere else to
	/// put an uncommitted row. It is a deliberate tradeoff, and the test suite pins it so a reader can tell it
	/// from a defect.
	/// </para>
	/// </remarks>
	internal sealed class TransactionLog
	{
		private readonly Stack<Action> _undo = new Stack<Action>();

		/// <summary>
		/// Whether a transaction is currently open on the owning Data Access Layer instance.
		/// </summary>
		public bool IsOpen { get; private set; }

		/// <summary>
		/// Opens a transaction.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when one is already open. Transactions do not nest.</exception>
		public void Start()
		{
			if (IsOpen)
				throw new InvalidOperationException("A transaction is already open on this Data Access Layer, and transactions do not nest.");

			//anything written before now was committed as it happened, and is not this transaction's to undo
			_undo.Clear();
			IsOpen = true;
		}

		/// <summary>
		/// Commits the open transaction, which here is simply forgetting how to undo it.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when no transaction is open.</exception>
		public void Commit()
		{
			RequireOpen();

			_undo.Clear();
			IsOpen = false;
		}

		/// <summary>
		/// Reverses everything written since <see cref="Start"/> and leaves no transaction open.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when no transaction is open.</exception>
		public void RollBack()
		{
			RequireOpen();
			Undo();
		}

		/// <summary>
		/// Rolls back an open transaction on the way through <see cref="ExampleDataAccess.Dispose"/>.
		/// </summary>
		/// <remarks>
		/// Unlike <see cref="RollBack"/> it neither complains when nothing is open nor reports a failure. An
		/// unclosed transaction is an abandoned one and must not persist, but disposal has to be safe to call from
		/// a <c>finally</c> block - throwing here would mask whatever exception was already unwinding.
		/// </remarks>
		public void Abandon()
		{
			if (!IsOpen)
				return;

			try
			{
				Undo();
			}
			catch
			{
				//swallowed by contract; a real implementation may log it, but must not propagate it
			}
		}

		/// <summary>
		/// Enrols a write by recording how to reverse it.
		/// </summary>
		/// <param name="undo">The action that puts the store back the way it was before the write.</param>
		/// <remarks>
		/// The action is discarded when no transaction is open, which is what makes a call made outside one
		/// commit on its own.
		/// </remarks>
		public void Record(Action undo)
		{
			if (IsOpen)
				_undo.Push(undo);
		}

		/// <summary>
		/// Replays the log newest first, so a row written more than once ends up as it was before the first of
		/// those writes.
		/// </summary>
		private void Undo()
		{
			try
			{
				while (_undo.Count > 0)
					_undo.Pop()();
			}
			finally
			{
				//the instance has no transaction open once this returns, however it returns
				_undo.Clear();
				IsOpen = false;
			}
		}

		private void RequireOpen()
		{
			if (!IsOpen)
				throw new InvalidOperationException("No transaction is open on this Data Access Layer.");
		}
	}
}

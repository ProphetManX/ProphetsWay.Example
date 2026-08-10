using ProphetsWay.BaseDataAccess;
using ProphetsWay.Example.DataAccess.Entities;
using System;
using System.Collections.Generic;

namespace ProphetsWay.Example.DataAccess.IDaos
{
	/// <summary>
	/// The Data Access Layer contract for <see cref="CompanyResource"/>, declaring only the three operations
	/// that are meaningful for an entity with no identifier.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This DAO deliberately does not inherit <see cref="IBaseDao{T}"/>. It declares <see cref="Insert"/>,
	/// <see cref="Delete"/> and <see cref="GetAll"/> as its own members, and offers no <c>Update</c> and no
	/// <c>Get</c>.
	/// </para>
	///
	/// <para><b>CONTRACT.</b> Every numbered rule below is binding on any implementation.</para>
	///
	/// <para>
	/// <b>1.</b> A row is identified by <see cref="CompanyResource.CompanyId"/> and
	/// <see cref="CompanyResource.ResourceId"/> together. Every operation matches on the pair.
	/// </para>
	///
	/// <para>
	/// <b>2.</b> <see cref="Insert"/> assigns nothing back onto its argument — there is no generated
	/// identifier — so the instance the caller passed in is unchanged when the call returns.
	/// </para>
	///
	/// <para>
	/// <b>3.</b> <see cref="Insert"/> of a pair that is already stored is a no-op: the store is unchanged and
	/// no exception is thrown. At most one row therefore exists per pair.
	/// </para>
	///
	/// <para>
	/// <b>4.</b> <see cref="Delete"/> is a hard delete — the row is genuinely removed — and returns the number
	/// of rows removed, which rule 3 guarantees is never greater than <c>1</c>. Deleting a join that is not
	/// there returns <c>0</c> and throws nothing.
	/// </para>
	///
	/// <para>
	/// <b>5.</b> <see cref="GetAll"/> always returns a list, never <c>null</c>, in no guaranteed order.
	/// </para>
	///
	/// <para>
	/// <b>6.</b> The <c>item</c> parameter of <see cref="GetAll"/> is a type selector only. Reached through the
	/// generic dispatcher on <c>BaseDataAccess</c> it is <c>null</c>, so an implementation must never read it.
	/// </para>
	///
	/// <para>
	/// <b>7.</b> <see cref="Insert"/> and <see cref="Delete"/> throw <see cref="ArgumentNullException"/> when
	/// passed <c>null</c>.
	/// </para>
	///
	/// <para>
	/// <b>8.</b> <c>Get&lt;CompanyResource&gt;(object id)</c> on the generic dispatcher always throws
	/// <see cref="DataAccessConventionException"/> and can never be made to work. It fails for two independent
	/// reasons — the entity exposes no identifier property, and this DAO declares no <c>Get</c> method — and
	/// which of the two is reported is unspecified. A test must assert on the exception <i>type</i> only.
	/// </para>
	///
	/// <para>
	/// <b>9.</b> The list <see cref="GetAll"/> returns and every join in it are snapshots, and a join handed to
	/// <see cref="Insert"/> or <see cref="Delete"/> is read rather than adopted: mutating the returned list or
	/// any entity in it does not change stored data, and mutating an argument after the call returns does not
	/// reach the store. Stored data changes only through <see cref="Insert"/> and <see cref="Delete"/>, each of
	/// which reads its argument's values as they stand at the moment of the call. Two lists retrieved
	/// separately are independent of each other and of the store.
	/// </para>
	///
	/// <para><b>WHY.</b></para>
	///
	/// <para>
	/// <b><see cref="IBaseDao{T}"/> is a menu, not a mandate.</b> The base interfaces exist to save you from
	/// writing signatures you were going to write anyway; they do not oblige you to expose an operation that
	/// makes no sense for your entity. A DAO that inherits nothing and declares exactly what it supports is a
	/// legitimate member of this paradigm, and business logic depending on it is decoupled from the storage
	/// technology in exactly the same way.
	/// </para>
	///
	/// <para>
	/// <b>No <c>Update</c>.</b> A <see cref="CompanyResource"/> is nothing but its two foreign keys. There is
	/// no non-key field to change, so there is nothing to update <i>to</i> — changing either key does not
	/// modify the row, it describes a different row entirely. The correct way to change a join is to delete one
	/// and insert another.
	/// </para>
	///
	/// <para>
	/// <b>No <c>Get</c>.</b> <see cref="IBaseDao{T}"/> defines <c>Get(T)</c> in terms of an identifier field,
	/// and this entity has none. Its natural key is a pair, so single-row retrieval would need a two-argument
	/// lookup that the base interface has no way to express. The dispatcher's other members do not resolve an
	/// identifier, so <c>GetAll&lt;CompanyResource&gt;()</c> dispatches to <see cref="GetAll"/> and works
	/// normally.
	/// </para>
	///
	/// <para>
	/// <b>This shape is a novelty, not the recommended norm.</b> It is here to prove a real capability and to
	/// show an honest boundary of the library. The case it fits is narrow — a pure many-to-many join that
	/// carries no attributes of its own. Give an entity an identifier by default: it costs one column, it makes
	/// the entity addressable by the generic dispatcher, it lets a single row be updated in place, and it stops
	/// being optional the moment the join grows a field of its own. Reach for this shape only when you can say
	/// why none of that matters.
	/// </para>
	///
	/// <para>
	/// <b>Rule 9 is what makes this repository's central claim true.</b> A database hands back rows, not object
	/// references. An in-memory store that hands back the objects it is holding gives a caller a way to change
	/// stored data that no database-backed implementation can reproduce, and the claim that the same tests pass
	/// against either Data Access Layer would be quietly false. Stating the rule forces an in-memory
	/// implementation to copy on read and on write, so a dictionary-backed store and a database-backed one are
	/// genuinely interchangeable.
	/// </para>
	/// </remarks>
	public interface ICompanyResourceDao
	{
		/// <summary>
		/// Records that a company has access to a resource.
		/// </summary>
		/// <param name="item">
		/// The join to store. Both <see cref="CompanyResource.CompanyId"/> and
		/// <see cref="CompanyResource.ResourceId"/> must be set, as together they are the record's only
		/// identity.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="item"/> is <c>null</c>.
		/// </exception>
		/// <remarks>
		/// <para>
		/// Returns nothing, and — unlike <c>Insert</c> on the other DAOs — assigns nothing back onto
		/// <paramref name="item"/>. Inserting a pair that is already stored leaves the store unchanged and
		/// throws nothing.
		/// </para>
		/// <para>
		/// Does not verify that the company or the resource it names exists. Referential integrity is the
		/// storage layer's concern, and a given implementation may or may not enforce it.
		/// </para>
		/// </remarks>
		void Insert(CompanyResource item);

		/// <summary>
		/// Removes the record that a company has access to a resource.
		/// </summary>
		/// <param name="item">
		/// The join to remove, identified by both <see cref="CompanyResource.CompanyId"/> and
		/// <see cref="CompanyResource.ResourceId"/>. Matching is on the pair — supplying only one of the two
		/// does not delete every join sharing that value.
		/// </param>
		/// <returns>
		/// The number of rows removed: <c>1</c> when the join existed, <c>0</c> when no join with that pair was
		/// stored. Never greater than <c>1</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="item"/> is <c>null</c>.
		/// </exception>
		/// <remarks>
		/// A hard delete, and idempotent. <see cref="CompanyResource"/> is not a soft-delete entity, so the row
		/// is genuinely removed and a later <see cref="GetAll"/> will not return it. Contrast
		/// <see cref="IDepartmentDao"/>, where <c>Delete</c> only stamps a timestamp.
		/// </remarks>
		int Delete(CompanyResource item);

		/// <summary>
		/// Returns every company-to-resource join that is stored.
		/// </summary>
		/// <param name="item">
		/// A type selector only, matching the convention of <see cref="IBaseGetAllDao{T}"/>. Its property
		/// values are never read, and it is <c>null</c> when the call arrives through the generic dispatcher.
		/// </param>
		/// <returns>
		/// Every stored join, in no guaranteed order. An empty list when none are stored — never <c>null</c>.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The only retrieval this DAO offers. With no identifier there is no single-row <c>Get</c>, so a
		/// caller wanting the resources of one company retrieves all joins and filters them. That is acceptable
		/// for an example and for a genuinely small join table; it is another reason this shape does not scale
		/// and is not the recommended default.
		/// </para>
		/// <para>
		/// Treat the returned collection as read-only. The declared <see cref="IList{T}"/> permits
		/// <c>Add</c> and <c>Remove</c>, but an implementation is free to return a fixed-size or otherwise
		/// unmodifiable list, and under rule 9 the list and the joins in it are snapshots, so mutating either
		/// never affects stored data.
		/// </para>
		/// </remarks>
		IList<CompanyResource> GetAll(CompanyResource item);
	}
}

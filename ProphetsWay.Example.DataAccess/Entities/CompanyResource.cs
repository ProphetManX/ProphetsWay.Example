using ProphetsWay.BaseDataAccess;
using System;

namespace ProphetsWay.Example.DataAccess.Entities
{
	/// <summary>
	/// A join between a <see cref="Company"/> and a <see cref="Resource"/>, recording that the company has
	/// access to the resource, and the repository's showcase for an entity that has no identifier at all.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>Read this before copying the pattern.</b> This entity implements <see cref="IBaseEntity"/> and
	/// nothing else. It has no surrogate key and no <see cref="IBaseIdEntity{T}"/>. It exists to prove that
	/// <c>ProphetsWay.BaseDataAccess</c> supports such a shape — it is a demonstration of a boundary, not a
	/// recommended default. See <see cref="IDaos.ICompanyResourceDao"/> for what that shape costs and for the
	/// behavioral contract.
	/// </para>
	/// <para>
	/// <b>The narrow case this fits.</b> A pure many-to-many join that carries no attributes of its own: no
	/// timestamps, no ordering, no status, nothing but the two foreign keys. The moment such a table grows a
	/// third column of its own, it has become an entity in its own right and should be given an identifier.
	/// </para>
	/// <para>
	/// <b>The natural key is the pair.</b> A row is identified by <see cref="CompanyId"/> and
	/// <see cref="ResourceId"/> together. Neither alone identifies anything — one company holds many
	/// resources and one resource is held by many companies.
	/// </para>
	/// <para>
	/// <b>This type carries no behavior.</b> It is a data-carrying object with automatic properties and a
	/// default parameterless constructor, and it performs no validation. In particular nothing on this type
	/// checks that the company or the resource it names actually exists — the Data Access Layer contract
	/// nevertheless requires the caller to name rows that do, and a store that enforces referential integrity
	/// will reject a join that does not.
	/// </para>
	/// <para>
	/// Equality is the default reference equality inherited from <see cref="object"/>. Two instances naming
	/// the same company and the same resource are not equal to one another; compare the two identifier
	/// properties instead.
	/// </para>
	/// </remarks>
	public class CompanyResource : IBaseEntity
	{
		/// <summary>
		/// The identifier of the <see cref="Company"/> side of this join.
		/// </summary>
		/// <value>
		/// The company's <see cref="Company.Id"/>. Zero on a newly constructed instance, and zero is not a
		/// meaningful identifier.
		/// </value>
		/// <remarks>
		/// Set by the caller before <c>Insert</c>, <c>Delete</c>, or any lookup. Never assigned by the Data
		/// Access Layer — unlike a surrogate key, this value is supplied, not generated. This type does not
		/// validate it, but <c>Insert</c> requires it to name a company that exists.
		/// </remarks>
		public int CompanyId { get; set; }

		/// <summary>
		/// The identifier of the <see cref="Resource"/> side of this join.
		/// </summary>
		/// <value>
		/// The resource's <see cref="Resource.Id"/>. <see cref="Guid.Empty"/> on a newly constructed instance,
		/// and <see cref="Guid.Empty"/> is not a meaningful identifier.
		/// </value>
		/// <remarks>
		/// Set by the caller before <c>Insert</c>, <c>Delete</c>, or any lookup. Never assigned by the Data
		/// Access Layer. This type does not validate it, but <c>Insert</c> requires it to name a resource that
		/// exists.
		/// </remarks>
		public Guid ResourceId { get; set; }
	}
}

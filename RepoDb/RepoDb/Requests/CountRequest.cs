﻿using RepoDb.Interfaces;
using System;
using System.Data;

namespace RepoDb.Requests
{
    /// <summary>
    /// A class that holds the value of the count operation arguments.
    /// </summary>
    internal class CountRequest : BaseRequest, IEquatable<CountRequest>
    {
        private int? m_hashCode = null;

        /// <summary>
        /// Creates a new instance of <see cref="CountRequest"/> object.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="connection">The connection object.</param>
        /// <param name="where">The query expression.</param>
        /// <param name="statementBuilder">The statement builder.</param>
        public CountRequest(Type entityType, IDbConnection connection, QueryGroup where = null, IStatementBuilder statementBuilder = null)
            : base(entityType, connection, statementBuilder)
        {
            Where = where;
        }

        /// <summary>
        /// Gets the query expression used.
        /// </summary>
        public QueryGroup Where { get; }

        // Equality and comparers

        /// <summary>
        /// Returns the hashcode for this <see cref="CountRequest"/>.
        /// </summary>
        /// <returns>The hashcode value.</returns>
        public override int GetHashCode()
        {
            // Make sure to return if it is already provided
            if (!ReferenceEquals(null, m_hashCode))
            {
                return m_hashCode.Value;
            }

            // Get first the entity hash code
            var hashCode = $"Count.{EntityType.FullName}".GetHashCode();

            // Get the properties hash codes
            if (Where != null)
            {
                hashCode += Where.GetHashCode();
            }

            // Set back the hash code value
            m_hashCode = hashCode;

            // Return the actual value
            return hashCode;
        }

        /// <summary>
        /// Compares the <see cref="CountRequest"/> object equality against the given target object.
        /// </summary>
        /// <param name="obj">The object to be compared to the current object.</param>
        /// <returns>True if the instances are equals.</returns>
        public override bool Equals(object obj)
        {
            return GetHashCode() == obj?.GetHashCode();
        }

        /// <summary>
        /// Compares the <see cref="CountRequest"/> object equality against the given target object.
        /// </summary>
        /// <param name="other">The object to be compared to the current object.</param>
        /// <returns>True if the instances are equal.</returns>
        public bool Equals(CountRequest other)
        {
            return GetHashCode() == other?.GetHashCode();
        }

        /// <summary>
        /// Compares the equality of the two <see cref="CountRequest"/> objects.
        /// </summary>
        /// <param name="objA">The first <see cref="CountRequest"/> object.</param>
        /// <param name="objB">The second <see cref="CountRequest"/> object.</param>
        /// <returns>True if the instances are equal.</returns>
        public static bool operator ==(CountRequest objA, CountRequest objB)
        {
            if (ReferenceEquals(null, objA))
            {
                return ReferenceEquals(null, objB);
            }
            return objA?.GetHashCode() == objB?.GetHashCode();
        }

        /// <summary>
        /// Compares the inequality of the two <see cref="CountRequest"/> objects.
        /// </summary>
        /// <param name="objA">The first <see cref="CountRequest"/> object.</param>
        /// <param name="objB">The second <see cref="CountRequest"/> object.</param>
        /// <returns>True if the instances are not equal.</returns>
        public static bool operator !=(CountRequest objA, CountRequest objB)
        {
            return (objA == objB) == false;
        }
    }
}

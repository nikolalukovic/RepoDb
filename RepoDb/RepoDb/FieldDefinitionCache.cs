﻿using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RepoDb
{
    /// <summary>
    /// A class used to cache the field definitions of the entity.
    /// </summary>
    internal static class FieldDefinitionCache
    {
        private static readonly ConcurrentDictionary<string, IEnumerable<FieldDefinition>> m_cache = new ConcurrentDictionary<string, IEnumerable<FieldDefinition>>();

        /// <summary>
        /// Gets the cached field definitions of the entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the target entity.</typeparam>
        /// <returns>The cached field definitions of the entity.</returns>
        public static IEnumerable<FieldDefinition> Get<TEntity>()
            where TEntity : class
        {
            var key = typeof(TEntity).FullName;
            var result = (IEnumerable<FieldDefinition>)null;
            m_cache.TryGetValue(key, out result);
            return result;
        }

        /// <summary>
        /// Gets the cached field definitions of the entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the target entity.</typeparam>
        /// <param name="connectionString">The connection string to be used.</param>
        /// <returns>The cached field definitions of the entity.</returns>
        public static IEnumerable<FieldDefinition> Get<TEntity>(string connectionString)
            where TEntity : class
        {
            var key = typeof(TEntity).FullName;
            var result = (IEnumerable<FieldDefinition>)null;
            if (m_cache.TryGetValue(key, out result) == false)
            {
                result = SqlDbHelper.GetFieldDefinitions(connectionString, ClassMappedNameCache.Get<TEntity>());
                m_cache.TryAdd(key, result);
            }
            return result;
        }
    }
}

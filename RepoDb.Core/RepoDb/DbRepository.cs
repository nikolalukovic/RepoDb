﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using RepoDb.Interfaces;
using System.Threading.Tasks;
using RepoDb.Enumerations;
using RepoDb.Attributes;
using System.Linq.Expressions;

namespace RepoDb
{
    /// <summary>
    /// A base object for all <b>Shared-Based Repositories</b>. This object is usually being inheritted if
    /// the derived class is meant for shared-based operations when it comes to data manipulations.
    /// This object is used by <see cref="BaseRepository{TEntity, TDbConnection}"/> as an underlying repository for all its
    /// operations.
    /// </summary>
    /// <typeparam name="TDbConnection">The type of the <see cref="DbConnection"/> object.</typeparam>
    public class DbRepository<TDbConnection> : IDisposable where TDbConnection : DbConnection
    {
        #region Fields

        private static readonly object m_syncLock = new object();
        private TDbConnection m_connection;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="DbRepository{TDbConnection}"/> object.
        /// </summary>
        /// <param name="connectionString">The connection string to be used by this repository.</param>
        public DbRepository(string connectionString)
            : this(connectionString, null, null, null, null, ConnectionPersistency.PerCall)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DbRepository{TDbConnection}"/> object.
        /// </summary>
        /// <param name="connectionString">The connection string to be used by this repository.</param>
        /// <param name="commandTimeout">The command timeout in seconds to be used on every operations by this repository.</param>
        public DbRepository(string connectionString, int? commandTimeout)
            : this(connectionString, commandTimeout, null, null, null, ConnectionPersistency.PerCall)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DbRepository{TDbConnection}"/> object.
        /// </summary>
        /// <param name="connectionString">The connection string to be used by this repository.</param>
        /// <param name="cache">The cache object to be used by this repository. This object must implement the <see cref="ICache"/> interface.</param>
        public DbRepository(string connectionString, ICache cache)
            : this(connectionString, null, cache, null, null, ConnectionPersistency.PerCall)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DbRepository{TDbConnection}"/> object.
        /// </summary>
        /// <param name="connectionString">The connection string to be used by this repository.</param>
        /// <param name="trace">The trace object to be used by this repository. This object must implement the <see cref="ITrace"/> interface.</param>
        public DbRepository(string connectionString, ITrace trace)
            : this(connectionString, null, null, trace, null, ConnectionPersistency.PerCall)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DbRepository{TDbConnection}"/> object.
        /// </summary>
        /// <param name="connectionString">The connection string to be used by this repository.</param>
        /// <param name="statementBuilder">The SQL statement builder object to be used by this repository. This object must implement the <see cref="ITrace"/> interface.</param>
        public DbRepository(string connectionString, IStatementBuilder statementBuilder)
            : this(connectionString, null, null, null, statementBuilder, ConnectionPersistency.PerCall)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DbRepository{TDbConnection}"/> object.
        /// </summary>
        /// <param name="connectionString">The connection string to be used by this repository.</param>
        /// <param name="connectionPersistency">
        /// The database connection persistency type. Setting to <see cref="ConnectionPersistency.Instance"/> will make the repository re-used a single connection all throughout its lifespan. Setting 
        /// to <see cref="ConnectionPersistency.PerCall"/> will create a new connection object on every repository call.
        /// </param>
        public DbRepository(string connectionString, ConnectionPersistency connectionPersistency)
            : this(connectionString, null, null, null, null, connectionPersistency)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DbRepository{TDbConnection}"/> object.
        /// </summary>
        /// <param name="connectionString">The connection string to be used by this repository.</param>
        /// <param name="commandTimeout">The command timeout in seconds to be used on every operation by this repository.</param>
        /// <param name="cache">The cache object to be used by this repository. This object must implement the <see cref="ICache"/> interface.</param>
        public DbRepository(string connectionString, int? commandTimeout, ICache cache)
            : this(connectionString, commandTimeout, cache, null, null, ConnectionPersistency.PerCall)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DbRepository{TDbConnection}"/> object.
        /// </summary>
        /// <param name="connectionString">The connection string to be used by this repository.</param>
        /// <param name="commandTimeout">The command timeout in seconds to be used on every operation by this repository.</param>
        /// <param name="cache">The cache object to be used by this repository. This object must implement the <see cref="ICache"/> interface.</param>
        /// <param name="trace">The trace object to be used by this repository. This object must implement the <see cref="ITrace"/> interface.</param>
        public DbRepository(string connectionString, int? commandTimeout, ICache cache, ITrace trace)
            : this(connectionString, commandTimeout, cache, trace, null, ConnectionPersistency.PerCall)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DbRepository{TDbConnection}"/> object.
        /// </summary>
        /// <param name="connectionString">The connection string to be used by this repository.</param>
        /// <param name="commandTimeout">The command timeout in seconds to be used on every operation by this repository.</param>
        /// <param name="cache">The cache object to be used by this repository. This object must implement the <see cref="ICache"/> interface.</param>
        /// <param name="trace">The trace object to be used by this repository. This object must implement the <see cref="ITrace"/> interface.</param>
        /// <param name="statementBuilder">The SQL statement builder object to be used by this repository. This object must implement the <see cref="IStatementBuilder"/> interface.</param>
        public DbRepository(string connectionString, int? commandTimeout, ICache cache, ITrace trace, IStatementBuilder statementBuilder)
            : this(connectionString, commandTimeout, cache, trace, statementBuilder, ConnectionPersistency.PerCall)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DbRepository{TDbConnection}"/> object.
        /// </summary>
        /// <param name="connectionString">The connection string to be used by this repository.</param>
        /// <param name="commandTimeout">The command timeout in seconds to be used on every operation by this repository.</param>
        /// <param name="cache">The cache object to be used by this repository. This object must implement the <see cref="ICache"/> interface.</param>
        /// <param name="trace">The trace object to be used by this repository. This object must implement the <see cref="ITrace"/> interface.</param>
        /// <param name="statementBuilder">The SQL statement builder object to be used by this repository. This object must implement the <see cref="IStatementBuilder"/> interface.</param>
        /// <param name="connectionPersistency">
        /// The database connection persistency type. Setting to <see cref="ConnectionPersistency.Instance"/> will make the repository re-used a single connection all throughout its lifespan. Setting 
        /// to <see cref="ConnectionPersistency.PerCall"/> will create a new connection object on every repository call.
        /// </param>
        public DbRepository(string connectionString, int? commandTimeout, ICache cache, ITrace trace, IStatementBuilder statementBuilder,
            ConnectionPersistency connectionPersistency)
        {
            // Properties
            ConnectionString = connectionString;
            CommandTimeout = commandTimeout;
            Cache = (cache ?? new MemoryCache());
            Trace = trace;
            StatementBuilder = (statementBuilder ?? StatementBuilderMapper.Get(typeof(TDbConnection))?.StatementBuilder ?? new SqlDbStatementBuilder());
            ConnectionPersistency = connectionPersistency;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the connection used by this repository.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets the command timeout value in seconds that is being used by this repository on every operation.
        /// </summary>
        public int? CommandTimeout { get; }

        /// <summary>
        /// Gets the cache object that is being used by this repository.
        /// </summary>
        public ICache Cache { get; }

        /// <summary>
        /// Gets the trace object that is being used by this repository.
        /// </summary>
        public ITrace Trace { get; }

        /// <summary>
        /// Gets the statement builder object that is being used by this repository.
        /// </summary>
        public IStatementBuilder StatementBuilder { get; }

        /// <summary>
        /// Gets the database connection persistency used by this repository. The default value is <see cref="ConnectionPersistency.PerCall"/>.
        /// </summary>
        public ConnectionPersistency ConnectionPersistency { get; }

        #endregion

        #region Other Methods

        // CreateConnection (TDbConnection)

        /// <summary>
        /// Creates a new instance of the database connection. If the value <see cref="ConnectionPersistency"/> property is <see cref="ConnectionPersistency.Instance"/>, then this will return
        /// the <see cref="DbConnection"/> that is being used by the current repository instance. However, if the value of the <see cref="ConnectionPersistency"/>
        /// property is <see cref="ConnectionPersistency.PerCall"/>, then this will return a new instance of the <see cref="DbConnection"/> object.
        /// </summary>
        /// <returns>An instance of the <see cref="DbConnection"/> object.</returns>
        public TDbConnection CreateConnection()
        {
            return CreateConnection(false);
        }

        /// <summary>
        /// Creates a new instance of the database connection. If the value <see cref="ConnectionPersistency"/> property is <see cref="ConnectionPersistency.Instance"/>, then this will return
        /// the <see cref="DbConnection"/> that is being used by the current repository instance. However, if the value of the <see cref="ConnectionPersistency"/>
        /// property is <see cref="ConnectionPersistency.PerCall"/>, then this will return a new instance of the <see cref="DbConnection"/> object.
        /// </summary>
        /// <param name="force">Set to true to forcely create a new instance of <see cref="DbConnection"/> object regardless of the persistency.</param>
        /// <returns>An instance of the <see cref="DbConnection"/> object.</returns>
        public TDbConnection CreateConnection(bool force)
        {
            var connection = (TDbConnection)null;
            if (force == false && ConnectionPersistency == ConnectionPersistency.Instance)
            {
                lock (m_syncLock)
                {
                    if (m_connection == null)
                    {
                        connection = Activator.CreateInstance<TDbConnection>();
                        connection.ConnectionString = ConnectionString;
                        m_connection = connection;
                    }
                    else
                    {
                        connection = m_connection;
                    }
                }
            }
            else
            {
                connection = Activator.CreateInstance<TDbConnection>();
                connection.ConnectionString = ConnectionString;
            }
            return connection;
        }

        /// <summary>
        /// Dispose the current repository instance (of type <see cref="DbRepository{TDbConnection}"/>). It is not necessary to call this method if the value of the <see cref="ConnectionPersistency"/>
        /// property is equals to <see cref="ConnectionPersistency.PerCall"/>. This method only manages the connection persistency for the repositories where the value
        /// of the <see cref="ConnectionPersistency"/> property is equals to <see cref="ConnectionPersistency.Instance"/>.
        /// </summary>
        public void Dispose()
        {
            if (ConnectionPersistency == ConnectionPersistency.Instance)
            {
                m_connection?.Dispose();
            }
        }

        #endregion

        #region Operational Methods

        // BatchQuery

        /// <summary>
        /// Query the data from the database by batch. The batching will vary on the page number and number of rows per batch defined on this
        /// operation. This operation is useful for paging purposes.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="page">The page of the batch to be used by this operation.</param>
        /// <param name="rowsPerBatch">The number of rows per batch to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public IEnumerable<TEntity> BatchQuery<TEntity>(int page, int rowsPerBatch,
            IEnumerable<OrderField> orderBy, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.BatchQuery<TEntity>(page: page,
                rowsPerBatch: rowsPerBatch,
                orderBy: orderBy,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Query the data from the database by batch based on the given query expression. The batching will vary on the page number and number of rows
        /// per batch defined by this operation. This operation is useful for paging purposes.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="whereOrWhat">The query expression or primary key value to be used by this operation.</param>
        /// <param name="page">The page of the batch to be used by this operation.</param>
        /// <param name="rowsPerBatch">The number of rows per batch to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public IEnumerable<TEntity> BatchQuery<TEntity>(object whereOrWhat, int page, int rowsPerBatch,
            IEnumerable<OrderField> orderBy, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.BatchQuery<TEntity>(whereOrWhat: whereOrWhat,
                page: page,
                rowsPerBatch: rowsPerBatch,
                orderBy: orderBy,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Query the data from the database by batch based on the given query expression. The batching will vary on the page number and number of rows
        /// per batch defined by this operation. This operation is useful for paging purposes.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="page">The page of the batch to be used by this operation.</param>
        /// <param name="rowsPerBatch">The number of rows per batch to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public IEnumerable<TEntity> BatchQuery<TEntity>(Expression<Func<TEntity, bool>> where, int page, int rowsPerBatch,
            IEnumerable<OrderField> orderBy, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.BatchQuery<TEntity>(where: where,
                page: page,
                rowsPerBatch: rowsPerBatch,
                orderBy: orderBy,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Query the data from the database by batch based on the given query expression. The batching will vary on the page number and number of rows
        /// per batch defined by this operation. This operation is useful for paging purposes.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="page">The page of the batch to be used by this operation.</param>
        /// <param name="rowsPerBatch">The number of rows per batch to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public IEnumerable<TEntity> BatchQuery<TEntity>(QueryField where, int page, int rowsPerBatch,
            IEnumerable<OrderField> orderBy, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.BatchQuery<TEntity>(where: where,
                page: page,
                rowsPerBatch: rowsPerBatch,
                orderBy: orderBy,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Query the data from the database by batch based on the given query expression. The batching will vary on the page number and number of rows
        /// per batch defined by this operation. This operation is useful for paging purposes.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="page">The page of the batch to be used by this operation.</param>
        /// <param name="rowsPerBatch">The number of rows per batch to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public IEnumerable<TEntity> BatchQuery<TEntity>(IEnumerable<QueryField> where, int page, int rowsPerBatch,
            IEnumerable<OrderField> orderBy, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.BatchQuery<TEntity>(where: where,
                page: page,
                rowsPerBatch: rowsPerBatch,
                orderBy: orderBy,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Query the data from the database by batch based on the given query expression. The batching will vary on the page number and number of rows
        /// per batch defined by this operation. This operation is useful for paging purposes.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="page">The page of the batch to be used by this operation.</param>
        /// <param name="rowsPerBatch">The number of rows per batch to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public IEnumerable<TEntity> BatchQuery<TEntity>(QueryGroup where, int page, int rowsPerBatch,
            IEnumerable<OrderField> orderBy, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.BatchQuery<TEntity>(where: where,
                page: page,
                rowsPerBatch: rowsPerBatch,
                orderBy: orderBy,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // BatchQueryAsync

        /// <summary>
        /// Query the data from the database by batch in an asynchronous way. The batching will vary on the page number and number of rows per batch defined on this
        /// operation. This operation is useful for paging purposes.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="page">The page of the batch to be used by this operation.</param>
        /// <param name="rowsPerBatch">The number of rows per batch to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public Task<IEnumerable<TEntity>> BatchQueryAsync<TEntity>(int page, int rowsPerBatch,
            IEnumerable<OrderField> orderBy, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                BatchQuery<TEntity>(page: page,
                    rowsPerBatch: rowsPerBatch,
                    orderBy: orderBy,
                    transaction: transaction));
        }

        /// <summary>
        /// Query the data from the database by batch based on the given query expression in an asynchronous way. The batching will vary on the page number and number of rows
        /// per batch defined by this operation. This operation is useful for paging purposes.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="whereOrWhat">The query expression or primary key value to be used by this operation.</param>
        /// <param name="page">The page of the batch to be used by this operation.</param>
        /// <param name="rowsPerBatch">The number of rows per batch to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public Task<IEnumerable<TEntity>> BatchQueryAsync<TEntity>(object whereOrWhat, int page, int rowsPerBatch,
            IEnumerable<OrderField> orderBy, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                BatchQuery<TEntity>(whereOrWhat: whereOrWhat,
                    page: page,
                    rowsPerBatch: rowsPerBatch,
                    orderBy: orderBy,
                    transaction: transaction));
        }

        /// <summary>
        /// Query the data from the database by batch based on the given query expression in an asynchronous way. The batching will vary on the page number and number of rows
        /// per batch defined by this operation. This operation is useful for paging purposes.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="page">The page of the batch to be used by this operation.</param>
        /// <param name="rowsPerBatch">The number of rows per batch to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public Task<IEnumerable<TEntity>> BatchQueryAsync<TEntity>(Expression<Func<TEntity, bool>> where, int page, int rowsPerBatch,
            IEnumerable<OrderField> orderBy, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                BatchQuery<TEntity>(where: where,
                    page: page,
                    rowsPerBatch: rowsPerBatch,
                    orderBy: orderBy,
                    transaction: transaction));
        }

        /// <summary>
        /// Query the data from the database by batch based on the given query expression in an asynchronous way. The batching will vary on the page number and number of rows
        /// per batch defined by this operation. This operation is useful for paging purposes.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="page">The page of the batch to be used by this operation.</param>
        /// <param name="rowsPerBatch">The number of rows per batch to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public Task<IEnumerable<TEntity>> BatchQueryAsync<TEntity>(QueryField where, int page, int rowsPerBatch,
            IEnumerable<OrderField> orderBy, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                BatchQuery<TEntity>(where: where,
                    page: page,
                    rowsPerBatch: rowsPerBatch,
                    orderBy: orderBy,
                    transaction: transaction));
        }

        /// <summary>
        /// Query the data from the database by batch based on the given query expression in an asynchronous way. The batching will vary on the page number and number of rows
        /// per batch defined by this operation. This operation is useful for paging purposes.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="page">The page of the batch to be used by this operation.</param>
        /// <param name="rowsPerBatch">The number of rows per batch to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public Task<IEnumerable<TEntity>> BatchQueryAsync<TEntity>(IEnumerable<QueryField> where, int page, int rowsPerBatch,
            IEnumerable<OrderField> orderBy, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                BatchQuery<TEntity>(where: where,
                    page: page,
                    rowsPerBatch: rowsPerBatch,
                    orderBy: orderBy,
                    transaction: transaction));
        }

        /// <summary>
        /// Query the data from the database by batch based on the given query expression in an asynchronous way. The batching will vary on the page number and number of rows
        /// per batch defined by this operation. This operation is useful for paging purposes.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="page">The page of the batch to be used by this operation.</param>
        /// <param name="rowsPerBatch">The number of rows per batch to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public Task<IEnumerable<TEntity>> BatchQueryAsync<TEntity>(QueryGroup where, int page, int rowsPerBatch,
            IEnumerable<OrderField> orderBy, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                BatchQuery<TEntity>(where: where,
                    page: page,
                    rowsPerBatch: rowsPerBatch,
                    orderBy: orderBy,
                    transaction: transaction));
        }

        // BulkInsert

        /// <summary>
        /// Bulk-inserting the list of data entity objects in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entities">The list of the data entities to be bulk-inserted.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int BulkInsert<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            // Create a connection
            var connection = CreateConnection();

            // Call the method
            var result = connection.BulkInsert<TEntity>(entities: entities,
                commandTimeout: CommandTimeout,
                trace: Trace);

            // Dispose the connection
            if (ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // BulkInsertAsync

        /// <summary>
        /// Bulk-inserting the list of data entity objects in the database in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entities">The list of the data entities to be bulk-inserted.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> BulkInsertAsync<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                BulkInsert<TEntity>(entities: entities));
        }

        // Count

        /// <summary>
        /// Counts the number of rows from the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An integer value for the number of rows counted from the database.</returns>
        public long Count<TEntity>(IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Count<TEntity>(commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Counts the number of rows from the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="whereOrWhat">The query expression or primary key value to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An integer value for the number of rows counted from the database based on the given query expression.</returns>
        public long Count<TEntity>(object whereOrWhat, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Count<TEntity>(commandTimeout: CommandTimeout,
                whereOrWhat: whereOrWhat,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Counts the number of rows from the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An integer value for the number of rows counted from the database based on the given query expression.</returns>
        public long Count<TEntity>(Expression<Func<TEntity, bool>> where, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Count<TEntity>(where: where,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Counts the number of rows from the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An integer value for the number of rows counted from the database based on the given query expression.</returns>
        public long Count<TEntity>(QueryField where, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Count<TEntity>(where: where,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Counts the number of rows from the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An integer value for the number of rows counted from the database based on the given query expression.</returns>
        public long Count<TEntity>(IEnumerable<QueryField> where, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Count<TEntity>(where: where,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Counts the number of rows from the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An integer value for the number of rows counted from the database based on the given query expression.</returns>
        public long Count<TEntity>(QueryGroup where, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Count<TEntity>(where: where,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // CountAsync

        /// <summary>
        /// Counts the number of rows from the database in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An integer value for the number of rows counted from the database.</returns>
        public Task<long> CountAsync<TEntity>(IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Count<TEntity>(transaction: transaction));
        }

        /// <summary>
        /// Counts the number of rows from the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="whereOrWhat">The query expression or primary key value to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An integer value for the number of rows counted from the database based on the given query expression.</returns>
        public Task<long> CountAsync<TEntity>(object whereOrWhat, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Count<TEntity>(whereOrWhat: whereOrWhat,
                    transaction: transaction));
        }

        /// <summary>
        /// Counts the number of rows from the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An integer value for the number of rows counted from the database based on the given query expression.</returns>
        public Task<long> CountAsync<TEntity>(Expression<Func<TEntity, bool>> where, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Count<TEntity>(where: where,
                    transaction: transaction));
        }

        /// <summary>
        /// Counts the number of rows from the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An integer value for the number of rows counted from the database based on the given query expression.</returns>
        public Task<long> CountAsync<TEntity>(QueryField where, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Count<TEntity>(where: where,
                    transaction: transaction));
        }

        /// <summary>
        /// Counts the number of rows from the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An integer value for the number of rows counted from the database based on the given query expression.</returns>
        public Task<long> CountAsync<TEntity>(IEnumerable<QueryField> where, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Count<TEntity>(where: where,
                    transaction: transaction));
        }

        /// <summary>
        /// Counts the number of rows from the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An integer value for the number of rows counted from the database based on the given query expression.</returns>
        public Task<long> CountAsync<TEntity>(QueryGroup where, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Count<TEntity>(where: where,
                    transaction: transaction));
        }

        // Delete

        /// <summary>
        /// Deletes all data in the database based on the target data entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Delete<TEntity>(IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Delete<TEntity>(commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Deletes a data in the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Delete<TEntity>(Expression<Func<TEntity, bool>> where, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Delete<TEntity>(where: where,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Deletes a data in the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Delete<TEntity>(QueryField where, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Delete<TEntity>(where: where,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Deletes a data in the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Delete<TEntity>(IEnumerable<QueryField> where, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Delete<TEntity>(where: where,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Deletes a data in the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="whereOrWhat">The query expression or primary key value to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Delete<TEntity>(object whereOrWhat, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Delete<TEntity>(whereOrWhat: whereOrWhat,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Deletes a data in the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Delete<TEntity>(QueryGroup where, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Delete<TEntity>(where: where,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // DeleteAsync

        /// <summary>
        /// Deletes all data in the database based on the target data entity in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> DeleteAsync<TEntity>(IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Delete<TEntity>(transaction: transaction));
        }

        /// <summary>
        /// Deletes a data in the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> where, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Delete<TEntity>(where: where,
                    transaction: transaction));
        }

        /// <summary>
        /// Deletes a data in the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> DeleteAsync<TEntity>(QueryField where, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Delete<TEntity>(where: where,
                    transaction: transaction));
        }

        /// <summary>
        /// Deletes a data in the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> DeleteAsync<TEntity>(IEnumerable<QueryField> where, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Delete<TEntity>(where: where,
                    transaction: transaction));
        }

        /// <summary>
        /// Deletes a data in the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="whereOrWhat">The query expression or primary key value to be used by this operation. When is set to null, it deletes all the data from the database.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> DeleteAsync<TEntity>(object whereOrWhat, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Delete<TEntity>(whereOrWhat: whereOrWhat,
                    transaction: transaction));
        }

        /// <summary>
        /// Deletes a data in the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> DeleteAsync<TEntity>(QueryGroup where, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Delete<TEntity>(where: where,
                    transaction: transaction));
        }

        // DeleteAll

        /// <summary>
        /// Deletes all data in the database based on the target data entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int DeleteAll<TEntity>(IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.DeleteAll<TEntity>(commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // DeleteAllAsync

        /// <summary>
        /// Deletes all data in the database based on the target data entity in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> DeleteAllAsync<TEntity>(IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                DeleteAll<TEntity>(transaction: transaction));
        }

        // InlineInsert

        /// <summary>
        /// Inserts a data in the database by targetting certain fields only.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The object that contains the targetted columns to be inserted.</param>
        /// <param name="overrideIgnore">True if to allow the insert operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>
        /// The value of the primary key of the newly inserted data entity object. Returns null if the 
        /// primary key property is not present.
        /// </returns>
        public object InlineInsert<TEntity>(object entity, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.InlineInsert<TEntity>(entity: entity,
                overrideIgnore: overrideIgnore,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // InlineInsertAsync

        /// <summary>
        /// Inserts a data in the database by targetting certain fields only in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The object that contains the targetted columns to be inserted.</param>
        /// <param name="overrideIgnore">True if to allow the insert operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>
        /// The value of the primary key of the newly inserted data entity object. Returns null if the 
        /// primary key property is not present.
        /// </returns>
        public Task<object> InlineInsertAsync<TEntity>(object entity, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                InlineInsert<TEntity>(entity: entity,
                    overrideIgnore: overrideIgnore,
                    transaction: transaction));
        }

        // InlineMerge

        /// <summary>
        /// Merges a data in the database by targetting certain fields only. It uses the primary key as the default qualifier field.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be merged.</param>
        /// <param name="overrideIgnore">True if to allow the merge operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int InlineMerge<TEntity>(object entity, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.InlineMerge<TEntity>(entity: entity,
                overrideIgnore: overrideIgnore,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Merges a data in the database by targetting certain fields only.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be merged.</param>
        /// <param name="qualifier">The qualifier field to be used by the inline merge operation on a SQL Statement.</param>
        /// <param name="overrideIgnore">True if to allow the merge operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int InlineMerge<TEntity>(object entity, Expression<Func<TEntity, object>> qualifier, bool? overrideIgnore = false,
            IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.InlineMerge<TEntity>(entity: entity,
                qualifier: qualifier,
                overrideIgnore: overrideIgnore,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Merges a data in the database by targetting certain fields only.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be merged.</param>
        /// <param name="qualifier">The qualifier field to be used by the inline merge operation on a SQL Statement.</param>
        /// <param name="overrideIgnore">True if to allow the merge operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int InlineMerge<TEntity>(object entity, Field qualifier, bool? overrideIgnore = false,
            IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.InlineMerge<TEntity>(entity: entity,
                qualifier: qualifier,
                overrideIgnore: overrideIgnore,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Merges a data in the database by targetting certain fields only.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be merged.</param>
        /// <param name="qualifiers">The list of the qualifier fields to be used by the inline merge operation on a SQL Statement.</param>
        /// <param name="overrideIgnore">True if to allow the merge operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int InlineMerge<TEntity>(object entity, IEnumerable<Field> qualifiers, bool? overrideIgnore = false,
            IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.InlineMerge<TEntity>(entity: entity,
                qualifiers: qualifiers,
                overrideIgnore: overrideIgnore,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // InlineMergeAsync

        /// <summary>
        /// Merges a data in the database by targetting certain fields only in an asynchronous way. Uses the primary key as the default qualifier field.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be merged.</param>
        /// <param name="overrideIgnore">True if to allow the merge operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> InlineMergeAsync<TEntity>(object entity, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                InlineMerge<TEntity>(entity: entity,
                    overrideIgnore: overrideIgnore,
                    transaction: transaction));
        }

        /// <summary>
        /// Merges a data in the database by targetting certain fields only in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be merged.</param>
        /// <param name="qualifier">The qualifier field to be used by the inline merge operation on a SQL Statement.</param>
        /// <param name="overrideIgnore">True if to allow the merge operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> InlineMergeAsync<TEntity>(object entity, Expression<Func<TEntity, object>> qualifier, bool? overrideIgnore = false,
            IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                InlineMerge<TEntity>(entity: entity,
                    qualifier: qualifier,
                    overrideIgnore: overrideIgnore,
                    transaction: transaction));
        }

        /// <summary>
        /// Merges a data in the database by targetting certain fields only in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be merged.</param>
        /// <param name="qualifier">The qualifier field to be used by the inline merge operation on a SQL Statement.</param>
        /// <param name="overrideIgnore">True if to allow the merge operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> InlineMergeAsync<TEntity>(object entity, Field qualifier, bool? overrideIgnore = false,
            IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                InlineMerge<TEntity>(entity: entity,
                    qualifier: qualifier,
                    overrideIgnore: overrideIgnore,
                    transaction: transaction));
        }

        /// <summary>
        /// Merges a data in the database by targetting certain fields only in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be merged.</param>
        /// <param name="qualifiers">The list of the qualifier fields to be used by the inline merge operation on a SQL Statement.</param>
        /// <param name="overrideIgnore">True if to allow the merge operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> InlineMergeAsync<TEntity>(object entity, IEnumerable<Field> qualifiers, bool? overrideIgnore = false,
            IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                InlineMerge<TEntity>(entity: entity,
                    qualifiers: qualifiers,
                    overrideIgnore: overrideIgnore,
                    transaction: transaction));
        }

        // InlineUpdate

        /// <summary>
        /// Updates a data in the database by targetting certain fields only.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be updated.</param>
        /// <param name="whereOrWhat">The query expression or primary key value to be used by this operation.</param>
        /// <param name="overrideIgnore">True if to allow the update operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int InlineUpdate<TEntity>(object entity, object whereOrWhat, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.InlineUpdate<TEntity>(entity: entity,
                whereOrWhat: whereOrWhat,
                overrideIgnore: overrideIgnore,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Updates a data in the database by targetting certain fields only.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="overrideIgnore">True if to allow the update operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int InlineUpdate<TEntity>(object entity, Expression<Func<TEntity, bool>> where, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.InlineUpdate<TEntity>(entity: entity,
                where: where,
                overrideIgnore: overrideIgnore,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Updates a data in the database by targetting certain fields only.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="overrideIgnore">True if to allow the update operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int InlineUpdate<TEntity>(object entity, QueryField where, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.InlineUpdate<TEntity>(entity: entity,
                where: where,
                overrideIgnore: overrideIgnore,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Updates a data in the database by targetting certain fields only.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="overrideIgnore">True if to allow the update operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int InlineUpdate<TEntity>(object entity, IEnumerable<QueryField> where, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.InlineUpdate<TEntity>(entity: entity,
                where: where,
                overrideIgnore: overrideIgnore,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Updates a data in the database by targetting certain fields only.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="overrideIgnore">True if to allow the update operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int InlineUpdate<TEntity>(object entity, QueryGroup where, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.InlineUpdate<TEntity>(entity: entity,
                where: where,
                overrideIgnore: overrideIgnore,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // InlineUpdateAsync

        /// <summary>
        /// Updates a data in the database by targetting certain fields only in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be updated.</param>
        /// <param name="whereOrWhat">The query expression or primary key value to be used by this operation.</param>
        /// <param name="overrideIgnore">True if to allow the update operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> InlineUpdateAsync<TEntity>(object entity, object whereOrWhat, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                InlineUpdate<TEntity>(entity: entity,
                    whereOrWhat: whereOrWhat,
                    overrideIgnore: overrideIgnore,
                    transaction: transaction));
        }

        /// <summary>
        /// Updates a data in the database by targetting certain fields only in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="overrideIgnore">True if to allow the update operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> InlineUpdateAsync<TEntity>(object entity, Expression<Func<TEntity, bool>> where, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                InlineUpdate<TEntity>(entity: entity,
                    where: where,
                    overrideIgnore: overrideIgnore,
                    transaction: transaction));
        }

        /// <summary>
        /// Updates a data in the database by targetting certain fields only in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="overrideIgnore">True if to allow the update operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> InlineUpdateAsync<TEntity>(object entity, QueryField where, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                InlineUpdate<TEntity>(entity: entity,
                    where: where,
                    overrideIgnore: overrideIgnore,
                    transaction: transaction));
        }

        /// <summary>
        /// Updates a data in the database by targetting certain fields only in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="overrideIgnore">True if to allow the update operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> InlineUpdateAsync<TEntity>(object entity, IEnumerable<QueryField> where, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                InlineUpdate<TEntity>(entity: entity,
                    where: where,
                    overrideIgnore: overrideIgnore,
                    transaction: transaction));
        }

        /// <summary>
        /// Updates a data in the database by targetting certain fields only in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The dynamic data entity object that contains the targetted columns to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="overrideIgnore">True if to allow the update operation on the properties with <see cref="IgnoreAttribute"/> defined.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> InlineUpdateAsync<TEntity>(object entity, QueryGroup where, bool? overrideIgnore = false, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                InlineUpdate<TEntity>(entity: entity,
                    where: where,
                    overrideIgnore: overrideIgnore,
                    transaction: transaction));
        }

        // Insert

        /// <summary>
        /// Inserts a data in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The data entity object to be inserted.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>
        /// The value of the primary key of the newly inserted data entity object. Returns null if the 
        /// primary key property is not present.
        /// </returns>
        public object Insert<TEntity>(TEntity entity, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Insert<TEntity>(entity: entity,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // InsertAsync

        /// <summary>
        /// Inserts a data in the database in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The data entity object to be inserted.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>
        /// The value of the primary key of the newly inserted data entity object. Returns null if the 
        /// primary key property is not present.
        /// </returns>
        public Task<object> InsertAsync<TEntity>(TEntity entity, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Insert<TEntity>(entity: entity,
                    transaction: transaction));
        }

        // Merge

        /// <summary>
        /// Merges an existing data entity object in the database. By default, this operation uses the primary key property as
        /// the qualifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The entity to be merged.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Merge<TEntity>(TEntity entity, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Merge<TEntity>(entity: entity,
                qualifiers: null,
                    transaction: transaction);
        }

        /// <summary>
        /// Merges an existing data entity object in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The entity to be merged.</param>
        /// <param name="qualifier">
        /// The qualifer field to be used during merge operation. The qualifers are the fields used when qualifying the condition
        /// (equation of the fields) of the source and destination tables.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Merge<TEntity>(TEntity entity, Expression<Func<TEntity, object>> qualifier, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Merge<TEntity>(entity: entity,
                qualifier: qualifier,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Merges an existing data entity object in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The entity to be merged.</param>
        /// <param name="qualifier">
        /// The qualifer field to be used during merge operation. The qualifers are the fields used when qualifying the condition
        /// (equation of the fields) of the source and destination tables.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Merge<TEntity>(TEntity entity, Field qualifier, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Merge<TEntity>(entity: entity,
                qualifier: qualifier,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Merges an existing data entity object in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The entity to be merged.</param>
        /// <param name="qualifiers">
        /// The list of qualifer fields to be used during merge operation. The qualifers are the fields used when qualifying the condition
        /// (equation of the fields) of the source and destination tables.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Merge<TEntity>(TEntity entity, IEnumerable<Field> qualifiers, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Merge<TEntity>(entity: entity,
                qualifiers: qualifiers,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // MergeAsync

        /// <summary>
        /// Merges an existing data entity object in the database in an asynchronous way. By default, this operation uses the primary key property as
        /// the qualifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The entity to be merged.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> MergeAsync<TEntity>(TEntity entity, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Merge<TEntity>(entity: entity,
                    transaction: transaction));
        }

        /// <summary>
        /// Merges an existing data entity object in the database in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The entity to be merged.</param>
        /// <param name="qualifier">
        /// The qualifer field to be used during merge operation. The qualifers are the fields used when qualifying the condition
        /// (equation of the fields) of the source and destination tables.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> MergeAsync<TEntity>(TEntity entity, Expression<Func<TEntity, object>> qualifier, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Merge<TEntity>(entity: entity,
                    qualifier: qualifier,
                    transaction: transaction));
        }

        /// <summary>
        /// Merges an existing data entity object in the database in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The entity to be merged.</param>
        /// <param name="qualifier">
        /// The qualifer field to be used during merge operation. The qualifers are the fields used when qualifying the condition
        /// (equation of the fields) of the source and destination tables.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> MergeAsync<TEntity>(TEntity entity, Field qualifier, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Merge<TEntity>(entity: entity,
                    qualifier: qualifier,
                    transaction: transaction));
        }

        /// <summary>
        /// Merges an existing data entity object in the database in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The entity to be merged.</param>
        /// <param name="qualifiers">
        /// The list of qualifer fields to be used during merge operation. The qualifers are the fields used when qualifying the condition
        /// (equation of the fields) of the source and destination tables.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> MergeAsync<TEntity>(TEntity entity, IEnumerable<Field> qualifiers, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Merge<TEntity>(entity: entity,
                    qualifiers: qualifiers,
                    transaction: transaction));
        }

        // Query

        /// <summary>
        /// Query a data from the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="top">The top number of rows to be used by this operation.</param>
        /// <param name="cacheKey">
        /// The key to the cache. If the cache key is present in the cache, then the item from the cache will be returned instead. Setting this
        /// to null would force the repository to query from the database.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <param name="recursive">
        /// The value that indicates whether the child data entity objects defined in the target data entity object will
        /// be included in the result of the query. The default value is false.
        /// </param>
        /// <param name="recursionDepth">
        /// Defines the depth of the recursion when querying the data from the database. By default, the value is null to enable the querying of all 
        /// child data entities defined on the targetted data entity. Maximum recursion of 15 cycles only to avoid cyclomatic overflow operation.
        /// </param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public IEnumerable<TEntity> Query<TEntity>(IEnumerable<OrderField> orderBy = null, int? top = 0, string cacheKey = null,
            IDbTransaction transaction = null, bool? recursive = false, int? recursionDepth = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Query<TEntity>(orderBy: orderBy,
                top: top,
                cacheKey: cacheKey,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                cache: Cache,
                trace: Trace,
                statementBuilder: StatementBuilder,
                recursive: recursive,
                recursionDepth: recursionDepth);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Query a data from the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="top">The top number of rows to be used by this operation.</param>
        /// <param name="cacheKey">
        /// The key to the cache. If the cache key is present in the cache, then the item from the cache will be returned instead. Setting this
        /// to null would force the repository to query from the database.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <param name="recursive">
        /// The value that indicates whether the child data entity objects defined in the target data entity object will
        /// be included in the result of the query. The default value is false.
        /// </param>
        /// <param name="recursionDepth">
        /// Defines the depth of the recursion when querying the data from the database. By default, the value is null to enable the querying of all 
        /// child data entities defined on the targetted data entity. Maximum recursion of 15 cycles only to avoid cyclomatic overflow operation.
        /// </param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public IEnumerable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>> where, IEnumerable<OrderField> orderBy = null, int? top = 0,
            string cacheKey = null, IDbTransaction transaction = null, bool? recursive = false, int? recursionDepth = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Query<TEntity>(where: where,
                orderBy: orderBy,
                top: top,
                cacheKey: cacheKey,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                cache: Cache,
                trace: Trace,
                statementBuilder: StatementBuilder,
                recursive: recursive,
                recursionDepth: recursionDepth);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Query a data from the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="top">The top number of rows to be used by this operation.</param>
        /// <param name="cacheKey">
        /// The key to the cache. If the cache key is present in the cache, then the item from the cache will be returned instead. Setting this
        /// to null would force the repository to query from the database.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <param name="recursive">
        /// The value that indicates whether the child data entity objects defined in the target data entity object will
        /// be included in the result of the query. The default value is false.
        /// </param>
        /// <param name="recursionDepth">
        /// Defines the depth of the recursion when querying the data from the database. By default, the value is null to enable the querying of all 
        /// child data entities defined on the targetted data entity. Maximum recursion of 15 cycles only to avoid cyclomatic overflow operation.
        /// </param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public IEnumerable<TEntity> Query<TEntity>(QueryField where, IEnumerable<OrderField> orderBy = null, int? top = 0,
            string cacheKey = null, IDbTransaction transaction = null, bool? recursive = false, int? recursionDepth = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Query<TEntity>(where: where,
                orderBy: orderBy,
                top: top,
                cacheKey: cacheKey,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                cache: Cache,
                trace: Trace,
                statementBuilder: StatementBuilder,
                recursive: recursive,
                recursionDepth: recursionDepth);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Query a data from the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="top">The top number of rows to be used by this operation.</param>
        /// <param name="cacheKey">
        /// The key to the cache. If the cache key is present in the cache, then the item from the cache will be returned instead. Setting this
        /// to null would force the repository to query from the database.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <param name="recursive">
        /// The value that indicates whether the child data entity objects defined in the target data entity object will
        /// be included in the result of the query. The default value is false.
        /// </param>
        /// <param name="recursionDepth">
        /// Defines the depth of the recursion when querying the data from the database. By default, the value is null to enable the querying of all 
        /// child data entities defined on the targetted data entity. Maximum recursion of 15 cycles only to avoid cyclomatic overflow operation.
        /// </param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public IEnumerable<TEntity> Query<TEntity>(IEnumerable<QueryField> where, IEnumerable<OrderField> orderBy = null, int? top = 0,
            string cacheKey = null, IDbTransaction transaction = null, bool? recursive = false, int? recursionDepth = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Query<TEntity>(where: where,
                orderBy: orderBy,
                top: top,
                cacheKey: cacheKey,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                cache: Cache,
                trace: Trace,
                statementBuilder: StatementBuilder,
                recursive: recursive,
                recursionDepth: recursionDepth);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Query a data from the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="whereOrWhat">The query expression or primary key value to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="top">The top number of rows to be used by this operation.</param>
        /// <param name="cacheKey">
        /// The key to the cache. If the cache key is present in the cache, then the item from the cache will be returned instead. Setting this
        /// to null would force the repository to query from the database.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <param name="recursive">
        /// The value that indicates whether the child data entity objects defined in the target data entity object will
        /// be included in the result of the query. The default value is false.
        /// </param>
        /// <param name="recursionDepth">
        /// Defines the depth of the recursion when querying the data from the database. By default, the value is null to enable the querying of all 
        /// child data entities defined on the targetted data entity. Maximum recursion of 15 cycles only to avoid cyclomatic overflow operation.
        /// </param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public IEnumerable<TEntity> Query<TEntity>(object whereOrWhat, IEnumerable<OrderField> orderBy = null, int? top = 0,
            string cacheKey = null, IDbTransaction transaction = null, bool? recursive = false, int? recursionDepth = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Query<TEntity>(whereOrWhat: whereOrWhat,
                orderBy: orderBy,
                top: top,
                cacheKey: cacheKey,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                cache: Cache,
                trace: Trace,
                statementBuilder: StatementBuilder,
                recursive: recursive,
                recursionDepth: recursionDepth);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Query a data from the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="top">The top number of rows to be used by this operation.</param>
        /// <param name="cacheKey">
        /// The key to the cache. If the cache key is present in the cache, then the item from the cache will be returned instead. Setting this
        /// to null would force the repository to query from the database.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <param name="recursive">
        /// The value that indicates whether the child data entity objects defined in the target data entity object will
        /// be included in the result of the query. The default value is false.
        /// </param>
        /// <param name="recursionDepth">
        /// Defines the depth of the recursion when querying the data from the database. By default, the value is null to enable the querying of all 
        /// child data entities defined on the targetted data entity. Maximum recursion of 15 cycles only to avoid cyclomatic overflow operation.
        /// </param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public IEnumerable<TEntity> Query<TEntity>(QueryGroup where, IEnumerable<OrderField> orderBy = null, int? top = 0,
            string cacheKey = null, IDbTransaction transaction = null, bool? recursive = false, int? recursionDepth = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Query<TEntity>(where: where,
                orderBy: orderBy,
                top: top,
                cacheKey: cacheKey,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                cache: Cache,
                trace: Trace,
                statementBuilder: StatementBuilder,
                recursive: recursive,
                recursionDepth: recursionDepth);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // QueryAsync

        /// <summary>
        /// Query a data from the database in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="top">The top number of rows to be used by this operation.</param>
        /// <param name="cacheKey">
        /// The key to the cache. If the cache key is present in the cache, then the item from the cache will be returned instead. Setting this
        /// to null would force the repository to query from the database.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <param name="recursive">
        /// The value that indicates whether the child data entity objects defined in the target data entity object will
        /// be included in the result of the query. The default value is false.
        /// </param>
        /// <param name="recursionDepth">
        /// Defines the depth of the recursion when querying the data from the database. By default, the value is null to enable the querying of all 
        /// child data entities defined on the targetted data entity. Maximum recursion of 15 cycles only to avoid cyclomatic overflow operation.
        /// </param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public Task<IEnumerable<TEntity>> QueryAsync<TEntity>(IEnumerable<OrderField> orderBy = null, int? top = 0,
            string cacheKey = null, IDbTransaction transaction = null, bool? recursive = false, int? recursionDepth = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Query<TEntity>(orderBy: orderBy,
                    top: top,
                    cacheKey: cacheKey,
                    transaction: transaction,
                    recursive: recursive,
                    recursionDepth: recursionDepth));
        }

        /// <summary>
        /// Query a data from the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="top">The top number of rows to be used by this operation.</param>
        /// <param name="cacheKey">
        /// The key to the cache. If the cache key is present in the cache, then the item from the cache will be returned instead. Setting this
        /// to null would force the repository to query from the database.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <param name="recursive">
        /// The value that indicates whether the child data entity objects defined in the target data entity object will
        /// be included in the result of the query. The default value is false.
        /// </param>
        /// <param name="recursionDepth">
        /// Defines the depth of the recursion when querying the data from the database. By default, the value is null to enable the querying of all 
        /// child data entities defined on the targetted data entity. Maximum recursion of 15 cycles only to avoid cyclomatic overflow operation.
        /// </param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public Task<IEnumerable<TEntity>> QueryAsync<TEntity>(Expression<Func<TEntity, bool>> where, IEnumerable<OrderField> orderBy = null, int? top = 0,
            string cacheKey = null, IDbTransaction transaction = null, bool? recursive = false, int? recursionDepth = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Query<TEntity>(where: where,
                    orderBy: orderBy,
                    top: top,
                    cacheKey: cacheKey,
                    transaction: transaction,
                    recursive: recursive,
                    recursionDepth: recursionDepth));
        }

        /// <summary>
        /// Query a data from the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="top">The top number of rows to be used by this operation.</param>
        /// <param name="cacheKey">
        /// The key to the cache. If the cache key is present in the cache, then the item from the cache will be returned instead. Setting this
        /// to null would force the repository to query from the database.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <param name="recursive">
        /// The value that indicates whether the child data entity objects defined in the target data entity object will
        /// be included in the result of the query. The default value is false.
        /// </param>
        /// <param name="recursionDepth">
        /// Defines the depth of the recursion when querying the data from the database. By default, the value is null to enable the querying of all 
        /// child data entities defined on the targetted data entity. Maximum recursion of 15 cycles only to avoid cyclomatic overflow operation.
        /// </param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public Task<IEnumerable<TEntity>> QueryAsync<TEntity>(QueryField where, IEnumerable<OrderField> orderBy = null, int? top = 0,
            string cacheKey = null, IDbTransaction transaction = null, bool? recursive = false, int? recursionDepth = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Query<TEntity>(where: where,
                    orderBy: orderBy,
                    top: top,
                    cacheKey: cacheKey,
                    transaction: transaction,
                    recursive: recursive,
                    recursionDepth: recursionDepth));
        }

        /// <summary>
        /// Query a data from the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="top">The top number of rows to be used by this operation.</param>
        /// <param name="cacheKey">
        /// The key to the cache. If the cache key is present in the cache, then the item from the cache will be returned instead. Setting this
        /// to null would force the repository to query from the database.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <param name="recursive">
        /// The value that indicates whether the child data entity objects defined in the target data entity object will
        /// be included in the result of the query. The default value is false.
        /// </param>
        /// <param name="recursionDepth">
        /// Defines the depth of the recursion when querying the data from the database. By default, the value is null to enable the querying of all 
        /// child data entities defined on the targetted data entity. Maximum recursion of 15 cycles only to avoid cyclomatic overflow operation.
        /// </param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public Task<IEnumerable<TEntity>> QueryAsync<TEntity>(IEnumerable<QueryField> where, IEnumerable<OrderField> orderBy = null, int? top = 0,
            string cacheKey = null, IDbTransaction transaction = null, bool? recursive = false, int? recursionDepth = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Query<TEntity>(where: where,
                    orderBy: orderBy,
                    top: top,
                    cacheKey: cacheKey,
                    transaction: transaction,
                    recursive: recursive,
                    recursionDepth: recursionDepth));
        }

        /// <summary>
        /// Query a data from the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="whereOrWhat">The query expression or primary key value to be used by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="top">The top number of rows to be used by this operation.</param>
        /// <param name="cacheKey">
        /// The key to the cache. If the cache key is present in the cache, then the item from the cache will be returned instead. Setting this
        /// to null would force the repository to query from the database.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <param name="recursive">
        /// The value that indicates whether the child data entity objects defined in the target data entity object will
        /// be included in the result of the query. The default value is false.
        /// </param>
        /// <param name="recursionDepth">
        /// Defines the depth of the recursion when querying the data from the database. By default, the value is null to enable the querying of all 
        /// child data entities defined on the targetted data entity. Maximum recursion of 15 cycles only to avoid cyclomatic overflow operation.
        /// </param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public Task<IEnumerable<TEntity>> QueryAsync<TEntity>(object whereOrWhat, IEnumerable<OrderField> orderBy = null, int? top = 0,
            string cacheKey = null, IDbTransaction transaction = null, bool? recursive = false, int? recursionDepth = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Query<TEntity>(whereOrWhat: whereOrWhat,
                    orderBy: orderBy,
                    top: top,
                    cacheKey: cacheKey,
                    transaction: transaction,
                    recursive: recursive,
                    recursionDepth: recursionDepth));
        }

        /// <summary>
        /// Query a data from the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="orderBy">The order definition of the fields to be used by this operation.</param>
        /// <param name="top">The top number of rows to be used by this operation.</param>
        /// <param name="cacheKey">
        /// The key to the cache. If the cache key is present in the cache, then the item from the cache will be returned instead. Setting this
        /// to null would force the repository to query from the database.
        /// </param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <param name="recursive">
        /// The value that indicates whether the child data entity objects defined in the target data entity object will
        /// be included in the result of the query. The default value is false.
        /// </param>
        /// <param name="recursionDepth">
        /// Defines the depth of the recursion when querying the data from the database. By default, the value is null to enable the querying of all 
        /// child data entities defined on the targetted data entity. Maximum recursion of 15 cycles only to avoid cyclomatic overflow operation.
        /// </param>
        /// <returns>An enumerable list of An enumerable list of data entity object.</returns>
        public Task<IEnumerable<TEntity>> QueryAsync<TEntity>(QueryGroup where, IEnumerable<OrderField> orderBy = null, int? top = 0,
            string cacheKey = null, IDbTransaction transaction = null, bool? recursive = false, int? recursionDepth = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Query<TEntity>(where: where,
                    orderBy: orderBy,
                    top: top,
                    cacheKey: cacheKey,
                    transaction: transaction,
                    recursive: recursive,
                    recursionDepth: recursionDepth));
        }

        // Truncate

        /// <summary>
        /// Truncates a table from the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        public void Truncate<TEntity>()
            where TEntity : class
        {
            // Create a connection
            var connection = CreateConnection();

            // Call the method
            connection.Truncate<TEntity>(commandTimeout: CommandTimeout,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }
        }

        // TruncateAsync

        /// <summary>
        /// Truncates a table from the database in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        public Task TruncateAsync<TEntity>()
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Truncate<TEntity>());
        }

        // Update

        /// <summary>
        /// Updates a data in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The instance of data entity object to be updated.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Update<TEntity>(TEntity entity, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Update<TEntity>(entity: entity,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Updates a data in the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The instance of data entity object to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Update<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> where, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Update<TEntity>(entity: entity,
                where: where,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Updates a data in the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The instance of data entity object to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Update<TEntity>(TEntity entity, QueryField where, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Update<TEntity>(entity: entity,
                where: where,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Updates a data in the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The instance of data entity object to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Update<TEntity>(TEntity entity, IEnumerable<QueryField> where, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Update<TEntity>(entity: entity,
                where: where,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Updates a data in the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The instance of data entity object to be updated.</param>
        /// <param name="whereOrWhat">The query expression or primary key value to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Update<TEntity>(TEntity entity, object whereOrWhat, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Update<TEntity>(entity: entity,
                whereOrWhat: whereOrWhat,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Updates a data in the database based on the given query expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The instance of data entity object to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int Update<TEntity>(TEntity entity, QueryGroup where, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.Update<TEntity>(entity: entity,
                where: where,
                commandTimeout: CommandTimeout,
                transaction: transaction,
                trace: Trace,
                statementBuilder: StatementBuilder);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // UpdateAsync

        /// <summary>
        /// Updates a data in the database in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The instance of data entity object to be updated.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> UpdateAsync<TEntity>(TEntity entity, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Update<TEntity>(entity: entity,
                    transaction: transaction));
        }

        /// <summary>
        /// Updates a data in the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The instance of data entity object to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> UpdateAsync<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> where, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Update<TEntity>(entity: entity,
                    where: where,
                    transaction: transaction));
        }

        /// <summary>
        /// Updates a data in the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The instance of data entity object to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> UpdateAsync<TEntity>(TEntity entity, QueryField where, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Update<TEntity>(entity: entity,
                    where: where,
                    transaction: transaction));
        }

        /// <summary>
        /// Updates a data in the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The instance of data entity object to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> UpdateAsync<TEntity>(TEntity entity, IEnumerable<QueryField> where, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Update<TEntity>(entity: entity,
                    where: where,
                    transaction: transaction));
        }

        /// <summary>
        /// Updates a data in the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The instance of data entity object to be updated.</param>
        /// <param name="whereOrWhat">The query expression or primary key value to be used by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> UpdateAsync<TEntity>(TEntity entity, object whereOrWhat, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Update<TEntity>(entity: entity,
                    whereOrWhat: whereOrWhat,
                    transaction: transaction));
        }

        /// <summary>
        /// Updates a data in the database based on the given query expression in an asynchronous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="entity">The instance of data entity object to be updated.</param>
        /// <param name="where">The query expression to be used  by this operation.</param>
        /// <param name="transaction">The transaction to be used by this operation.</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> UpdateAsync<TEntity>(TEntity entity, QueryGroup where, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                Update<TEntity>(entity: entity,
                    where: where,
                    transaction: transaction));
        }

        #endregion

        #region ExecuteMethods

        // ExecuteQuery

        /// <summary>
        /// Executes a query from the database. It uses the underlying method <see cref="IDbCommand.ExecuteReader(CommandBehavior)"/> and
        /// converts the result back to an enumerable list of data entity object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="commandText">The command text to be used on the execution.</param>
        /// <param name="param">
        /// The dynamic object to be used as parameter. This object must contain all the values for all the parameters
        /// defined in the <see cref="IDbCommand.CommandText"/> property.
        /// </param>
        /// <param name="commandType">The command type to be used on the execution.</param>
        /// <param name="transaction">The transaction to be used on the execution (if present).</param>
        /// <returns>
        /// An enumerable list of data entity object containing the converted results of the underlying <see cref="IDataReader"/> object.
        /// </returns>
        public IEnumerable<TEntity> ExecuteQuery<TEntity>(string commandText, object param = null, CommandType? commandType = null, IDbTransaction transaction = null)
            where TEntity : class
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.ExecuteQuery<TEntity>(commandText: commandText,
                param: param,
                commandType: commandType,
                commandTimeout: CommandTimeout,
                transaction: transaction);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // ExecuteQueryAsync

        /// <summary>
        /// Executes a query from the database in an asynchronous way. It uses the underlying method <see cref="IDbCommand.ExecuteReader(CommandBehavior)"/> and 
        /// converts the result back to an enumerable list of data entity object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity object.</typeparam>
        /// <param name="commandText">The command text to be used on the execution.</param>
        /// <param name="param">
        /// The dynamic object to be used as parameter. This object must contain all the values for all the parameters
        /// defined in the <see cref="IDbCommand.CommandText"/> property.
        /// </param>
        /// <param name="commandType">The command type to be used on the execution.</param>
        /// <param name="transaction">The transaction to be used on the execution (if present).</param>
        /// <returns>
        /// An enumerable list of data entity object containing the converted results of the underlying <see cref="IDataReader"/> object.
        /// </returns>
        public Task<IEnumerable<TEntity>> ExecuteQueryAsync<TEntity>(string commandText, object param = null, CommandType? commandType = null, IDbTransaction transaction = null)
            where TEntity : class
        {
            return Task.Factory.StartNew(() =>
                ExecuteQuery<TEntity>(commandText: commandText,
                    param: param,
                    commandType: commandType,
                    transaction: transaction));
        }

        // ExecuteNonQuery

        /// <summary>
        /// Executes a query from the database. It uses the underlying method <see cref="IDbCommand.ExecuteNonQuery"/> and
        /// returns the number of affected rows during the execution.
        /// </summary>
        /// <param name="commandText">The command text to be used on the execution.</param>
        /// <param name="param">
        /// The dynamic object to be used as parameter. This object must contain all the values for all the parameters
        /// defined in the <see cref="IDbCommand.CommandText"/> property.
        /// </param>
        /// <param name="commandType">The command type to be used on the execution.</param>
        /// <param name="transaction">The transaction to be used on the execution (if present).</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public int ExecuteNonQuery(string commandText, object param = null, CommandType? commandType = null, IDbTransaction transaction = null)
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.ExecuteNonQuery(commandText: commandText,
                param: param,
                commandType: commandType,
                commandTimeout: CommandTimeout,
                transaction: transaction);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // ExecuteNonQueryAsync

        /// <summary>
        /// Executes a query from the database in an asynchronous way. It uses the underlying method <see cref="IDbCommand.ExecuteNonQuery"/> and
        /// returns the number of affected rows during the execution.
        /// </summary>
        /// <param name="commandText">The command text to be used on the execution.</param>
        /// <param name="param">
        /// The dynamic object to be used as parameter. This object must contain all the values for all the parameters
        /// defined in the <see cref="IDbCommand.CommandText"/> property.
        /// </param>
        /// <param name="commandType">The command type to be used on the execution.</param>
        /// <param name="transaction">The transaction to be used on the execution (if present).</param>
        /// <returns>An instance of integer that holds the number of rows affected by the execution.</returns>
        public Task<int> ExecuteNonQueryAsync(string commandText, object param = null, CommandType? commandType = null, IDbTransaction transaction = null)
        {
            return Task.Factory.StartNew<int>(() =>
                ExecuteNonQuery(commandText: commandText,
                    param: param,
                    commandType: commandType,
                    transaction: transaction));
        }

        // ExecuteScalar

        /// <summary>
        /// Executes a query from the database. It uses the underlying method <see cref="IDbCommand.ExecuteScalar"/> and
        /// returns the first occurence value (first column of first row) of the execution.
        /// </summary>
        /// <param name="commandText">The command text to be used on the execution.</param>
        /// <param name="param">
        /// The dynamic object to be used as parameter. This object must contain all the values for all the parameters
        /// defined in the <see cref="IDbCommand.CommandText"/> property.
        /// </param>
        /// <param name="commandType">The command type to be used on the execution.</param>
        /// <param name="transaction">The transaction to be used on the execution (if present).</param>
        /// <returns>An object that holds the first occurence value (first column of first row) of the execution.</returns>
        public object ExecuteScalar(string commandText, object param = null, CommandType? commandType = null, IDbTransaction transaction = null)
        {
            // Create a connection
            var connection = (transaction?.Connection ?? CreateConnection());

            // Call the method
            var result = connection.ExecuteScalar(commandText: commandText,
                param: param,
                commandType: commandType,
                commandTimeout: CommandTimeout,
                transaction: transaction);

            // Dispose the connection
            if (transaction == null && ConnectionPersistency == ConnectionPersistency.PerCall)
            {
                connection.Dispose();
            }

            // Return the result
            return result;
        }

        // ExecuteScalarAsync

        /// <summary>
        /// Executes a query from the database in an asynchronous way. It uses the underlying method <see cref="IDbCommand.ExecuteScalar"/> and
        /// returns the first occurence value (first column of first row) of the execution.
        /// </summary>
        /// <param name="commandText">The command text to be used on the execution.</param>
        /// <param name="param">
        /// The dynamic object to be used as parameter. This object must contain all the values for all the parameters
        /// defined in the <see cref="IDbCommand.CommandText"/> property.
        /// </param>
        /// <param name="commandType">The command type to be used on the execution.</param>
        /// <param name="transaction">The transaction to be used on the execution (if present).</param>
        /// <returns>An object that holds the first occurence value (first column of first row) of the execution.</returns>
        public Task<object> ExecuteScalarAsync(string commandText, object param = null, CommandType? commandType = null, IDbTransaction transaction = null)
        {
            return Task.Factory.StartNew<object>(() =>
                ExecuteScalar(commandText: commandText,
                    param: param,
                    commandType: commandType,
                    transaction: transaction));
        }

        #endregion
    }
}

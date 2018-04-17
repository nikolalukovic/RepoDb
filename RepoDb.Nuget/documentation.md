### Documentation
Click [here](https://github.com/mikependon/RepoDb/blob/master/RepoDb.Documents/documentation_v1.0.10.md) for the documentation. (in-progress)

### Goal

We aim to let .Net developers limit the implementation of SQL Statements within the application. We believe that as a .Net developer, one should only focus on .Net and Business scenario development and not on writing repetitive SQL Statements. Ofcourse, unless you are working closely with SQL Server Management Studio.

### Notes

 - We will keep it simple as possible (KISS principle)
 - We will make it fast as possible
 - We will never make complex implementations (specially for Queries and Methods)
 - We will avoid developing complex JOINs (until it is needed and requested by the community)
 - We will never ever do try-catch inside the library

### Features

 - BatchQuery/BatchQueryAsync
 - Query/QueryAsync
 - Insert/InsertAsync
 - Delete/DeleteAsync
 - Update/UpdateAsync
 - InlineUpdate/InlineUpdateAsync
 - Merge/MergeAsync
 - BulkInsert/BulkInsertAsync
 - ExecuteReader/ExecuteReaderAsync
 - ExecuteNonQuery/ExecuteNonQueryAsync
 - ExecuteScalar/ExecuteScalarAsync
 - Transaction
 - Asynchronous Operations
 - Type Mapping
 - Field Mapping
 - Expression Tree
 - Caching
 - Tracing
 - SQL Builder
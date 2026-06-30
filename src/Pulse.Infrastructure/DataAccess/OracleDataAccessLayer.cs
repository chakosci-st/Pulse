using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using Dapper;
using Dapper.Oracle;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Entities = Pulse.Core.Entities;
using log4net;
/// <summary>
/// A data access layer for interacting with Oracle databases using Dapper.
/// Provides methods for executing queries, stored procedures, and managing transactions.
/// </summary>
namespace Pulse.Infrastructure.DataAccess
{
    public class OracleDataAccessLayer : IDisposable
    {
        private readonly string _connectionString; // Connection string for the Oracle database.
        private IDbConnection _connection; // The database connection object. 
        private IDbTransaction _transaction; // The current database transaction. 
        private OracleDynamicParameters _parameters; // Parameters for stored procedures.
        private readonly ILog _logger;
        /// <summary>
        /// Maps .NET types to Oracle database types.
        /// </summary>
        /// <param name="type">The .NET type to map.</param>
        /// <returns>The corresponding DbType.</returns>
        private DbType GetDbType(Type type)
        {
            if (type == typeof(string))
                return DbType.String;
            if (type == typeof(int) || type == typeof(int?) || type == typeof(Int16) || type == typeof(Int16?) || type == typeof(Int32) || type == typeof(Int32?) || type == typeof(Int64) || type == typeof(Int64?))
                return DbType.Int32;
            if (type == typeof(Int64) || type == typeof(Int64?))
                return DbType.Int64;
            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return DbType.DateTime;
            if (type == typeof(bool) || type == typeof(bool?))
                return DbType.Boolean;
            if (type == typeof(decimal) || type == typeof(decimal?))
                return DbType.Decimal;
            if (type == typeof(double) || type == typeof(double?))
                return DbType.Double;
            if (type == typeof(float) || type == typeof(float?))
                return DbType.Single;
            if (type == typeof(Guid) || type == typeof(Guid?))
                return DbType.Guid;

            throw new ArgumentException($"Unsupported type: {type.FullName}");
        }

        private OracleMappingType GetOracleMappingType(Type type)
        {
            if (type == typeof(string))
                return OracleMappingType.Varchar2;
            if (type == typeof(int) || type == typeof(int?) || type == typeof(Int16) || type == typeof(Int16?) || type == typeof(Int32) || type == typeof(Int32?) || type == typeof(Int64) || type == typeof(Int64?))
                return OracleMappingType.Int32;
            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return OracleMappingType.Date;
            if (type == typeof(bool) || type == typeof(bool?))
                return OracleMappingType.Int16;
            if (type == typeof(decimal) || type == typeof(decimal?))
                return OracleMappingType.Decimal;
            if (type == typeof(double) || type == typeof(double?))
                return OracleMappingType.Double;
            if (type == typeof(float) || type == typeof(float?))
                return OracleMappingType.Single;
            if (type == typeof(Guid) || type == typeof(Guid?))
                return OracleMappingType.Varchar2;

            throw new ArgumentException($"Unsupported type: {type.FullName}");
        }

        #region "Initialize"
        /// <summary>
        /// Initializes a new instance of the OracleDataAccessLayer class.
        /// </summary>
        /// <param name="connection">The name of the connection string in the configuration file.</param>
        /// <param name="connectionString">The connection string to use (optional).</param>
        public OracleDataAccessLayer(string connection = "ORACONNECTION", string connectionString = "", ILog logger = null)
        {
            _logger = logger;
            _connectionString = !string.IsNullOrEmpty(connectionString) ? connectionString : GetConnectionString(connection);
            _connection = new OracleConnection(_connectionString);
            _connection.Open();
            _logger?.Info("OracleDataAccessLayer created. Connection opened.");
        }
        #endregion

        #region "Common Methods"
        /// <summary>
        /// Retrieves the connection string from the configuration file.
        /// </summary>
        /// <param name="connection">The name of the connection string.</param>
        /// <returns>The connection string.</returns>
        private string GetConnectionString(string connection)
        {
            return ConfigurationManager.ConnectionStrings[connection]?.ConnectionString ?? throw new InvalidOperationException("Connection string not found.");
        }

        /// <summary>
        /// Sets a new connection string and reinitializes the database connection.
        /// </summary>
        /// <param name="connectionString">The new connection string.</param>
        public void SetConnection(string connectionString)
        {
            if (_connection.State == ConnectionState.Open) _connection.Close();
            _connection = new OracleConnection(connectionString);
            _connection.Open();
        }

        /// <summary>
        /// Adds a parameter to the current set of parameters for a stored procedure.
        /// </summary>
        /// <param name="parametertype">The Oracle data type of the parameter.</param>
        /// <param name="parameterdirection">The direction of the parameter (Input/Output).</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parametervalue">The value of the parameter (optional).</param>
        /// <param name="parametersize">The size of the parameter (optional).</param>
        public void AddParameter(OracleMappingType parametertype, ParameterDirection parameterdirection, string name, object parametervalue = null, int? parametersize = null)
        {

            if (parametervalue == null && parameterdirection == ParameterDirection.Output)
                _parameters.Add(name, dbType: parametertype, direction: parameterdirection);
            else if (parametersize != null)
                _parameters.Add(name, parametervalue, parametertype, parameterdirection, size: parametersize);
            else
                _parameters.Add(name, parametervalue, parametertype, parameterdirection);
        }

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        public void BeginTransaction()
        {
            if (_transaction == null)
            {
                _transaction = _connection.BeginTransaction();
                _logger?.Info("Transaction started.");
            }
            else
            {
                _logger?.Warn("BeginTransaction called, but transaction already exists.");
            }
        }

        /// <summary>
        /// Commits the current database transaction.
        /// </summary>
        public void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _logger?.Info("Transaction committed.");
                _transaction = null;
            }
            else
            {
                _logger?.Warn("CommitTransaction called, but no transaction exists.");
            }
        }



        /// <summary>
        /// Rolls back the current database transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _logger?.Info("Transaction rolled back.");
                _transaction = null;
            }
            else
            {
                _logger?.Warn("RollbackTransaction called, but no transaction exists.");
            }
        }

        public IDbTransaction GetTransaction()
        {
            return _transaction;
        }

        #endregion

        #region "Query"
        /// <summary>
        /// Executes a query asynchronously and returns the result as a collection of objects.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the result set.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="parameters">The parameters for the query (optional).</param>
        /// <returns>A task representing the asynchronous operation, with a result of the query.</returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null)
        {
            return await _connection.QueryAsync<T>(sql, parameters, _transaction);
        }
        /// <summary>
        /// Executes a query and returns the result as a collection of objects.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the result set.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="parameters">The parameters for the query (optional).</param>
        /// <returns>A collection of objects resulting from the query.</returns>
        public IEnumerable<T> Query<T>(string sql, object parameters = null)
        {
            return _connection.Query<T>(sql, parameters, _transaction);
        }
        #endregion

        #region "Execute"
        /// <summary>
        /// Executes a command asynchronously and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="parameters">The parameters for the command (optional).</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected.</returns>
        public async Task<int> ExecuteAsync(string sql, object parameters = null)
        {
            return await _connection.ExecuteAsync(sql, parameters, _transaction);
        }

        /// <summary>
        /// Executes a command and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="parameters">The parameters for the command (optional).</param>
        /// <returns>The number of rows affected.</returns>
        public int Execute(string sql, object parameters = null)
        {
            return _connection.Execute(sql, parameters, _transaction);
        }
        #endregion

        #region "ExecuteScalar"
        /// <summary>
        /// Executes a command asynchronously and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="parameters">The parameters for the command (optional).</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected.</returns>
        public async Task<T> ExecuteScalarAsync<T>(string sql, object parameters = null)
        {
            return await _connection.ExecuteScalarAsync<T>(sql, parameters, _transaction);
        }

        /// <summary>
        /// Executes a command and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="parameters">The parameters for the command (optional).</param>
        /// <returns>The number of rows affected.</returns>
        public T ExecuteScalar<T>(string sql, object parameters = null)
        {
            return _connection.ExecuteScalar<T>(sql, parameters, _transaction);
        }
        #endregion

        #region "LoadData"
        /// <summary>
        /// Asynchronously loads data from a stored procedure and retrieves the total record count.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="storedProcedure">The name of the stored procedure to execute.</param>
        /// <param name="totalRecordsParameterName">The name of the output parameter for the total record count.</param>
        /// <returns>A tuple containing the list of data and the total record count.</returns>
        public async Task<(List<T> Data, int TotalRecords)> LoadStoredProcedureDataAsync<T>(string storedProcedure, string totalRecordsParameterName)
        {
            List<T> obj = new List<T>();
            int totalRecords = 0;

            using (var multi = await _connection.QueryMultipleAsync(storedProcedure, _parameters, commandType: CommandType.StoredProcedure))
            {
                obj = (await multi.ReadAsync<T>()).AsList();
                totalRecords = _parameters.Get<int>(totalRecordsParameterName);
            }

            return (obj, totalRecords);
        }

        /// <summary>
        /// Loads data from a stored procedure and retrieves the total record count.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="storedprocedure">The name of the stored procedure to execute.</param>
        /// <param name="totalRecordsParameterName">The name of the output parameter for the total record count.</param>
        /// <returns>A tuple containing the list of data and the total record count.</returns>
        public (List<T>, int totalRecords) LoadStoredProcedureData<T>(string storedprocedure, string totalRecordsParameterName)
        {
            List<T> obj = new List<T>();
            int totalRecords = 0;


            using (var multi = _connection.QueryMultiple(storedprocedure, _parameters, commandType: CommandType.StoredProcedure))
            {
                obj = multi.Read<T>().AsList();
                totalRecords = _parameters.Get<int>(totalRecordsParameterName);
            }

            return (obj, totalRecords);
        }

        /// <summary>
        /// Asynchronously loads paginated data from a stored procedure with search and sorting options.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="storedProcedure">The name of the stored procedure to execute.</param>
        /// <param name="pageIndex">The index of the page to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="searchTerm">The search term to filter the data.</param>
        /// <param name="sortColumn">The column to sort the data by.</param>
        /// <param name="sortDirection">The direction to sort the data (e.g., ASC or DESC).</param>
        /// <returns>A tuple containing the list of data and the total record count.</returns>
        public async Task<(List<T> Data, int TotalRecords)> LoadStoredProcedureDataAsync<T>(string storedProcedure, int pageIndex, int pageSize, string searchTerm, string sortColumn, string sortDirection)
        {
            List<T> obj = new List<T>();
            int totalRecords = 0;
            var prm = new OracleDynamicParameters();

            prm.Add("p_pageIndex", pageIndex, OracleMappingType.Int32, ParameterDirection.Input);
            prm.Add("p_pageSize", pageSize, OracleMappingType.Int32, ParameterDirection.Input);
            prm.Add("p_searchTerm", searchTerm, OracleMappingType.Varchar2, ParameterDirection.Input, size: 250);
            prm.Add("p_sortColumn", sortColumn, OracleMappingType.Varchar2, ParameterDirection.Input, size: 40);
            prm.Add("p_sortDirection", sortDirection, OracleMappingType.Varchar2, ParameterDirection.Input, size: 4);
            prm.Add("p_totalRecords", dbType: OracleMappingType.Int32, direction: ParameterDirection.Output);
            prm.Add("p_cursor", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);

            using (var multi = await _connection.QueryMultipleAsync(storedProcedure, prm, commandType: CommandType.StoredProcedure))
            {
                obj = (await multi.ReadAsync<T>()).AsList();
                totalRecords = prm.Get<int>("p_totalRecords");
            }

            return (obj, totalRecords);
        }

        /// <summary>
        /// Loads paginated data from a stored procedure with search and sorting options.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="storedprocedure">The name of the stored procedure to execute.</param>
        /// <param name="pageIndex">The index of the page to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="searchTerm">The search term to filter the data.</param>
        /// <param name="sortColumn">The column to sort the data by.</param>
        /// <param name="sortDirection">The direction to sort the data (e.g., ASC or DESC).</param>
        /// <param name="totalRecords">The total number of records (output parameter).</param>
        /// <returns>A list of data for the specified page.</returns>
        public List<T> LoadStoredProcedureData<T>(string storedprocedure, int pageIndex, int pageSize, string searchTerm, string sortColumn, string sortDirection, out int totalRecords)
        {
            List<T> obj = new List<T>();
            totalRecords = 0;
            var prm = new OracleDynamicParameters();

            prm.Add("p_pageIndex", pageIndex, OracleMappingType.Int32, ParameterDirection.Input);
            prm.Add("p_pageSize", pageSize, OracleMappingType.Int32, ParameterDirection.Input);
            prm.Add("p_searchTerm", searchTerm, OracleMappingType.Varchar2, ParameterDirection.Input, size: 250);
            prm.Add("p_sortColumn", sortColumn, OracleMappingType.Varchar2, ParameterDirection.Input, size: 40);
            prm.Add("p_sortDirection", sortDirection, OracleMappingType.Varchar2, ParameterDirection.Input, size: 4);
            prm.Add("p_totalRecords", dbType: OracleMappingType.Int32, direction: ParameterDirection.Output);
            prm.Add("p_cursor", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);

            using (var multi = _connection.QueryMultiple(storedprocedure, prm, commandType: CommandType.StoredProcedure))
            {
                obj = multi.Read<T>().AsList();
                totalRecords = prm.Get<int>("p_totalRecords");
            }

            return obj;
        }

        /// <summary>
        /// Asynchronously loads paginated data from a stored procedure with multiple search terms and sorting options.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="storedProcedure">The name of the stored procedure to execute.</param>
        /// <param name="pageIndex">The index of the page to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="searchTerm1">The first search term to filter the data.</param>
        /// <param name="searchTerm2">The second search term to filter the data.</param>
        /// <param name="searchTerm3">The third search term to filter the data.</param>
        /// <param name="sortColumn">The column to sort the data by.</param>
        /// <param name="sortDirection">The direction to sort the data (e.g., ASC or DESC).</param>
        /// <returns>A tuple containing the list of data and the total record count.</returns>
        public async Task<(List<T> Data, int TotalRecords)> LoadStoredProcedureDataAsync<T>(string storedProcedure, int pageIndex, int pageSize, string searchTerm1, string searchTerm2, string searchTerm3, string sortColumn, string sortDirection)
        {
            List<T> obj = new List<T>();
            int totalRecords = 0;
            var prm = new OracleDynamicParameters();

            prm.Add("p_pageIndex", pageIndex, OracleMappingType.Int32, ParameterDirection.Input);
            prm.Add("p_pageSize", pageSize, OracleMappingType.Int32, ParameterDirection.Input);
            prm.Add("p_searchTerm1", searchTerm1, OracleMappingType.Varchar2, ParameterDirection.Input, size: 250);
            prm.Add("p_searchTerm2", searchTerm2, OracleMappingType.Varchar2, ParameterDirection.Input, size: 250);
            prm.Add("p_searchTerm3", searchTerm3, OracleMappingType.Varchar2, ParameterDirection.Input, size: 250);
            prm.Add("p_sortColumn", sortColumn, OracleMappingType.Varchar2, ParameterDirection.Input, size: 40);
            prm.Add("p_sortDirection", sortDirection, OracleMappingType.Varchar2, ParameterDirection.Input, size: 4);
            prm.Add("p_totalRecords", dbType: OracleMappingType.Int32, direction: ParameterDirection.Output);
            prm.Add("p_cursor", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);

            using (var multi = await _connection.QueryMultipleAsync(storedProcedure, prm, commandType: CommandType.StoredProcedure))
            {
                obj = (await multi.ReadAsync<T>()).AsList();
                totalRecords = prm.Get<int>("p_totalRecords");
            }

            return (obj, totalRecords);
        }

        /// <summary>
        /// Loads paginated data from a stored procedure with multiple search terms and sorting options.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="storedProcedure">The name of the stored procedure to execute.</param>
        /// <param name="pageIndex">The index of the page to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="searchTerm1">The first search term to filter the data.</param>
        /// <param name="searchTerm2">The second search term to filter the data.</param>
        /// <param name="searchTerm3">The third search term to filter the data.</param>
        /// <param name="sortColumn">The column to sort the data by.</param>
        /// <param name="sortDirection">The direction to sort the data (e.g., ASC or DESC).</param>
        /// <returns>A tuple containing the list of data and the total record count.</returns>
        public (List<T>, int totalRecords) LoadStoredProcedureData<T>(string storedprocedure, int pageIndex, int pageSize, string searchTerm1, string searchTerm2, string searchTerm3, string sortColumn, string sortDirection)
        {
            List<T> obj = new List<T>();
            int totalRecords = 0;
            var prm = new OracleDynamicParameters();

            prm.Add("p_pageIndex", pageIndex, OracleMappingType.Int32, ParameterDirection.Input);
            prm.Add("p_pageSize", pageSize, OracleMappingType.Int32, ParameterDirection.Input);
            prm.Add("p_searchTerm1", searchTerm1, OracleMappingType.Varchar2, ParameterDirection.Input, size: 250);
            prm.Add("p_searchTerm2", searchTerm2, OracleMappingType.Varchar2, ParameterDirection.Input, size: 250);
            prm.Add("p_searchTerm3", searchTerm3, OracleMappingType.Varchar2, ParameterDirection.Input, size: 250);
            prm.Add("p_sortColumn", sortColumn, OracleMappingType.Varchar2, ParameterDirection.Input, size: 40);
            prm.Add("p_sortDirection", sortDirection, OracleMappingType.Varchar2, ParameterDirection.Input, size: 4);
            prm.Add("p_totalRecords", dbType: OracleMappingType.Int32, direction: ParameterDirection.Output);
            prm.Add("p_cursor", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);

            using (var multi = _connection.QueryMultiple(storedprocedure, prm, commandType: CommandType.StoredProcedure))
            {
                obj = multi.Read<T>().AsList();
                totalRecords = prm.Get<int>("p_totalRecords");
            }

            return (obj, totalRecords);
        }

        /// <summary>
        /// Executes a stored procedure with a cursor and retrieves the result as a collection of dynamic objects.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameters">The parameters for the stored procedure.</param>
        /// <returns>A task representing the asynchronous operation, with a result of the query as a collection of dynamic objects.</returns>
        public async Task<IEnumerable<dynamic>> ExecuteStoredProcedureWithCursorAsync(string procedureName, OracleDynamicParameters parameters)
        {
            parameters.Add("p_cursor", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);
            return await _connection.QueryAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
            //using (var multi = await _connection.QueryMultipleAsync(procedureName, parameters, commandType: CommandType.StoredProcedure))
            //{
            //    return multi.Read<dynamic>().AsList();
            //}
        }

        /// <summary>
        /// Loads data from a stored procedure and retrieves the total record count.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="storedprocedure">The name of the stored procedure to execute.</param>
        /// <param name="parameters">The parameters for the stored procedure.</param>
        /// <returns>A tuple containing the list of data and the total record count.</returns>
        public (List<T>, int totalRecords) LoadStoredProcedureData<T>(string storedprocedure, OracleDynamicParameters parameters)
        {
            List<T> obj = new List<T>();
            int totalRecords = 0;
            parameters.Add("p_totalrecords", dbType: OracleMappingType.Int32, direction: ParameterDirection.Output);
            parameters.Add("p_cursor", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);

            using (var multi = _connection.QueryMultiple(storedprocedure, parameters, commandType: CommandType.StoredProcedure))
            {
                obj = multi.Read<T>().AsList();
                totalRecords = parameters.Get<int>("p_totalrecords");
            }

            return (obj, totalRecords);
        }

        /// <summary>
        /// Loads data from a stored procedure and retrieves only the data without the total record count.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="storedprocedure">The name of the stored procedure to execute.</param>
        /// <param name="parameters">The parameters for the stored procedure.</param>
        /// <returns>A list of data retrieved from the stored procedure.</returns>
        public List<T> LoadStoredProcedureDataOnly<T>(string storedprocedure, OracleDynamicParameters parameters)
        {
            List<T> obj = new List<T>();
            parameters.Add("p_cursor", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);

            using (var multi = _connection.QueryMultiple(storedprocedure, parameters, commandType: CommandType.StoredProcedure))
            {
                obj = multi.Read<T>().AsList();
            }

            return obj;
        }


        /// <summary>
        /// Asynchronously loads data from a stored procedure and retrieves the total record count.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="storedprocedure">The name of the stored procedure to execute.</param>
        /// <param name="parameters">The parameters for the stored procedure.</param>
        /// <returns>A task representing the asynchronous operation, with a tuple containing the list of data and the total record count.</returns>
        public async Task<(List<T>, int totalRecords)> LoadStoredProcedureDataAsync<T>(string storedprocedure, OracleDynamicParameters parameters)
        {
            List<T> obj = new List<T>();
            int totalRecords = 0;


            using (var multi = await _connection.QueryMultipleAsync(storedprocedure, parameters, commandType: CommandType.StoredProcedure))
            {
                obj = multi.Read<T>().AsList();
                totalRecords = parameters.Get<int>("p_totalRecords");
            }

            return (obj, totalRecords);
        }



        /// <summary>
        /// Loads data from a SQL query.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <returns>A list of data retrieved from the query.</returns>
        public List<T> LoadData<T>(string sql)
        {
            return _connection.Query<T>(sql).ToList();
        }

        /// <summary>
        /// Loads data from a SQL query.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <returns>A list of data retrieved from the query.</returns>
        public async Task<List<T>> LoadDataAsync<T>(string sql)
        {
            return (await _connection.QueryAsync<T>(sql)).ToList();
        }

        /// <summary>
        /// Asynchronously loads data from a SQL query with parameters.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="data">The parameters for the query.</param>
        /// <returns>A task representing the asynchronous operation, with a list of data retrieved from the query.</returns>
        public async Task<List<T>> LoadDataAsync<T>(string sql, T data)
        {
            return (await _connection.QueryAsync<T>(sql, data)).ToList();
        }

        /// <summary>
        /// Loads data from a SQL query with parameters.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="data">The parameters for the query.</param>
        /// <returns>A list of data retrieved from the query.</returns>
        public List<T> LoadData<T>(string sql, T data)
        {
            return _connection.Query<T>(sql, data).ToList();
        }

        /// <summary>
        /// Asynchronously loads data from a SQL query with dynamic parameters.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="param">The dynamic parameters for the query.</param>
        /// <returns>A task representing the asynchronous operation, with a list of data retrieved from the query.</returns>
        public async Task<List<T>> LoadDataAsync<T>(string sql, DynamicParameters param)
        {
            return (await _connection.QueryAsync<T>(sql, param)).ToList();
        }

        /// <summary>
        /// Loads data from a SQL query with dynamic parameters.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="param">The dynamic parameters for the query.</param>
        /// <returns>A list of data retrieved from the query.</returns>
        public List<T> LoadData<T>(string sql, DynamicParameters param)
        {
            return _connection.Query<T>(sql, param).ToList();
        }
        #endregion

        #region "FindData"
        /// <summary>
        /// Asynchronously retrieves a single record from the database using a SQL query.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <returns>A task representing the asynchronous operation, with the single record retrieved from the query, or null if no record is found.</returns>
        public async Task<T> FindDataAsync<T>(string sql)
        {
            return (await _connection.QueryAsync<T>(sql)).SingleOrDefault();
        }

        /// <summary>
        /// Retrieves a single record from the database using a SQL query.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <returns>The single record retrieved from the query, or null if no record is found.</returns>
        public T FindData<T>(string sql)
        {
            return _connection.Query<T>(sql).SingleOrDefault();
        }

        /// <summary>
        /// Asynchronously retrieves a single record from the database using a SQL query and parameters.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="data">The parameters for the query.</param>
        /// <returns>A task representing the asynchronous operation, with the single record retrieved from the query, or null if no record is found.</returns>
        public async Task<T> FindDataAsync<T>(string sql, T data)
        {
            return (await _connection.QueryAsync<T>(sql, data)).ToList().FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a single record from the database using a SQL query and parameters.
        /// </summary>
        /// <typeparam name="T">The type of the data to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="data">The parameters for the query.</param>
        /// <returns>The single record retrieved from the query, or null if no record is found.</returns>
        public T FindData<T>(string sql, T data)
        {
            return _connection.Query<T>(sql, data).ToList().FirstOrDefault();
        }
        #endregion

        #region "QuerySingle"
        /// <summary>
        /// Asynchronously executes a SQL query and retrieves a single result.
        /// </summary>
        /// <typeparam name="T">The type of the result to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <returns>A task representing the asynchronous operation, with the single result of the query.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query does not return exactly one result.</exception>
        public async Task<T> QuerySingleAsync<T>(string sql)
        {
            return await _connection.QuerySingleAsync<T>(sql);
        }

        /// <summary>
        /// Executes a SQL query and retrieves a single result.
        /// </summary>
        /// <typeparam name="T">The type of the result to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <returns>The single result of the query.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query does not return exactly one result.</exception>
        public T QuerySingle<T>(string sql)
        {
            return _connection.QuerySingle<T>(sql);
        }

        /// <summary>
        /// Asynchronously executes a SQL query with parameters and retrieves a single result.
        /// </summary>
        /// <typeparam name="T">The type of the result to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="param">The parameters for the query.</param>
        /// <returns>A task representing the asynchronous operation, with the single result of the query.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query does not return exactly one result.</exception>
        public async Task<T> QuerySingleAsync<T>(string sql, DynamicParameters param)
        {
            return await _connection.QuerySingleAsync<T>(sql, param);
        }

        /// <summary>
        /// Executes a SQL query with parameters and retrieves a single result.
        /// </summary>
        /// <typeparam name="T">The type of the result to retrieve.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="param">The parameters for the query.</param>
        /// <returns>The single result of the query.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query does not return exactly one result.</exception>
        public T QuerySingle<T>(string sql, DynamicParameters param)
        {
            return _connection.QuerySingle<T>(sql, param);
        }
        #endregion

        #region "SaveData"
        /// <summary>
        /// Asynchronously saves data to the database using a SQL query and a data object.
        /// </summary>
        /// <typeparam name="T">The type of the data object to save.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="data">The data object containing the values to save.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected.</returns>
        public async Task<int> SaveDataAsync<T>(string sql, T data)
        {
            return await _connection.ExecuteAsync(sql, data, _transaction);
        }

        /// <summary>
        /// Saves data to the database using a SQL query and a data object.
        /// </summary>
        /// <typeparam name="T">The type of the data object to save.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="data">The data object containing the values to save.</param>
        /// <returns>The number of rows affected.</returns>
        public int SaveData<T>(string sql, T data)
        {
            return _connection.Execute(sql, data, _transaction);
        }

        /// <summary>
        /// Asynchronously saves data to the database using a SQL query, a data object, and CLOB fields.
        /// Handles large string data (CLOB) by setting appropriate sizes for string parameters.
        /// </summary>
        /// <typeparam name="T">The type of the data object to save.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="data">The data object containing the values to save.</param>
        /// <param name="clobfields">A comma-separated list of fields that should be treated as CLOBs.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected.</returns>
        public async Task<int> SaveDataAsync<T>(string sql, T data, string clobfields)
        {
            var parameters = new OracleDynamicParameters();

            // Reflect over the properties of the data object
            foreach (var prop in typeof(T).GetProperties())
            {
                try
                {
                    var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    if (propertyType != typeof(string) && !propertyType.IsValueType)
                    {
                        continue;
                    }

                    var value = prop.GetValue(data);
                    bool isClob = IsClobField(clobfields, prop.Name);

                    if (isClob && propertyType == typeof(string))
                    {
                        parameters.Add(prop.Name, value, OracleMappingType.Clob, ParameterDirection.Input);
                    }
                    else
                    {
                        var oracleType = GetOracleMappingType(propertyType);
                        parameters.Add(prop.Name, value, oracleType, ParameterDirection.Input);
                    }
                }
                catch
                {

                }

            }

            return await _connection.ExecuteAsync(sql, parameters, _transaction);
        }

        /// <summary>
        /// Saves data to the database using a SQL query, a data object, and CLOB fields.
        /// Handles large string data (CLOB) by setting appropriate sizes for string parameters.
        /// </summary>
        /// <typeparam name="T">The type of the data object to save.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="data">The data object containing the values to save.</param>
        /// <param name="clobfields">A comma-separated list of fields that should be treated as CLOBs.</param>
        /// <returns>The number of rows affected.</returns>
        public int SaveData<T>(string sql, T data, string clobfields)
        {
            var parameters = new OracleDynamicParameters();

            // Reflect over the properties of the data object
            foreach (var prop in typeof(T).GetProperties())
            {
                try
                {
                    var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    if (propertyType != typeof(string) && !propertyType.IsValueType)
                    {
                        continue;
                    }

                    var value = prop.GetValue(data);
                    bool isClob = IsClobField(clobfields, prop.Name);

                    if (isClob && propertyType == typeof(string))
                    {
                        parameters.Add(prop.Name, value, OracleMappingType.Clob, ParameterDirection.Input);
                    }
                    else
                    {
                        var oracleType = GetOracleMappingType(propertyType);
                        parameters.Add(prop.Name, value, oracleType, ParameterDirection.Input);
                    }
                }
                catch
                {

                }

            }




            ////// Reflect over the properties of the data object
            ////foreach (var prop in typeof(T).GetProperties())
            ////{
            ////    var value = prop.GetValue(data);
            ////    var dbType = DbType.String;
            ////    var size = -1;

            ////    // Check if the property is the CLOB data
            ////    if (prop.PropertyType == typeof(string) && clobfields.IndexOf(prop.Name) > 0)
            ////    {
            ////        dbType = DbType.String;
            ////        size = -1; // Set size to -1 for CLOB data
            ////    }

            ////    parameters.Add(prop.Name, value, dbType, size: size);
            ////}

            return _connection.Execute(sql, parameters, _transaction);


            //return _connection.Execute(sql, data);
        }
        #endregion

        #region "SaveDataReturnId"
        /// <summary>
        /// Asynchronously saves data to the database using a SQL query and returns the value of an output parameter (e.g., the generated ID).
        /// </summary>
        /// <typeparam name="T">The type of the data object to save.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="data">The data object containing the values to save.</param>
        /// <param name="parametername">The name of the output parameter to retrieve (default is "Id").</param>
        /// <returns>A task representing the asynchronous operation, with the value of the output parameter.</returns>
        public async Task<int> SaveDataReturnIdAsync<T>(string sql, T data, string parametername = "Id")
        {
            var param = new DynamicParameters(data);
            param.Add(name: parametername, dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _connection.ExecuteAsync(sql, param, _transaction);

            return param.Get<int>(parametername);
        }
        /// <summary>
        /// Saves data to the database using a SQL query and returns the value of an output parameter (e.g., the generated ID).
        /// </summary>
        /// <typeparam name="T">The type of the data object to save.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="data">The data object containing the values to save.</param>
        /// <param name="parametername">The name of the output parameter to retrieve (default is "Id").</param>
        /// <returns>The value of the output parameter.</returns>
        public int SaveDataReturnId<T>(string sql, T data, string parametername = "Id")
        {
            var param = new DynamicParameters(data);

            param.Add(name: parametername, dbType: DbType.Int32, direction: ParameterDirection.Output);

            _connection.Execute(sql, param, _transaction);

            return param.Get<int>(parametername);
        }


        #endregion

        #region "SaveDataReturnSysId"
        public async Task<string> SaveDataReturnSysIdAsync<T>(string sql, T data)
        {
            return await SaveDataReturnParameterNameAsync<T>(sql, data, "SysId");
        }


        public string SaveDataReturnSysId<T>(string sql, T data)
        {
            return SaveDataReturnParameterName<T>(sql, data, "SysId");
        }
        #endregion

        #region "SaveDataReturnParameterName"
        public async Task<string> SaveDataReturnParameterNameAsync<T>(string sql, T data, string parametername)
        {
            return await SaveDataReturnParameterNameInnerAsync(sql, data, parametername);
        }
        public async Task<string> SaveDataWithClobReturnParameterNameAsync<T>(string sql, T data, string parametername, string clobnames)
        {
            return await SaveDataReturnParameterNameInnerAsync(sql, data, parametername, clobnames);
        }

        public string SaveDataReturnParameterName<T>(string sql, T data, string parametername)
        {
            return SaveDataReturnParameterNameInnerAsync(sql, data, parametername).GetAwaiter().GetResult();
        }
        public async Task<string> SaveDataReturnParameterNameAsync<T>(string sql, T data, string parametername, DbType type)
        {
            return await SaveDataReturnParameterNameInnerAsync<T>(sql, data, parametername, type);
        }
        public string SaveDataReturnParameterName<T>(string sql, T data, string parametername, DbType type)
        {
            return SaveDataReturnParameterNameInnerAsync<T>(sql, data, parametername, type).GetAwaiter().GetResult();
        }
        public async Task<string> SaveDataReturnParameterNameAsync<T>(string sql, T data, string parametername, DbType type, int parametersize)
        {
            return await SaveDataReturnParameterNameInnerAsync<T>(sql, data, parametername, type, parametersize);
        }
        public string SaveDataReturnParameterName<T>(string sql, T data, string parametername, DbType type, int parametersize)
        {
            return SaveDataReturnParameterNameInnerAsync<T>(sql, data, parametername, type, parametersize).GetAwaiter().GetResult();
        }
        public async Task<string> SaveDataReturnParameterNameAsync<T>(string sql, T data, string parametername, int parametersize)
        {
            return await SaveDataReturnParameterNameInnerAsync<T>(sql, data, parametername, parametersize);
        }
        public string SaveDataReturnParameterName<T>(string sql, T data, string parametername, int parametersize)
        {
            return SaveDataReturnParameterNameInnerAsync<T>(sql, data, parametername, parametersize).GetAwaiter().GetResult();
        }

        #region "Inner"
        private async Task<string> SaveDataReturnParameterNameInnerAsync<T>(string sql, T data, string parametername, string clobfields)
        {
            var param = new OracleDynamicParameters();
            param.Add(parametername, dbType: OracleMappingType.Varchar2, direction: ParameterDirection.Output, size: 40);


            // Reflect over the properties of the data object
            foreach (var prop in typeof(T).GetProperties())
            {
                if (parametername.Equals(prop.Name, StringComparison.OrdinalIgnoreCase))
                    continue;

                var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (propertyType != typeof(string) && !propertyType.IsValueType)
                {
                    continue;
                }

                var value = prop.GetValue(data);

                bool isClob = propertyType == typeof(string) && IsClobField(clobfields, prop.Name);

                if (isClob)
                {
                    param.Add(prop.Name, value, OracleMappingType.Clob, ParameterDirection.Input);
                }
                else
                {
                    var oracleType = GetOracleMappingType(propertyType);
                    param.Add(prop.Name, value, oracleType, ParameterDirection.Input);
                }
            }



            await _connection.ExecuteAsync(sql, param, _transaction);

            return param.Get<string>(parametername);
        }

        private static bool IsClobField(string clobfields, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(clobfields) || string.IsNullOrWhiteSpace(propertyName))
            {
                return false;
            }

            var fields = clobfields.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());

            return fields.Any(x => x.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        }
        private async Task<string> SaveDataReturnParameterNameInnerAsync<T>(string sql, T data, string parametername)
        {
            var param = new DynamicParameters(data);
            param.Add(name: parametername, dbType: DbType.StringFixedLength, direction: ParameterDirection.Output, size: 40);

            await _connection.ExecuteAsync(sql, param, _transaction);

            return param.Get<string>(parametername);
        }
        private async Task<string> SaveDataReturnParameterNameInnerAsync<T>(string sql, T data, string parametername, DbType type)
        {
            var param = new DynamicParameters(data);
            param.Add(name: parametername, dbType: type, direction: ParameterDirection.Output);

            await _connection.ExecuteAsync(sql, param, _transaction);

            return param.Get<string>(parametername);
        }
        private async Task<string> SaveDataReturnParameterNameInnerAsync<T>(string sql, T data, string parametername, DbType type, int parametersize)
        {
            var param = new DynamicParameters(data);

            param.Add(name: parametername, dbType: type, direction: ParameterDirection.Output, size: parametersize);

            await _connection.ExecuteAsync(sql, param, _transaction);

            return param.Get<string>(parametername);
        }
        private async Task<string> SaveDataReturnParameterNameInnerAsync<T>(string sql, T data, string parametername, int parametersize)
        {
            var param = new DynamicParameters(data);
            param.Add(name: parametername, dbType: DbType.StringFixedLength, direction: ParameterDirection.Output, size: parametersize);

            await _connection.ExecuteAsync(sql, param, _transaction);

            return param.Get<string>(parametername);
        }
        #endregion
        #endregion

        #region "ExecuteProcedure"
        public async Task ExecuteProcedureAsync<T>(string procedurename, T data)
        {
            await _connection.ExecuteAsync(procedurename, data, null, null, CommandType.StoredProcedure);
        }
        public void ExecuteProcedure<T>(string procedurename, T data)
        {
            _connection.Execute(procedurename, data, null, null, CommandType.StoredProcedure);
        }


        public async Task ExecuteProcedureAsync(string procedurename)
        {
            await _connection.ExecuteAsync(procedurename, null, null, null, CommandType.StoredProcedure);
        }
        public void ExecuteProcedure(string procedurename)
        {
            _connection.Execute(procedurename, null, null, null, CommandType.StoredProcedure);
        }


        public async Task ExecuteProcedureAsync(string procedurename, DynamicParameters parameters)
        {
            await _connection.ExecuteAsync(procedurename, parameters, commandType: CommandType.StoredProcedure);
        }


        public void ExecuteProcedure(string procedurename, DynamicParameters parameters)
        {
            _connection.Execute(procedurename, parameters, commandType: CommandType.StoredProcedure);
        }
        #endregion

        #region "Multi"
        /// <summary>
        /// Retrieves paged data and the total record count from the database.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="dataQuery">The SQL query to retrieve the data.</param> 
        /// <param name="parameters">The parameters for the SQL queries.</param>
        /// <param name="map">An optional mapping function to transform the data (e.g., for complex relationships).</param>
        /// <returns>A <see cref="PagedResult{T}"/> containing the total record count and the paged data.</returns>
        public List<T> GetMappedData<T>(
            string dataQuery,          // SQL query for paged data
            DynamicParameters parameters,    // Parameters for the query  
            Func<T, T> map = null       // Optional mapping function for custom transformations
            )
        {

            // Execute the paged query
            var data = _connection.Query<T>(dataQuery, parameters).ToList();

            // Apply custom mapping if provided
            if (map != null)
            {
                data = data.Select(map).ToList();
            }

            // Return the total count and paged data
            return data;
        }

        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="dataQuery">The SQL query to retrieve the data.</param> 
        /// <param name="parameters">The parameters for the SQL queries.</param>
        /// <param name="map">An optional mapping function to transform the data (e.g., for complex relationships).</param>
        /// <returns>A <see cref="PagedResult{T}"/> containing the total record count and the paged data.</returns>
        public async Task<List<T>> GetMappedDataAsync<T>(
            string dataQuery,          // SQL query for paged data
            DynamicParameters parameters,    // Parameters for the query  
            Func<T, T> map = null       // Optional mapping function for custom transformations
        )
        {


            // Execute the paged query asynchronously to get the data for the current page
            var data = (await _connection.QueryAsync<T>(dataQuery, parameters)).ToList();

            // If a custom mapping function is provided, apply it to each item in the data
            if (map != null)
            {
                data = data.Select(map).ToList();
            }

            return data;
        }


        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database, with support for multi-mapping.
        /// </summary>
        /// <typeparam name="T1">The type of the primary entity to retrieve.</typeparam>
        /// <typeparam name="T2">The type of the second entity to map.</typeparam>
        /// <typeparam name="TResult">The type of the final result after mapping.</typeparam>
        /// <param name="dataQuery">The SQL query to retrieve the data.</param> 
        /// <param name="parameters">The parameters for the SQL queries.</param>
        /// <param name="map">A mapping function to transform the data (e.g., for complex relationships).</param>
        /// <param name="splitOn">The column name where the result set should be split for multi-mapping.</param>
        /// <returns>A list of mapped results.</returns>
        public async Task<List<TResult>> GetMappedDataAsync<T1, T2, TResult>(
            string dataQuery,
            object parameters,
            Func<T1, T2, TResult> map,
            string splitOn
        )
        {
            var data = (await _connection.QueryAsync(dataQuery, map, parameters, splitOn: splitOn)).ToList();
            return data;
        }

        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database, with support for multi-mapping.
        /// </summary>
        /// <typeparam name="T1">The type of the primary entity to retrieve.</typeparam>
        /// <typeparam name="T2">The type of the second entity to map.</typeparam>
        /// <typeparam name="T3">The type of the third entity to map.</typeparam>
        /// <typeparam name="TResult">The type of the final result after mapping.</typeparam>
        /// <param name="dataQuery">The SQL query to retrieve the data.</param> 
        /// <param name="parameters">The parameters for the SQL queries.</param>
        /// <param name="map">A mapping function to transform the data (e.g., for complex relationships).</param>
        /// <param name="splitOn">The column name where the result set should be split for multi-mapping.</param>
        /// <returns>A list of mapped results.</returns>
        public async Task<List<TResult>> GetMappedDataAsync<T1, T2, T3, TResult>(
            string dataQuery,
            object parameters,
            Func<T1, T2, T3, TResult> map,
            string splitOn
        )
        {
            var data = (await _connection.QueryAsync(dataQuery, map, parameters, splitOn: splitOn)).ToList();
            return data;
        }

        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database, with support for multi-mapping.
        /// </summary>
        /// <typeparam name="T1">The type of the primary entity to retrieve.</typeparam>
        /// <typeparam name="T2">The type of the second entity to map.</typeparam>
        /// <typeparam name="T3">The type of the third entity to map.</typeparam>
        /// <typeparam name="T4">The type of the fourth entity to map.</typeparam>
        /// <typeparam name="TResult">The type of the final result after mapping.</typeparam>
        /// <param name="dataQuery">The SQL query to retrieve the data.</param> 
        /// <param name="parameters">The parameters for the SQL queries.</param>
        /// <param name="map">A mapping function to transform the data (e.g., for complex relationships).</param>
        /// <param name="splitOn">The column name where the result set should be split for multi-mapping.</param>
        /// <returns>A list of mapped results.</returns>
        public async Task<List<TResult>> GetMappedDataAsync<T1, T2, T3, T4, TResult>(
            string dataQuery,
            object parameters,
            Func<T1, T2, T3, T4, TResult> map,
            string splitOn
        )
        {
            var data = (await _connection.QueryAsync(dataQuery, map, parameters, splitOn: splitOn)).ToList();
            return data;
        }

        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database, with support for multi-mapping.
        /// </summary>
        /// <typeparam name="T1">The type of the primary entity to retrieve.</typeparam>
        /// <typeparam name="T2">The type of the second entity to map.</typeparam>
        /// <typeparam name="T3">The type of the third entity to map.</typeparam>
        /// <typeparam name="T4">The type of the fourth entity to map.</typeparam>
        /// <typeparam name="T5">The type of the fifth entity to map.</typeparam>
        /// <typeparam name="TResult">The type of the final result after mapping.</typeparam>
        /// <param name="dataQuery">The SQL query to retrieve the data.</param> 
        /// <param name="parameters">The parameters for the SQL queries.</param>
        /// <param name="map">A mapping function to transform the data (e.g., for complex relationships).</param>
        /// <param name="splitOn">The column name where the result set should be split for multi-mapping.</param>
        /// <returns>A list of mapped results.</returns>
        public async Task<List<TResult>> GetMappedDataAsync<T1, T2, T3, T4, T5, TResult>(
            string dataQuery,
            object parameters,
            Func<T1, T2, T3, T4, T5, TResult> map,
            string splitOn
        )
        {
            var data = (await _connection.QueryAsync(dataQuery, map, parameters, splitOn: splitOn)).ToList();
            return data;
        }

        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database, with support for multi-mapping.
        /// </summary>
        /// <typeparam name="T1">The type of the primary entity to retrieve.</typeparam>
        /// <typeparam name="T2">The type of the second entity to map.</typeparam>
        /// <typeparam name="T3">The type of the third entity to map.</typeparam>
        /// <typeparam name="T4">The type of the fourth entity to map.</typeparam>
        /// <typeparam name="T5">The type of the fifth entity to map.</typeparam>
        /// <typeparam name="T6">The type of the sixth entity to map.</typeparam>
        /// <typeparam name="TResult">The type of the final result after mapping.</typeparam>
        /// <param name="dataQuery">The SQL query to retrieve the data.</param> 
        /// <param name="parameters">The parameters for the SQL queries.</param>
        /// <param name="map">A mapping function to transform the data (e.g., for complex relationships).</param>
        /// <param name="splitOn">The column name where the result set should be split for multi-mapping.</param>
        /// <returns>A list of mapped results.</returns>
        public async Task<List<TResult>> GetMappedDataAsync<T1, T2, T3, T4, T5, T6, TResult>(
            string dataQuery,
            object parameters,
            Func<T1, T2, T3, T4, T5, T6, TResult> map,
            string splitOn
        )
        {
            var data = (await _connection.QueryAsync(dataQuery, map, parameters, splitOn: splitOn)).ToList();
            return data;
        }

        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database, with support for multi-mapping.
        /// </summary>
        /// <typeparam name="T1">The type of the primary entity to retrieve.</typeparam>
        /// <typeparam name="T2">The type of the second entity to map.</typeparam>
        /// <typeparam name="T3">The type of the third entity to map.</typeparam>
        /// <typeparam name="T4">The type of the fourth entity to map.</typeparam>
        /// <typeparam name="T5">The type of the fifth entity to map.</typeparam>
        /// <typeparam name="T6">The type of the sixth entity to map.</typeparam>
        /// <typeparam name="T7">The type of the seventh entity to map.</typeparam>
        /// <typeparam name="TResult">The type of the final result after mapping.</typeparam>
        /// <param name="dataQuery">The SQL query to retrieve the data.</param> 
        /// <param name="parameters">The parameters for the SQL queries.</param>
        /// <param name="map">A mapping function to transform the data (e.g., for complex relationships).</param>
        /// <param name="splitOn">The column name where the result set should be split for multi-mapping.</param>
        /// <returns>A list of mapped results.</returns>
        public async Task<List<TResult>> GetMappedDataAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(
            string dataQuery,
            object parameters,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> map,
            string splitOn
        )
        {
            var data = (await _connection.QueryAsync(dataQuery, map, parameters, splitOn: splitOn)).ToList();
            return data;
        }
        #endregion


        #region "Paged"
        /// <summary>
        /// Retrieves paged data and the total record count from the database.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="pagedQuery">The SQL query to retrieve the paged data.</param>
        /// <param name="countQuery">The SQL query to retrieve the total record count.</param>
        /// <param name="parameters">The parameters for the SQL queries (e.g., filters, paging).</param>
        /// <param name="map">An optional mapping function to transform the data (e.g., for complex relationships).</param>
        /// <returns>A <see cref="PagedResult{T}"/> containing the total record count and the paged data.</returns>
        public Entities.PagedResult<T> GetPagedData<T>(
    string pagedQuery,          // SQL query for paged data
    string countQuery,          // SQL query for total count 
    object parameters,          // Parameters for the query (e.g., filters, paging)
    Func<T, T> map = null       // Optional mapping function for custom transformations
)
        {
            // Execute the total count query
            int totalRecords = _connection.ExecuteScalar<int>(countQuery, parameters);

            // Execute the paged query
            var data = _connection.Query<T>(pagedQuery, parameters).ToList();

            // Apply custom mapping if provided
            if (map != null)
            {
                data = data.Select(map).ToList();
            }

            // Return the total count and paged data
            return new Entities.PagedResult<T>
            {
                TotalRecords = totalRecords,
                Data = data
            };
        }

        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="pagedQuery">The SQL query to retrieve the paged data.</param>
        /// <param name="countQuery">The SQL query to retrieve the total record count.</param>
        /// <param name="parameters">The parameters for the SQL queries (e.g., filters, paging).</param>
        /// <param name="map">An optional mapping function to transform the data (e.g., for complex relationships).</param>
        /// <returns>A <see cref="PagedResult{T}"/> containing the total record count and the paged data.</returns>
        public async Task<Entities.PagedResult<T>> GetPagedDataAsync<T>(
            string pagedQuery,          // SQL query for paged data
            string countQuery,          // SQL query for total count 
            object parameters,          // Parameters for the query (e.g., filters, paging)
            Func<T, T> map = null       // Optional mapping function for custom transformations
        )
        {
            // Open the connection asynchronously
            //await connection.OpenAsync();

            // Execute the total count query asynchronously to get the total number of records
            int totalRecords = await _connection.ExecuteScalarAsync<int>(countQuery, parameters);

            // Execute the paged query asynchronously to get the data for the current page
            var data = (await _connection.QueryAsync<T>(pagedQuery, parameters)).ToList();

            // If a custom mapping function is provided, apply it to each item in the data
            if (map != null)
            {
                data = data.Select(map).ToList();
            }

            // Return the total record count and the paged data as a PagedResult object
            return new Entities.PagedResult<T>
            {
                TotalRecords = totalRecords, // Total number of records in the dataset
                Data = data                  // The paged data for the current page
            };
        }


        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database, with support for multi-mapping.
        /// </summary>
        /// <typeparam name="T1">The type of the primary entity to retrieve.</typeparam>
        /// <typeparam name="T2">The type of the second entity to map (optional).</typeparam>
        /// <typeparam name="TResult">The type of the final result after mapping.</typeparam>
        /// <param name="pagedQuery">The SQL query to retrieve the paged data.</param>
        /// <param name="countQuery">The SQL query to retrieve the total record count.</param>
        /// <param name="parameters">The parameters for the SQL queries (e.g., filters, paging).</param>
        /// <param name="map">A function to map the results to the final object.</param>
        /// <param name="splitOn">The column name where the result set should be split for multi-mapping.</param>
        /// <returns>A <see cref="PagedResult{TResult}"/> containing the total record count and the paged data.</returns>
        public async Task<Entities.PagedResult<TResult>> GetPagedDataAsync<T1, T2, TResult>(
            string pagedQuery,          // SQL query for paged data
            string countQuery,          // SQL query for total count
            object parameters,          // Parameters for the query (e.g., filters, paging)
            Func<T1, T2, TResult> map, // Multi-mapping function
            string splitOn              // Column name to split the result set
        )
        {
            // Open the connection asynchronously
            //await _connection.OpenAsync();

            // Execute the total count query asynchronously
            int totalRecords = await _connection.ExecuteScalarAsync<int>(countQuery, parameters);

            // Execute the paged query asynchronously with multi-mapping
            var data = (await _connection.QueryAsync(pagedQuery, map, parameters, splitOn: splitOn)).ToList();

            // Return the total count and paged data
            return new Entities.PagedResult<TResult>
            {
                TotalRecords = totalRecords,
                Data = data
            };
        }

        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database, with support for multi-mapping.
        /// </summary>
        /// <typeparam name="T1">The type of the primary entity to retrieve.</typeparam>
        /// <typeparam name="T2">The type of the 2nd entity to map (optional).</typeparam>
        /// <typeparam name="T3">The type of the 3rd entity to map (optional).</typeparam> 
        /// <typeparam name="TResult">The type of the final result after mapping.</typeparam>
        /// <param name="pagedQuery">The SQL query to retrieve the paged data.</param>
        /// <param name="countQuery">The SQL query to retrieve the total record count.</param>
        /// <param name="parameters">The parameters for the SQL queries (e.g., filters, paging).</param>
        /// <param name="map">A function to map the results to the final object.</param>
        /// <param name="splitOn">The column name where the result set should be split for multi-mapping.</param>
        /// <returns>A <see cref="PagedResult{TResult}"/> containing the total record count and the paged data.</returns>
        public async Task<Entities.PagedResult<TResult>> GetPagedDataAsync<T1, T2, T3, TResult>(
            string pagedQuery,          // SQL query for paged data
            string countQuery,          // SQL query for total count
            object parameters,          // Parameters for the query (e.g., filters, paging)
            Func<T1, T2, T3, TResult> map, // Multi-mapping function
            string splitOn              // Column name to split the result set
        )
        {
            // Open the connection asynchronously
            //await _connection.OpenAsync();

            // Execute the total count query asynchronously
            int totalRecords = await _connection.ExecuteScalarAsync<int>(countQuery, parameters);

            // Execute the paged query asynchronously with multi-mapping
            var data = (await _connection.QueryAsync(pagedQuery, map, parameters, splitOn: splitOn)).ToList();

            // Return the total count and paged data
            return new Entities.PagedResult<TResult>
            {
                TotalRecords = totalRecords,
                Data = data
            };
        }

        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database, with support for multi-mapping.
        /// </summary>
        /// <typeparam name="T1">The type of the primary entity to retrieve.</typeparam>
        /// <typeparam name="T2">The type of the 2nd entity to map (optional).</typeparam>
        /// <typeparam name="T3">The type of the 3rd entity to map (optional).</typeparam>
        /// <typeparam name="T4">The type of the 4th entity to map (optional).</typeparam> 
        /// <typeparam name="TResult">The type of the final result after mapping.</typeparam>
        /// <param name="pagedQuery">The SQL query to retrieve the paged data.</param>
        /// <param name="countQuery">The SQL query to retrieve the total record count.</param>
        /// <param name="parameters">The parameters for the SQL queries (e.g., filters, paging).</param>
        /// <param name="map">A function to map the results to the final object.</param>
        /// <param name="splitOn">The column name where the result set should be split for multi-mapping.</param>
        /// <returns>A <see cref="PagedResult{TResult}"/> containing the total record count and the paged data.</returns>
        public async Task<Entities.PagedResult<TResult>> GetPagedDataAsync<T1, T2, T3, T4, TResult>(
            string pagedQuery,          // SQL query for paged data
            string countQuery,          // SQL query for total count
            object parameters,          // Parameters for the query (e.g., filters, paging)
            Func<T1, T2, T3, T4, TResult> map, // Multi-mapping function
            string splitOn              // Column name to split the result set
        )
        {
            // Open the connection asynchronously
            //await _connection.OpenAsync();

            // Execute the total count query asynchronously
            int totalRecords = await _connection.ExecuteScalarAsync<int>(countQuery, parameters);

            // Execute the paged query asynchronously with multi-mapping
            var data = (await _connection.QueryAsync(pagedQuery, map, parameters, splitOn: splitOn)).ToList();

            // Return the total count and paged data
            return new Entities.PagedResult<TResult>
            {
                TotalRecords = totalRecords,
                Data = data
            };
        }

        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database, with support for multi-mapping.
        /// </summary>
        /// <typeparam name="T1">The type of the primary entity to retrieve.</typeparam>
        /// <typeparam name="T2">The type of the 2nd entity to map (optional).</typeparam>
        /// <typeparam name="T3">The type of the 3rd entity to map (optional).</typeparam>
        /// <typeparam name="T4">The type of the 4th entity to map (optional).</typeparam>
        /// <typeparam name="T5">The type of the 5th entity to map (optional).</typeparam> 
        /// <typeparam name="TResult">The type of the final result after mapping.</typeparam>
        /// <param name="pagedQuery">The SQL query to retrieve the paged data.</param>
        /// <param name="countQuery">The SQL query to retrieve the total record count.</param>
        /// <param name="parameters">The parameters for the SQL queries (e.g., filters, paging).</param>
        /// <param name="map">A function to map the results to the final object.</param>
        /// <param name="splitOn">The column name where the result set should be split for multi-mapping.</param>
        /// <returns>A <see cref="PagedResult{TResult}"/> containing the total record count and the paged data.</returns>
        public async Task<Entities.PagedResult<TResult>> GetPagedDataAsync<T1, T2, T3, T4, T5, TResult>(
            string pagedQuery,          // SQL query for paged data
            string countQuery,          // SQL query for total count
            object parameters,          // Parameters for the query (e.g., filters, paging)
            Func<T1, T2, T3, T4, T5, TResult> map, // Multi-mapping function
            string splitOn              // Column name to split the result set
        )
        {
            // Open the connection asynchronously
            //await _connection.OpenAsync();

            // Execute the total count query asynchronously
            int totalRecords = await _connection.ExecuteScalarAsync<int>(countQuery, parameters);

            // Execute the paged query asynchronously with multi-mapping
            var data = (await _connection.QueryAsync(pagedQuery, map, parameters, splitOn: splitOn)).ToList();

            // Return the total count and paged data
            return new Entities.PagedResult<TResult>
            {
                TotalRecords = totalRecords,
                Data = data
            };
        }


        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database, with support for multi-mapping.
        /// </summary>
        /// <typeparam name="T1">The type of the primary entity to retrieve.</typeparam>
        /// <typeparam name="T2">The type of the 2nd entity to map (optional).</typeparam>
        /// <typeparam name="T3">The type of the 3rd entity to map (optional).</typeparam>
        /// <typeparam name="T4">The type of the 4th entity to map (optional).</typeparam>
        /// <typeparam name="T5">The type of the 5th entity to map (optional).</typeparam>
        /// <typeparam name="T6">The type of the 6th entity to map (optional).</typeparam> 
        /// <typeparam name="TResult">The type of the final result after mapping.</typeparam>
        /// <param name="pagedQuery">The SQL query to retrieve the paged data.</param>
        /// <param name="countQuery">The SQL query to retrieve the total record count.</param>
        /// <param name="parameters">The parameters for the SQL queries (e.g., filters, paging).</param>
        /// <param name="map">A function to map the results to the final object.</param>
        /// <param name="splitOn">The column name where the result set should be split for multi-mapping.</param>
        /// <returns>A <see cref="PagedResult{TResult}"/> containing the total record count and the paged data.</returns>
        public async Task<Entities.PagedResult<TResult>> GetPagedDataAsync<T1, T2, T3, T4, T5, T6, TResult>(
            string pagedQuery,          // SQL query for paged data
            string countQuery,          // SQL query for total count
            object parameters,          // Parameters for the query (e.g., filters, paging)
            Func<T1, T2, T3, T4, T5, T6, TResult> map, // Multi-mapping function
            string splitOn              // Column name to split the result set
        )
        {
            // Open the connection asynchronously
            //await _connection.OpenAsync();

            // Execute the total count query asynchronously
            int totalRecords = await _connection.ExecuteScalarAsync<int>(countQuery, parameters);

            // Execute the paged query asynchronously with multi-mapping
            var data = (await _connection.QueryAsync(pagedQuery, map, parameters, splitOn: splitOn)).ToList();

            // Return the total count and paged data
            return new Entities.PagedResult<TResult>
            {
                TotalRecords = totalRecords,
                Data = data
            };
        }

        /// <summary>
        /// Retrieves paged data and the total record count asynchronously from the database, with support for multi-mapping.
        /// </summary>
        /// <typeparam name="T1">The type of the primary entity to retrieve.</typeparam>
        /// <typeparam name="T2">The type of the 2nd entity to map (optional).</typeparam>
        /// <typeparam name="T3">The type of the 3rd entity to map (optional).</typeparam>
        /// <typeparam name="T4">The type of the 4th entity to map (optional).</typeparam>
        /// <typeparam name="T5">The type of the 5th entity to map (optional).</typeparam>
        /// <typeparam name="T6">The type of the 6th entity to map (optional).</typeparam>
        /// <typeparam name="T7">The type of the 7th entity to map (optional).</typeparam>
        /// <typeparam name="TResult">The type of the final result after mapping.</typeparam>
        /// <param name="pagedQuery">The SQL query to retrieve the paged data.</param>
        /// <param name="countQuery">The SQL query to retrieve the total record count.</param>
        /// <param name="parameters">The parameters for the SQL queries (e.g., filters, paging).</param>
        /// <param name="map">A function to map the results to the final object.</param>
        /// <param name="splitOn">The column name where the result set should be split for multi-mapping.</param>
        /// <returns>A <see cref="PagedResult{TResult}"/> containing the total record count and the paged data.</returns>
        public async Task<Entities.PagedResult<TResult>> GetPagedDataAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(
            string pagedQuery,          // SQL query for paged data
            string countQuery,          // SQL query for total count
            object parameters,          // Parameters for the query (e.g., filters, paging)
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> map, // Multi-mapping function
            string splitOn              // Column name to split the result set
        )
        {
            // Open the connection asynchronously
            //await _connection.OpenAsync();

            // Execute the total count query asynchronously
            int totalRecords = await _connection.ExecuteScalarAsync<int>(countQuery, parameters);

            // Execute the paged query asynchronously with multi-mapping
            var data = (await _connection.QueryAsync(pagedQuery, map, parameters, splitOn: splitOn)).ToList();

            // Return the total count and paged data
            return new Entities.PagedResult<TResult>
            {
                TotalRecords = totalRecords,
                Data = data
            };
        }
        #endregion




        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes of the resources used by the OracleDataAccessLayer.
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _transaction?.Dispose();
                    _connection?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~OracleDataAccess1() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

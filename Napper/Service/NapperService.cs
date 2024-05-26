
using Microsoft.Extensions.Options;

using Napper.Database;

using System.Collections.Generic;
using System.Linq;

namespace Napper.Service
{
    public sealed class NapperService(IOptions<NapperOption> option)
    {
        private readonly IDataAccessor _accessor = option.Value.DbTypeName switch
        {
            "MySQL" => new MySqlDataAccessor(option.Value.ConnectionString),
            "SQLServer" => new SqlServerDataAccessor(option.Value.ConnectionString),
            "Postgresql" => new PsqlDataAccessor(option.Value.ConnectionString),
            _ => throw new NotSupportedException($"Not Support DbType :{option.Value.DbTypeName}"),
        };

        public string Message { get; private set; } = "";

        ~NapperService()
        {
            _accessor.CloseConnection();
        }


        public List<Dictionary<string, object>>? SelectQuery(string tablename, Dictionary<string, object?>? parameters = null)
        {
            if (!_accessor.TryOpenConnection(out string errorMessage))
            {
                this.Message = errorMessage;
                return null;
            }

            var query = _accessor.GetSelectQuery(tablename);
            var oneRecordQuery = _accessor.GetSelectQuery(tablename, true);

            if (!_accessor.TryInitCommand(query, out errorMessage, true, oneRecordQuery, parameters))
            {
                this.Message = errorMessage;
                _accessor.CloseConnection();
                return null;
            }

            if (!_accessor.TrySelectExecute(out List<Dictionary<string, object>> result, out errorMessage))
            {
                this.Message = errorMessage;
                _accessor.CloseConnection();
                return null;
            }

            return result;
        }

        public IEnumerable<string>? TableAllQuery()
        {
            if (!_accessor.TryOpenConnection(out string errorMessage))
            {
                this.Message = errorMessage;
                return null;
            }

            var query = _accessor.GetTableAllQuery();

            if (!_accessor.TryInitCommand(query, out errorMessage))
            {
                this.Message = errorMessage;
                _accessor.CloseConnection();
                return null;
            }

            if (!_accessor.TrySelectExecute(out var dataList, out errorMessage))
            {
                this.Message = errorMessage;
                _accessor.CloseConnection();
                return null;
            }

            List<string>? result = null;

            if (dataList != null)
            {
                result = [];
                result.AddRange(from Dictionary<string, object> dateItem in dataList
                                select Convert.ToString(dateItem.First().Value) ?? "");
            }

            return result;

        }

        public bool InsertQuery(string tablename, Dictionary<string, object?> parameters)
        {
            if (!_accessor.TryOpenConnection(out string errorMessage))
            {
                this.Message = errorMessage;
                return false;
            }

            var query = _accessor.GetInsertQuery(tablename, parameters);

            if (!_accessor.TryInitCommand(query, out errorMessage, false, "", parameters))
            {
                this.Message = errorMessage;
                _accessor.CloseConnection();
                return false;
            }

            if (!_accessor.TryInsertExecute(out errorMessage))
            {
                this.Message = errorMessage;
                _accessor.CloseConnection();
                return false;
            }

            return true;
        }

    }
}

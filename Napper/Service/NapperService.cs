
using Microsoft.Extensions.Options;

using Napper.Database;

using System.Collections.Generic;

namespace Napper.Service
{
    public sealed class NapperService
    {
        private IDataAccessor _accessor;

        public string Message { get; private set; } = "";

        public NapperService(IOptions<NapperOption> option)
        {
            switch (option.Value.DbTypeName)
            {
                case "MySQL":
                    _accessor = new MySqlDataAccessor(option.Value.ConnectionString);
                    break;
                case "SQLServer":
                    _accessor = new SqlServerDataAccessor(option.Value.ConnectionString);
                    break;
                case "Postgresql":
                    _accessor = new PsqlDataAccessor(option.Value.ConnectionString);
                    break;
                default:
                    throw new NotSupportedException($"Not Support DbType :{option.Value.DbTypeName}");
            }
        }
        ~NapperService()
        {
            _accessor.CloseConnection();
        }


        public List<Dictionary<string, object>>? SelectQuery(string tablename, Dictionary<string, object?>? parameters = null)
        {
            string errorMessage = "";
            if (!_accessor.TryOpenConnection(out errorMessage))
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

            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (!_accessor.TrySelectExecute(out result, out errorMessage))
            {
                this.Message = errorMessage;
                _accessor.CloseConnection();
                return null;
            }

            return result;
        }

        public IEnumerable<string>? TableAllQuery()
        {
            string errorMessage = "";
            if (!_accessor.TryOpenConnection(out errorMessage))
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
                result = new List<string>();

                for(var i = 0; i < dataList.Count; i++)

                {
                    result.Add(Convert.ToString(dataList[i].First().Value) ?? "");
                }
            }

            return result;

        }

        public bool InsertQuery(string tablename, Dictionary<string, object?> parameters)
        {
            string errorMessage = "";
            if (!_accessor.TryOpenConnection(out errorMessage))
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

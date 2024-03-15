
using Microsoft.Extensions.Options;

using Napper.Database;

namespace Napper.Service
{
    public sealed class NapperService
    {
        private IDataAccessor _accessor;

        public string Message { get; private set; } = "";

        public NapperService(IOptions<NapperOption> option)
        {
            switch(option.Value.DbTypeName)
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

        public List<Dictionary<string, object>>? SelectQuery(string tablename, Dictionary<string, string>? parameters = null)
        {
            _accessor.OpenConnection();

            var query = _accessor.GetSelectQuery(tablename);
            var oneRecordQuery = _accessor.GetSelectQuery(tablename, true);

            if (!_accessor.InitCommand(query, oneRecordQuery, parameters))
            {
                this.Message = _accessor.ErrorMessage;
                _accessor.CloseConnection();
                return null;
            }

            var result = _accessor.SelectExecute();
            this.Message = _accessor.ErrorMessage;
            _accessor.CloseConnection();

            return result;
        }

        public IEnumerable<string>? TableAllQuery()
        {
            _accessor.OpenConnection();
            var query = _accessor.GetTableAllQuery();

            if (!_accessor.InitCommand(query))
            {
                this.Message = _accessor.ErrorMessage;
                _accessor.CloseConnection();
                return null;
            }

            var dataList = _accessor.SelectExecute();

            this.Message = _accessor.ErrorMessage;
            _accessor.CloseConnection();

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
        ~NapperService()
        {
            _accessor.CloseConnection();
        }

    }
}

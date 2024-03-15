using Npgsql;

using System.Data;

namespace Napper.Database
{
    public class PsqlDataAccessor : IDataAccessor
    {
        NpgsqlConnection? _connection = null;
        NpgsqlCommand? _command = null;
        string _connectionString = "";
        string _errorMessage = "";

        public ConnectionState State => (_connection == null) ? ConnectionState.Closed : _connection.State;

        public string ErrorMessage => _errorMessage;

        public PsqlDataAccessor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool OpenConnection()
        {
            try
            {
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();
                return true;
            }

            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }

            return false;
        }

        public void CloseConnection()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }
        public string GetTableAllQuery()
        {
            return "SELECT tablename FROM pg_tables WHERE schemaname = current_schema()";
        }

        public string GetSelectQuery(string tablename, bool isOneRecord = false)
        {
            var query = $"SELECT * FROM {tablename}";

            return !isOneRecord ? query : query + " LIMIT 1";
        }

        public bool InitCommand(string query, string oneRecordQuery = "", Dictionary<string, string>? parameters = null)
        {
            if (_connection == null) OpenConnection();
            var fields = new Dictionary<string, Type>();

            if (parameters != null && oneRecordQuery == "")
            {
                _errorMessage = "OneRecordQuery is Empty.";
                return false;
            }

            // パラメーターがあれば1回SQLを発行して、各フィールドの型を覚えておく。
            if (parameters != null && oneRecordQuery != "")
            {
                try
                {
                    var limitCommand = new NpgsqlCommand(oneRecordQuery, _connection);
                    var limitReader = limitCommand.ExecuteReader();
                    if (limitReader != null && limitReader.HasRows)
                    {
                        while (limitReader.Read())
                        {
                            for (var i = 0; i < limitReader.FieldCount; i++)
                            {
                                fields.Add(limitReader.GetName(i), limitReader.GetFieldType(i));
                            }
                        }
                        limitReader.Close();
                    }
                    limitCommand.Dispose();
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                    return false;
                }

            }

            _command = new NpgsqlCommand();
            var whereSql = "";
            // データがあればパラメータをフィールドの型でキャストする
            if (parameters != null && fields.Count > 0)
            {
                try
                {
                    foreach (var parameter in parameters)
                    {
                        if (fields.ContainsKey(parameter.Key))
                        {
                            if (whereSql == "")
                            {
                                whereSql = " WHERE " + parameter.Key + " = @" + parameter.Key;
                            }
                            else
                            {
                                whereSql = " AND " + parameter.Key + " = @" + parameter.Key;
                            }
                            _command.Parameters.Add(new NpgsqlParameter("@" + parameter.Key, Convert.ChangeType(parameter.Value, fields[parameter.Key])));
                        }
                        else
                        {
                            _errorMessage = $"Fields NotFound :{parameter.Key} ";
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                    return false;
                }
            }

            _command.Connection = _connection;
            _command.CommandText = query + whereSql;

            return true;
        }
        public List<Dictionary<string, object>>? SelectExecute()
        {
            var result = new List<Dictionary<string, object>>();

            if (_connection == null) OpenConnection();
            if (_command == null) throw new NullReferenceException(nameof(SelectExecute));

            try
            {
                var dataReader = _command.ExecuteReader();
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var dataItem = new Dictionary<string, object>();
                        for (var i = 0; i < dataReader.FieldCount; i++)
                        {
                            dataItem.Add(dataReader.GetName(i), dataReader.GetValue(i));
                        }
                        result.Add(dataItem);

                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }

            return null;
        }

        public bool CreateExecute()
        {
            throw new NotImplementedException();
        }

        public bool DeleteExecute()
        {
            throw new NotImplementedException();
        }





        public bool UpdateExecute()
        {
            throw new NotImplementedException();
        }
    }
}

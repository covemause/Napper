using System.Data;
using System.Data.SqlClient;

namespace Napper.Database
{
    public class SqlServerDataAccessor : IDataAccessor
    {
        SqlConnection? _connection = null;
        SqlCommand? _command = null;
        string _connectionString = "";

        public ConnectionState State => (_connection == null) ? ConnectionState.Closed : _connection.State;

        public SqlServerDataAccessor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool TryOpenConnection(out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
                return true;
            }

            catch (Exception ex)
            {
                errorMessage = ex.Message;
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
            return "select name from sys.objects where type = 'U'";
        }

        public string GetSelectQuery(string tablename, bool isOneRecord = false)
        {
            if (isOneRecord)
            {
                return $"SELECT TOP 1 * FROM {tablename}";
            }
            return $"SELECT * FROM {tablename}";
        }
        public string GetInsertQuery(string tablename, Dictionary<string, object?> parameters)
        {
            var query = "INSERT INTO " + tablename + " ({0}) VALUES ({1})";

            var fields = "";
            var fieldParams = "";

            foreach (var item in parameters)
            {
                if (fields == "")
                {
                    fields = item.Key;
                    fieldParams = "@" + item.Key;
                }
                else
                {
                    fields += ", " + item.Key;
                    fieldParams += ", @" + item.Key;
                }
            }

            return string.Format(query, fields, fieldParams);
        }

        public bool TryInitCommand(string query, out string errorMessage, bool isWhere = false, string oneRecordQuery = "", Dictionary<string, object?>? parameters = null)
        {
            errorMessage = string.Empty;

            if (_connection == null) return false;
            var fields = new Dictionary<string, Type>();


            // パラメーターがあれば1回SQLを発行して、各フィールドの型を覚えておく。
            if (parameters != null && oneRecordQuery != "")
            {
                try
                {
                    var limitCommand = new SqlCommand(oneRecordQuery, _connection);
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
                    errorMessage = ex.Message;
                    return false;
                }

            }


            _command = new SqlCommand();
            var whereSql = "";
            // データがあればパラメータをフィールドの型でキャストする
            if (isWhere && parameters != null && fields.Count > 0)
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
                                whereSql += " AND " + parameter.Key + " = @" + parameter.Key;
                            }
                            _command.Parameters.Add(new SqlParameter("@" + parameter.Key, Convert.ChangeType(parameter.Value, fields[parameter.Key])));
                        }
                        else
                        {
                            errorMessage = $"Fields NotFound :{parameter.Key} ";
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return false;
                }
            }
            else if (!isWhere && parameters != null)
            {
                foreach (var item in parameters)
                {
                    _command.Parameters.Add(new SqlParameter("@" + item.Key, item.Value));
                }
            }

            _command.Connection = _connection;
            _command.CommandText = query + whereSql;


            return true;
        }

        public bool TrySelectExecute(out List<Dictionary<string, object>> result, out string errorMessage)
        {

            if (_connection == null || _command == null)
                throw new NullReferenceException(nameof(TrySelectExecute));

            errorMessage = string.Empty;
            result = new List<Dictionary<string, object>>();

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

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

        }

        public bool TryInsertExecute(out string errorMessage)
        {
            if (_connection == null || _command == null)
                throw new NullReferenceException(nameof(TrySelectExecute));

            errorMessage = string.Empty;

            var result = false;

            try
            {
                if (_command.ExecuteNonQuery() == 1)
                {
                    return true;
                }
            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
            }

            return result;
        }

        public bool TryDeleteExecute(out string errorMessage)
        {
            throw new NotImplementedException();
        }


        public bool TryUpdateExecute(out string errorMessage)
        {
            throw new NotImplementedException();
        }

    }
}

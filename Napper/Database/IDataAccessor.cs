using System.Data;

namespace Napper.Database
{
    public interface IDataAccessor
    {
        ConnectionState State { get; }
        string ErrorMessage { get; }

        bool OpenConnection();

        void CloseConnection();

        string GetTableAllQuery();
        string GetSelectQuery(string tablename, bool isOneRecord = false);

        bool InitCommand(string query, string oneRecordQuery = "", Dictionary<string, string>? parameters = null);

        List<Dictionary<string, object>>? SelectExecute();
        bool CreateExecute();
        bool UpdateExecute();
        bool DeleteExecute();


    }
}

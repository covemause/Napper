using System.Data;

namespace Napper.Database
{
    public interface IDataAccessor
    {
        ConnectionState State { get; }

        bool TryOpenConnection(out string errorMessage);

        void CloseConnection();

        string GetTableAllQuery();
        string GetSelectQuery(string tablename, bool isOneRecord = false);
        string GetInsertQuery(string tablename, Dictionary<string, object?> parameters);

        bool TryInitCommand(string query, out string errorMessage, bool isWhere = false, string oneRecordQuery = "", Dictionary<string, object?>? parameters = null);

        bool TrySelectExecute(out List<Dictionary<string, object>> result, out string errorMessage);

        bool TryInsertExecute(out string errorMessage);
        bool TryUpdateExecute(out string errorMessage);
        bool TryDeleteExecute(out string errorMessage);

    }
}

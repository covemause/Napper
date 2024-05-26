using System.Text.Json;

namespace Napper.Controllers
{
    public static class QueryFomatter
    {
        public static bool TryUrlQueryStringToDictionary(string queryString, out Dictionary<string, object?>? values)
        {
            values = null;
            if (string.IsNullOrEmpty(queryString)) return true;

            var parameters = queryString.Split("&");
            values = [];

            foreach (var item in parameters)
            {
                if (string.IsNullOrEmpty(item)) continue;
                var target = item[0] == '?' ? item[1..] : item;

                var equalPos = target.IndexOf('=');
                if (equalPos == -1) { return false; }
                var key = target[..equalPos];
                var val = target[(equalPos + 1)..];

                values.Add(key, val);
            }

            return true;
        }

        public static bool TryJsonElementObjectToDictinary(JsonElement element, out Dictionary<string, object?>? values)
        {
            values = null;
            if (element.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            values = [];

            foreach (var item in element.EnumerateObject())
            {
                var t = item.Value.GetType();
                var s = item.Value.ToString();
                switch(item.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        if (item.Value.TryGetDateTime(out var datetimeValue))
                        {
                            values.Add(item.Name, datetimeValue);
                        }
                        else
                        {
                            values.Add(item.Name, item.Value.GetString());
                        }
                        break;
                    case JsonValueKind.Number:
                        if (item.Value.TryGetInt32(out int intValue))
                        {
                            values.Add(item.Name, intValue);
                            break;
                        }
                        if (item.Value.TryGetDouble(out double doubleValue))
                        {
                            values.Add(item.Name, doubleValue);
                            break;
                        }
                        if (item.Value.TryGetDecimal(out decimal decimalValue))
                        {
                            values.Add(item.Name, decimalValue);
                            break;
                        }
                        throw new ArgumentOutOfRangeException(null, nameof(item.Name));
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        values.Add(item.Name, item.Value.GetBoolean());
                        break;
                    case JsonValueKind.Null:
                        values.Add(item.Name, null);
                        break;
                    case JsonValueKind.Object:
                        values.Add(item.Name, item.Value.GetString());
                        break;



                }
            }

            return true;
        }
        public static bool TryJsonElementArrayToDictinary(JsonElement element, out List<Dictionary<string, object?>>? values)
        {
            values = null;
            if (element.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            values = [];

            foreach (var item in element.EnumerateArray())
            {
                //var dic = new Dictionary<string, object?>();
                if (item.ValueKind == JsonValueKind.Object)
                {
                    if (!TryJsonElementObjectToDictinary(item, out var dic)) return false;
                    if (dic == null) return false;
                    values.Add(dic);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}

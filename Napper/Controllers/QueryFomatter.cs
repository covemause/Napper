namespace Napper.Controllers
{
    public static class QueryFomatter
    {
        public static bool TryUrlQueryStringToDictionary(string queryString, out Dictionary<string, string>? values)
        {
            values = null;
            if (string.IsNullOrEmpty(queryString)) return true;

            var parameters = queryString.Split("&");
            values = new Dictionary<string, string>();

            foreach (var item in parameters)
            {
                if (string.IsNullOrEmpty(item)) continue;
                var target = item[0] == '?' ? item.Substring(1) : item;

                var equalPos = target.IndexOf('=');
                if (equalPos == -1) { return false; }
                var key = target.Substring(0, equalPos);
                var val = target.Substring(equalPos + 1);

                values.Add(key, val);
            }

            return true;
        }
    }
}

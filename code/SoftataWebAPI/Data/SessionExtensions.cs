using System.Text.Json;

namespace SoftataWebAPI.Data
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            System.Diagnostics.Debug.WriteLine("======SETTING===========");
            foreach (var k in session.Keys)
            {
                System.Diagnostics.Debug.WriteLine(key);
            }
            session.SetString(key, JsonSerializer.Serialize(value));
            System.Diagnostics.Debug.WriteLine("========SET=============");
            foreach (var k in session.Keys)
            {
                System.Diagnostics.Debug.WriteLine(key);
            }
            System.Diagnostics.Debug.WriteLine("========END SET=============");
        }

        public static T? Get<T>(this ISession session, string key)
        {
            System.Diagnostics.Debug.WriteLine("========GET=========");
            foreach (var k in session.Keys)
            {
                System.Diagnostics.Debug.WriteLine(key);
            }
            System.Diagnostics.Debug.WriteLine("========GOT=========");
            var value = session.GetString(key);
            if (value == null)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(value);
        }
    }
}

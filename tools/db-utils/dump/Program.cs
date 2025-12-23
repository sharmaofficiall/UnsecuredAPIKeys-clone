using System;
using Npgsql;

class Program {
    static int Main(string[] args) {
        var cs = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? "Host=localhost;Database=UnsecuredAPIKeys;Username=postgres;Password=sunny123;Port=5432";
        try {
            using var c = new NpgsqlConnection(cs);
            c.Open();
            using var cmd = new NpgsqlCommand("SELECT \"Id\", \"ApiKey\", \"ApiType\", \"Status\", \"SearchProvider\", \"FirstFoundUTC\", \"LastFoundUTC\" FROM \"APIKeys\" WHERE \"Status\"=1 ORDER BY \"Id\" DESC LIMIT 500;", c);
            using var r = cmd.ExecuteReader();
            while (r.Read()) {
                var id = r.GetInt64(0);
                var key = r.GetString(1);
                var type = r.GetInt32(2);
                var status = r.GetInt32(3);
                var provider = r.GetInt32(4);
                var first = r.IsDBNull(5) ? "NULL" : r.GetDateTime(5).ToString();
                var last = r.IsDBNull(6) ? "NULL" : r.GetDateTime(6).ToString();
                Console.WriteLine($"Id={id} Type={type} Status={status} Provider={provider} FirstFound={first} LastFound={last} Key={key}");
            }
            return 0;
        } catch (Exception ex) {
            Console.Error.WriteLine("Error: " + ex);
            return 2;
        }
    }
}

using MySqlConnector;

namespace Message_Parser.Model
{
    internal class DBConnection
    {
        private static readonly Lazy<MySqlConnection> _lazyConnection =
        new Lazy<MySqlConnection>(() =>
        {
            var conn = new MySqlConnection("server=localhost;user=root;password=1234;database=AnalyticsDB;");
            conn.Open();
            return conn;
        });

        private DBConnection() { }

        public static MySqlConnection Instance => _lazyConnection.Value;
    }
}

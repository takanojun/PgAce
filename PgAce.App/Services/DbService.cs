using System.Threading.Tasks;
using Npgsql;

namespace PgAce.App.Services
{
    public class DbService
    {
        public async Task<NpgsqlConnection> ConnectAsync(string connectionString)
        {
            var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            return conn;
        }
    }
}

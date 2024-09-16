using Oracle.ManagedDataAccess.Client;

namespace ConsoleApp1.ConnectionFactories;

public class OracleConnectionFactory
{
    public static OracleConnection CreateConnection(string connectionString, bool keepAlive)
    {
        var result = new OracleConnection(connectionString);

        result.Open();
        return result;
    }
}
using System.Data.Common;
using System.Text;
using ConsoleApp1;
using ConsoleApp1.ConnectionFactories;
using Oracle.ManagedDataAccess.Client;
using System.Text.Json;

class Program
{
    static void Main()
    {

        AsgharClass.run().Wait();

        // // var connectionString =
        // //     "USER ID=QA_MASTER;STATEMENT CACHE SIZE=0;PASSWORD=123456;CONNECTION TIMEOUT=300;STATEMENT CACHE PURGE=True;DATA SOURCE=\"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.30.112)(PORT=1521))(CONNECT_DATA=(SID=ORCL)))\";POOLING=False";
        // // var connectionString = "User Id=admin;Password=!@#123qwe;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SID=orcl)));Pooling=true;";
        //
        // var connectionStringBuilder = new OracleConnectionStringBuilder();
        // connectionStringBuilder.UserID = "admin";
        // connectionStringBuilder.Password = "!@#123qwe";
        // connectionStringBuilder.DataSource =
        //     "(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SID=orcl)))";
        //
        // connectionStringBuilder.Pooling = true;
        // connectionStringBuilder.MinPoolSize = 4;
        // connectionStringBuilder.ConnectionLifeTime = 5;
        // connectionStringBuilder.ValidateConnection
        //
        // var connectionString = connectionStringBuilder.ToString() + ";Enable.BestPractices=true";
        //
        // var x = new OracleConnection(connectionString);
        // x.Open();
        //
        // OracleInternalConnection pool = ((OracleInternalConnection)conn).GetConnectionPool();
        //     
        // // Retrieve statistics
        // int maxConnections = pool.MaxConnections;
        // int currentConnections = pool.CurrentConnections;
        // int availableConnections = pool.AvailableConnections;
        //     
        // Console.WriteLine($"Max Connections: {maxConnections}");
        // Console.WriteLine($"Current Connections: {currentConnections}");
        // Console.WriteLine($"Available Connections: {availableConnections}");
        //
        //
        // SimpleQueryExecuting(x);
        //
        // for (int i = 0; i < 10; i++)
        // {
        //     var y = new OracleConnection(connectionString);
        //     y.KeepAlive = true;
        //     y.Open();
        //     Task.Run(() => { SimpleQueryExecuting(y); });
        // }
        //
        // Thread.Sleep(20000);
        //
        //
        // using (var command = new OracleCommand("SELECT 1 FROM DUAL", x))
        // {
        //
        //     command.ExecuteNonQuery();
        //     Console.WriteLine("Connection is still open.");
        // }
        //
        // Console.WriteLine(x.State);
        // // var y = new OracleConnection(connectionString);
        // // y.Open();
        // var connection = OracleConnectionFactory.CreateConnection(connectionString, false);
        //
        //
        // // ExtractProfileSetting2(connection);
        //
        // // TestConnection.TestConnectionKeepAlive(connection);

    }

    // private static void MonitorPoolStatistics(OracleConnection connection)
    // {
    //     while (true)
    //     {
    //         Console.Clear();
    //         // Console.WriteLine($"Current Connections: {_connection.ConnectionPool.GetConnectionCount()}");
    //         Console.WriteLine($"Maximum Connections: {connection.ConnectionPool.MaxConnections}");
    //         Console.WriteLine($"Available Connections: {connection.ConnectionPool.GetAvailableCount()}");
    //         Console.WriteLine($"Connection Timeout: {connection.ConnectionPool.Timeout}");
    //         Thread.Sleep(5000); // Check every 5 seconds
    //     }
    // }

    private static void SimpleQueryExecuting(OracleConnection x)
    {
        using (var command = new OracleCommand("SELECT 1 FROM DUAL", x))
        {
            var counter = 0;
            while (true)
            {
                command.ExecuteNonQuery();
                Console.WriteLine("Connection is still open.");
                counter++;
                if (counter > 1000) break;
            }
            
        }
    }


    private static void ExtractProfileSetting2(OracleConnection connection)
    {
        var profileInfo = GetProfileInfo(connection);
        var sessionInfo = GetSessionInfo(connection);

        var x = JsonSerializer.Serialize(profileInfo);
        var y = JsonSerializer.Serialize(sessionInfo);
        Console.WriteLine(x);
    }

    private static Dictionary<string?, string> GetSessionInfo(OracleConnection connection)
    {
        var username = "admin";
        var program = "MSSE.DIA.exe";
        var query =
            $"select sessionParameter.* from ( SELECT sid FROM v$session WHERE username = '{username}') sidTable join v$ses_optimizer_env sessionParameter on sidTable.sid = sessionParameter.sid";

        using (OracleCommand command = new OracleCommand(query, connection))
        {
            using (OracleDataReader reader = command.ExecuteReader())
            {
                return ExtractSessionInfos(reader);
            }
        }
    }

    private static Dictionary<string?, string> GetProfileInfo(OracleConnection connection)
    {
        var username = "ADMIN";
        var query =
            $"select profileInfoTable.* from (SELECT username, profile FROM dba_users WHERE username = '{username}') profileInputTable join dba_profiles profileInfoTable on profileInputTable.profile = profileInfoTable.profile";

        using (OracleCommand command = new OracleCommand(query, connection))
        {
            using (OracleDataReader reader = command.ExecuteReader())
            {
                return ExtractProfileInfos(reader);
            }
        }
    }
    
    private static Dictionary<string?, string> ExtractProfileInfos(OracleDataReader reader)
    {
        var x = GetTableColumnName(reader);

        var log = new Dictionary<string?, string>(); 
        while (reader.Read())
        {
            var stringBuilder = new StringBuilder();
            object[] values = new object[reader.FieldCount];
            reader.GetValues(values);

            for (int i = 2; i < x.Count; i++)
            {
                stringBuilder.Append(x[i]);
                stringBuilder.Append(": ");
                stringBuilder.Append(values[i]);
                stringBuilder.Append("; ");
            }

            log[values[1].ToString()] =  stringBuilder.ToString();
        }

        return log;
    }
    private static Dictionary<string?, string> ExtractSessionInfos(OracleDataReader reader)
    {
        var x = GetTableColumnName(reader);

        var log = new Dictionary<string?, string>(); 
        while (reader.Read())
        {
            var stringBuilder = new StringBuilder();
            object[] values = new object[reader.FieldCount];
            reader.GetValues(values);

            for (int i = 3; i < x.Count; i++)
            {
                stringBuilder.Append(x[i]);
                stringBuilder.Append(": ");
                stringBuilder.Append(values[i]);
                stringBuilder.Append("; ");
            }

            log[values[2].ToString()] =  stringBuilder.ToString();
        }

        return log;
    }


    public static List<string> GetTableColumnName(DbDataReader dataReader)
    {
        var result = new List<string>();
        var columnsSchema = dataReader.GetColumnSchema();

        foreach (var column in columnsSchema)
        {
            result.Add(column.ColumnName);
        }

        return result;
    }
    
}




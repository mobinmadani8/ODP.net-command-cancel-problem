using Oracle.ManagedDataAccess.Client;

namespace ConsoleApp1;

public class TestConnection
{
    public static void TestConnectionKeepAlive(OracleConnection oracleConnection)
    {
        for (int i = 0; i < 50; i++)
        {
            Console.WriteLine($"command{i} started: ");
            var command = new OracleCommand("SELECT owner, table_name FROM all_tables", oracleConnection);
            var c = command.ExecuteNonQuery();
            Console.WriteLine(i+" "+ c);

            if (i == 20)
            {
                oracleConnection.Close();
                Thread.Sleep(7000);
                
            }
        }
    }

}
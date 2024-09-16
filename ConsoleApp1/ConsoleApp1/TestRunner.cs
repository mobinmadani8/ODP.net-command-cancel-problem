using System.Data.Common;
using Oracle.ManagedDataAccess.Client;

namespace ConsoleApp1
{
    public class TestRunner
    {
        public static async Task Run()
        {
            var connectionString =
                "STATEMENT CACHE SIZE=0;CONNECTION TIMEOUT=300;STATEMENT CACHE PURGE=True;DATA SOURCE=\"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.30.112)(PORT=1521))(CONNECT_DATA=(SID=ORCL)))\";POOLING=False;USER ID=QA_MASTER; PASSWORD=123456";

            var insertQuery = "INSERT INTO MockDataTable (Id) VALUES (:Id)";

            await using var connection = new OracleConnection(connectionString);
            connection.KeepAlive = true;
            await connection.OpenAsync();
            var counter = 0;

            var createTableQuery =
                "BEGIN " +
                "EXECUTE IMMEDIATE 'CREATE GLOBAL TEMPORARY TABLE MockDataTable (" +
                "Id NUMBER) ON COMMIT PRESERVE ROWS';" +
                "EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; " +
                "END;";

            await using (var createTableCommand = new OracleCommand(createTableQuery, connection))
            {
                await createTableCommand.ExecuteNonQueryAsync();
            }
            
            Console.WriteLine("table created");
            
            while (true)
            {
                Console.WriteLine("connection state before running query " + connection.State);

                await using (var command = new OracleCommand(insertQuery, connection))
                {
                    var ids = Enumerable.Range(1, 1000001).ToArray();
                    command.ArrayBindCount = 1000000;
                    command.Parameters.Add(new OracleParameter("Id", ids));

                    try
                    {
                        var cancelRunner = Task.Run(async () =>
                        {
                            await Task.Delay(600);
                            command.Cancel();
                        });
                        
                        var queryExecutor = command.ExecuteNonQueryAsync(default);

                        await Task.WhenAll(cancelRunner, queryExecutor);
                        
                        Console.WriteLine("Executing finish");
                    }
                    catch (DbException e)
                    {
                        Console.WriteLine(e);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }

                if (counter % 10 == 0)
                {
                    await using var command = new OracleCommand("SELECT COUNT(*) FROM MockDataTable", connection);
                    using var cancellationTokenSource2 = new CancellationTokenSource();

                    try
                    {
                        await command.ExecuteScalarAsync(cancellationTokenSource2.Token);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred for scalar: {ex.Message}");
                    }
                }

                counter++;
            }
        }
    }
}
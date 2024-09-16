using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class AsgharClass
    {
        public static async Task run()
        {
            string connectionString =
                "STATEMENT CACHE SIZE=0;CONNECTION TIMEOUT=300;STATEMENT CACHE PURGE=True;DATA SOURCE=\"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.87.128)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORACLE)))\";POOLING=False;USER ID=ADMIN; PASSWORD=!@#123qwe";

            string query = "INSERT INTO MockDataTable (Id) VALUES (:Id)";
            // $"INSERT INTO MockDataTable (Id, Column1, Column2, Column3, Column4, Column5, Column6, Column7, Column8)" +
            // $"VALUES (1, 'Val1', 'Val2', 3, TO_DATE('{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 'YYYY-MM-DD HH24:MI:SS'), 'Val6', 7, 'Val7', 9)";


            // using (var connection = new OracleConnection(connectionString))
            // {
            //     connection.KeepAlive = true;
            //     await connection.OpenAsync();
            //     // Try to create the table if it doesn't already exist
            //     try
            //     {
            //         var createTableQuery =
            //             "BEGIN " +
            //             "EXECUTE IMMEDIATE 'CREATE GLOBAL TEMPORARY TABLE MockDataTable (" +
            //             "Id NUMBER) ON COMMIT PRESERVE ROWS';" +
            //             "EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; " +
            //             "END;";
            //
            //         using (var createTableCommand = new OracleCommand(createTableQuery, connection))
            //         {
            //             await createTableCommand.ExecuteNonQueryAsync();
            //         }
            //     }
            //     catch (Exception ex)
            //     {
            //         Console.WriteLine($"Error creating table: {ex.Message}");
            //     }
            // }

            var counter = 0;
            while (true)
            {
                using (var connection = new OracleConnection(connectionString))
                {
                    connection.KeepAlive = true;
                    await connection.OpenAsync();
                    Console.WriteLine(connection.State);
                    await Task.Delay(1000);

                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        cancellationTokenSource.CancelAfter(600);
                        using (var command = new OracleCommand(query, connection))
                        {
                            // Define array of 100 rows
                            int[] ids = new int[1000000];
                            for (int i = 0; i < 1000000; i++)
                            {
                                ids[i] = i + 1; // Assign mock data for Ids (1 to 100)
                            }

                            // Bind array to the query
                            command.ArrayBindCount = 1000000;
                            command.Parameters.Add(new OracleParameter("Id", ids));
                            try
                            {
                                var x = cancellationTokenSource.Token;
                                // using (var y = x.Register(() => CancelCommand(command)))
                                {
                                    Console.WriteLine("Executing query...");
                                    await command.ExecuteNonQueryAsync(x);
                                    Console.WriteLine("Executing finish");

                                    // command.Cancel();
                                    // await x;
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                Console.WriteLine("Query was cancelled after 1 second.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"An error occurred: {ex.Message}");
                            }
                        }
                    }
                }

                // await Task.Delay(2000);
                if (counter % 10 == 0)
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        connection.KeepAlive = true;
                        await connection.OpenAsync();
                        Console.WriteLine(connection.State);

                        using (var command = new OracleCommand("SELECT COUNT(*) FROM MockDataTable", connection))
                        {
                            using (var cancellationTokenSource2 = new CancellationTokenSource())
                            {
                                Console.WriteLine("hello");
                                // cancellationTokenSource2.CancelAfter(10000);
                                while (true)
                                {
                                    try
                                    {
                                        Console.WriteLine(
                                            await command.ExecuteScalarAsync(cancellationTokenSource2.Token));
                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"An error occurred for scalar: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                }


                counter++;
                // Add a short delay before the next iteration
                // await Task.Delay(1000);
            }
        }


        private static void CancelCommand(OracleCommand command)
        {
            command.Cancel();
            // command.Dispose();

            // Task.Delay(500).Wait();
        }
    }
}

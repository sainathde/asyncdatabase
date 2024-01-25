using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Npgsql;

class Program
{
    static async Task Main()
    {
        string apiUrl = "https://randomuser.me/api/";

        // Retrieve user data from the API
        var userData = await MakeHttpRequest(apiUrl);

        // Save user data to PostgreSQL database
        await SaveToDatabase(userData);
    }

    static async Task<UserData> MakeHttpRequest(string apiUrl)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON response into an object
                    return JsonSerializer.Deserialize<UserData>(content);
                }
                else
                {
                    Console.WriteLine($"Negative Response: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return null;
    }

    static async Task SaveToDatabase(UserData userData)
    {
        try
        {
            string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=sainath;Database=randomusers";


            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                foreach (var result in userData.results)
                {
                    string insertQuery = "INSERT INTO users (first_name, last_name, email) VALUES (@FirstName, @LastName, @Email)";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", result.name.first);
                        cmd.Parameters.AddWithValue("@LastName", result.name.last);
                        cmd.Parameters.AddWithValue("@Email", result.email);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }

            Console.WriteLine("Data saved to PostgreSQL database.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to database: {ex.Message}");
        }
    }

    // Define a class structure that matches the JSON response
    public class UserData
    {
        public Result[] results { get; set; }

        public class Result
        {
            public Name name { get; set; }
            public string email { get; set; }
        }

        public class Name
        {
            public string first { get; set; }
            public string last { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ApiConsoleApp
{
    public class DogResponse
    {
        [JsonPropertyName("message")]
        public List<string> Photos { get; set; }
    }

    class Program
    {

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string connString = config.GetConnectionString("DefaultConnection");

            Console.Write("Введіть породу собаки (наприклад, husky, pug, akita): ");
            string breed = Console.ReadLine()?.Trim().ToLower();
            string url = $"https://dog.ceo/api/breed/{breed}/images/random/3";
            List<string> photoUrls = new List<string>();

            using HttpClient client = new HttpClient();
            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"можливо, такої породи немає.");
                    return;
                }
                string json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<DogResponse>(json);
                if (data?.Photos == null || data.Photos.Count == 0)
                {
                    Console.WriteLine("повернуло порожню відповідь.");
                    return;
                }
                photoUrls = data.Photos;
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("проблема з інтернетом .");
                return;
            }

            Console.WriteLine($"\n--- Отримано з API для породи '{breed}' ---");
            foreach (var link in photoUrls)
            {
                Console.WriteLine(link);
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                await conn.OpenAsync();

                string sqlBreed = @"
                    INSERT INTO Breeds (Name) VALUES (@name)
                    ON CONFLICT (Name) DO UPDATE SET Name = EXCLUDED.Name
                    RETURNING Id;";

                int breedId;
                using (var cmd = new NpgsqlCommand(sqlBreed, conn))
                {
                    cmd.Parameters.AddWithValue("name", breed);
                    breedId = (int)await cmd.ExecuteScalarAsync();
                }

                string sqlPhoto = @"
                    INSERT INTO Photos (ImageUrl, BreedId) VALUES (@url, @breedId)
                    ON CONFLICT (ImageUrl) DO NOTHING;";
                foreach (var link in photoUrls)
                {
                    using (var cmd = new NpgsqlCommand(sqlPhoto, conn))
                    {
                        cmd.Parameters.AddWithValue("url", link);
                        cmd.Parameters.AddWithValue("breedId", breedId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            Console.WriteLine("\nДані успішно збережено в БД!");

            Console.WriteLine("\n--- Дані з Бази Даних ---");
            using (var conn = new NpgsqlConnection(connString))
            {
                await conn.OpenAsync();
               
                string sqlSelect = @"
                    SELECT b.Name, p.ImageUrl
                    FROM Photos p
                    JOIN Breeds b ON p.BreedId = b.Id
                    ORDER BY p.Id DESC LIMIT 5;";
                using (var cmd = new NpgsqlCommand(sqlSelect, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Console.WriteLine($"[{reader.GetString(0)}] -> {reader.GetString(1)}");
                    }
                }
            }
            Console.WriteLine("\nГотово!");
            Console.ReadLine();
        }
    }
}






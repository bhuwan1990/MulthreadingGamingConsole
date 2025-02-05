using ConsoleApp1.Extension;
using ConsoleApp1.Models;
using System;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        //Console.Write("Enter Console ID: ");
        var consoleId = Guid.NewGuid();

        Console.Write("Enter Your Name: ");
        string playerName = Console.ReadLine();

        // check if player name is empty then ask untill he enterd the name properly


        while (string.IsNullOrEmpty(playerName))
        {
            Console.WriteLine("Player name is empty. Please enter your name.");
            playerName = Console.ReadLine();
        }


        var playerData = await GetDataFromMainDashboard(playerName);

        //Check if same player is already active
        if (playerData.Status == "Player is already active")
        {
            Console.WriteLine("Player is already active. Close the existing one");
            Console.ReadKey();
        }
        // check if player is old player then only show the score
        else if (playerData.Score != 0 && playerData.Status=="Stopped")
        {
            // Display player score
            DisplayPlayerScore(playerData);
            Console.ReadKey();
        }
        else
        {
            // Display player score
            DisplayPlayerScore(playerData);

            
            int score = 0;
            bool isPlaying = true;

            // Start the game
            while (isPlaying)
            {
                Console.WriteLine("\nPress 'U' to increase score, 'E' to exit:");
                char input = char.ToUpper(Console.ReadKey().KeyChar);

                PlayerScore playerScore = new PlayerScore
                {
                    Id = consoleId,
                    GuidShort = consoleId.ToShortGuid(),
                    Name = playerName,
                    Score = score,
                };

                 
                if (input == 'U')
                {
                    score++;
                    playerScore.Status = "Running";

                    await SendDataToMainDashboard(playerScore);
                }
                else if (input == 'E')
                {
                    playerScore.Status = "Stopped";
                    await SendDataToMainDashboard(playerScore);
                    isPlaying = false;
                }
            }
        }


    }

    static void DisplayPlayerScore(PlayerScore playerData)
    {
        Console.WriteLine("-------------------------------------------------");
        Console.WriteLine("Console ID\tPlayer Name\tScore");
        Console.WriteLine("-------------------------------------------------");

        Console.WriteLine($"{playerData.Id.ToShortGuid()}\t{playerData.Name}\t\t{playerData.Score}");
        Console.WriteLine("-------------------------------------------------");
    }

    static async Task SendDataToMainDashboard(PlayerScore playerScore)
    {
        RequestModel requestModel = new RequestModel
        {
            PlayerScore = playerScore,
            RequestType = "UPDATE_SCORE"
        };

        using (var pipeClient = new NamedPipeClientStream(".", "score_pipe", PipeDirection.InOut, PipeOptions.Asynchronous))
        {
            await pipeClient.ConnectAsync();
            using (var writer = new StreamWriter(pipeClient, Encoding.UTF8))
            {
                string jsonData = JsonSerializer.Serialize(requestModel);  // Convert to JSON
                await writer.WriteLineAsync(jsonData);
            }
        }
    }

    static async Task<PlayerScore> GetDataFromMainDashboard(string name)
    {
        var response = new PlayerScore();
        RequestModel requestModel = new RequestModel
        {
            PlayerScore = new PlayerScore
            {
                Name = name
            },
            RequestType = "CHECK_PLAYER"
        };
        using (var pipeClient = new NamedPipeClientStream(".", "score_pipe", PipeDirection.InOut))
        {
            await pipeClient.ConnectAsync();
            using (var writer = new StreamWriter(pipeClient, Encoding.UTF8))
            {
                string jsonData = JsonSerializer.Serialize(requestModel);  // Convert to JSON

                await writer.WriteLineAsync(jsonData);
                await writer.FlushAsync();
                var kk = pipeClient.IsConnected;
                using (var reader = new StreamReader(pipeClient, Encoding.UTF8, leaveOpen: true))
                {
                    var message = reader.ReadLine(); // Read response from dashboard
                    response = JsonSerializer.Deserialize<PlayerScore>(message);

                }
            }

        }

        return response;
    }
}

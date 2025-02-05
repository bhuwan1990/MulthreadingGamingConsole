using MaindashBoard;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;

class Program
{
    static List<ConsoleScore> playerList = new List<ConsoleScore>();
    static object consoleLock = new object();

    static void Main(string[] args)
    {
        int consoleCount = args.Length > 0 ? int.Parse(args[0]) : 3; // Default to 3 consoles

        //for (int i = 1; i <= consoleCount; i++)
        //{
        //    var guid = Guid.NewGuid();

        //    playerList.Add(new ConsoleScore { Id = guid, GuIdShort = guid.GetShortGuid(), Name = "Waiting", Score = 0, Status = "Stopped" });
        //}

        Thread listenerThread = new Thread(StartPipeListener);
        listenerThread.IsBackground = true;
        listenerThread.Start();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Main Dashboard - Live Scores");
            Console.WriteLine("ID\t\tName\tScore\tStatus");

            foreach (var console in playerList.OrderByDescending(x=>x.Score))
            {
                Console.WriteLine($"{console.GuidShort}\t{console.Name}\t{console.Score}\t{console.Status}");
            }

            //Parallel.ForEach(playerList, console =>
            //{
            //    lock (consoleLock)
            //    {
            //        Console.WriteLine($"{console.GuIdShort}\t{console.Name}\t{console.Score}\t{console.Status}");
            //    }
            //});
            Thread.Sleep(1000);
        }
    }

    static void StartPipeListener()
    {
        while (true)
        {
            using ( var pipeServer = new NamedPipeServerStream("score_pipe", PipeDirection.InOut))
            {
                pipeServer.WaitForConnection();
                using (var reader = new StreamReader(pipeServer, Encoding.UTF8))
                {

                    string message = reader.ReadLine();
                    if (!string.IsNullOrEmpty(message))
                    {
                        //var parts =  message.Split(',');

                        //Deseralize JSON data
                        var player = JsonSerializer.Deserialize<RequestModel>(message);

                        if (player?.RequestType=="UPDATE_SCORE")
                        {
                            UpdateScore(player.PlayerScore);

                        }
                        else 
                        if (player?.RequestType == "CHECK_PLAYER")
                        {
                            var playerData = GetPlayerData(player.PlayerScore);
                            if (playerData == null)
                            {
                                UpdateScore(player.PlayerScore);
                                playerData = player.PlayerScore;
                                SendDataToConsole(pipeServer, playerData);

                            }
                            else
                            {
                                if (playerData.Status == "Running")
                                {
                                    playerData.Status= "Player is already active";
                                    SendDataToConsole(pipeServer, playerData);
                                    playerData.Status = "Running"; //due to the fact that the player is already active
                                }
                                else
                                {
                                    SendDataToConsole(pipeServer, playerData);
                                }
                            }
                            
                        }
                    }
                }

            }
        }
    }

    private static void SendDataToConsole(NamedPipeServerStream pipeServer, ConsoleScore? playerData)
    {
        using (var writer = new StreamWriter(pipeServer, Encoding.UTF8))
        {

            writer.WriteLine(JsonSerializer.Serialize(playerData));
        }
    }


    static void UpdateScore(ConsoleScore player)
    {
        var console = playerList.FirstOrDefault(c => c.Name.ToLower() == player.Name.ToLower());
        if (console != null)
        {
            if (player.Status!="Stopped")
            {
                //console.Name = player.Name;
                console.Score++;
            }
           
            console.Status = player.Status;
        }
        else
        {
            player.Id = Guid.NewGuid();
            player.Score = 0;
            player.GuidShort = player.Id.GetShortGuid();
            player.Status = "Running";
            playerList.Add(player);
        }
    }
    static ConsoleScore GetPlayerData(ConsoleScore player)
    {
        var console = playerList.FirstOrDefault(c => c.Name.ToLower() == player.Name.ToLower());
        return console;
    }
}

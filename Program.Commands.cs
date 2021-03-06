﻿using System;

namespace PokeD.Server.Android
{
    public static partial class Program
    {
        private static bool ExecuteCommand(string message)
        {
            var command = message.Remove(0, 1).ToLower();
            message = message.Remove(0, 1);

            if (message.StartsWith("stop"))
            {
                Server?.Stop();
                Console.WriteLine("Stopped the server. Press any key to continue...");
                Console.Read();
                //Environment.Exit((int) ExitCodes.Success);
            }

            else if (command.StartsWith("help server"))
                return Server.ExecuteCommand(message.Remove(0, 11));

            else if (command.StartsWith("help"))
                return ExecuteHelpCommand(message.Remove(0, 4));

            else
                return Server.ExecuteCommand(message);

            return true;
        }

        private static bool ExecuteHelpCommand(string command) { return false; }
    }
}

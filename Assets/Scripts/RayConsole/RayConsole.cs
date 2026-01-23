using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RayConsole
{
    public class RayConsole
    {
        private readonly string _prefix;
        private readonly IEnumerable<IConsoleCommand> _commands;

        public RayConsole(string prefix, IEnumerable<IConsoleCommand> commands)
        {
            Debug.Log("<color=#6DE741> RayConsole commands interface initialized... </color>");

            _prefix = prefix;
            _commands = commands;
        }

        public void ExecuteCommand(string commandInput, string[] args)
        {
            bool commandFound = false;

            foreach (var command in _commands)
            {
                if (command == null)
                {
                    Debug.LogError("Null command detected in RayConsole command list");
                    continue;
                }
                if (!commandInput.Equals(command.CommandName, StringComparison.OrdinalIgnoreCase)) { continue; }

                commandFound = true;

                if (command.Execute(args)) { return; }
            }

            if (!commandFound)
            {
                Debug.LogWarning($"Unknown command: {commandInput}");
            }

        }

        public void ExecuteCommand(string inputValue)
        {
            if (!inputValue.StartsWith(_prefix)) { return; }

            inputValue = inputValue.Remove(0, _prefix.Length);

            string[] inputSplit = inputValue.Split(' ');

            string commandInput = inputSplit[0];
            string[] args = inputSplit.Skip(1).ToArray();

            ExecuteCommand(commandInput, args);
        }

    }
}

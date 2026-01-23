using UnityEngine;

namespace RayConsole
{
    [CreateAssetMenu(fileName = "HelpCommand", menuName = "RayConsole/Commands/Help Command", order = 1)]
    public class HelpCommand : ConsoleCommand
    {
        public override bool Execute(string[] args)
        {
            if (args.Length > 0)
            {
                foreach (ConsoleCommand command in RayConsoleBehaviour.Instance.Commands)
                {
                    if(command.CommandName == args[0])
                    {
                        Debug.Log($"- {command.CommandName}: {command.Description}");
                        return true;
                    }
                }
                Debug.Log($"Command '{args[0]}' not found.");
            }
            else
            {
                ShowAllCommands();
            }

            return true;
        }

        void ShowAllCommands()
        {
            Debug.Log("Available Commands:");
            foreach (var command in RayConsoleBehaviour.Instance.Commands)
            {
                Debug.Log($"- {command.CommandName}: {command.Description}");
            }
        }
    }
}
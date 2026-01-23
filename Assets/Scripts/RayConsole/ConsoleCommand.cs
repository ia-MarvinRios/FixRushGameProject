using UnityEngine;

namespace RayConsole
{
    /// <summary>
    /// Definition of a console command.
    /// </summary>
    public interface IConsoleCommand
    {
        string CommandName { get; }
        string Description { get; }
        bool Execute(string[] args);
    }

    /// <summary>
    /// Implementation of a console command as a ScriptableObject.
    /// </summary>
    public abstract class ConsoleCommand : ScriptableObject, IConsoleCommand
    {
        [SerializeField] private string commandName = string.Empty;
        [SerializeField] private string description = string.Empty;

        public string CommandName => commandName;
        public string Description => description;

        public abstract bool Execute(string[] args);
    }
}

using UnityEngine;

namespace RayConsole
{
    [CreateAssetMenu(fileName = "New log command", menuName = "RayConsole/Commands/LogCommand", order = 1)]
    public class LogCommand : ConsoleCommand
    {
        public override bool Execute(string[] args)
        {
            string message = string.Join(string.Empty, args);

            Debug.Log(message);

            return true;
        }
    }
}

using UnityEngine;

namespace RayConsole
{
    [CreateAssetMenu(fileName = "ClearConsoleCommand", menuName = "RayConsole/Commands/Clear Console", order = 1)]
    public class ClearConsoleCommand : ConsoleCommand
    {
        public override bool Execute(string[] args)
        {
            RayConsoleBehaviour.Instance.Clear();

            return true;
        }
    }
}
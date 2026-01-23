using RayConsole;
using UnityEngine;

[CreateAssetMenu(fileName = "Fix Command", menuName = "RayConsole/Commands/Fix Command", order = 1)]
public class FixCommand : ConsoleCommand
{
    public override bool Execute(string[] args)
    {
        if (args.Length > 0)
        {
            Debug.Log("Usage: /fix");
            return false;
        }
        /*
        if (!bool.TryParse(args[0], out bool shouldSpawn))
        {
            Debug.Log("<color=red>Invalid argument. Please provide 'true' or 'false'.</color>");
            return false;
        }
        */

        if (VManager.Instance.ActiveVehicles.Count > 0)
        {
            Destroy(VManager.Instance.ActiveVehicles[0]);
        }
        else Debug.LogAssertion("No vehicles left to fix.");

        return true;

    }
}

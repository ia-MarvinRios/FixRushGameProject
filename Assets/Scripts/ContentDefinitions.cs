using System.Collections;
using UnityEngine;

namespace FixRush
{

    /// <summary>
    /// Class that represents the data of a player, such as their name, ready status, and customization options.
    /// </summary>
    public class PlayerData
    {
        internal int ActorNumber { get; private set; }
        public string PlayerName { get; private set; }
        public bool IsReady { get; private set; }
        public int BodyID { get; private set; }
        public int HatID { get; private set; }
        public string SkinColorHex { get; private set; }
        public PlayerData(int actorNumber, string playerName, bool isReady, int bodyID, int hatID, string skinColorHex)
        {
            ActorNumber = actorNumber;
            PlayerName = playerName;
            IsReady = isReady;
            BodyID = bodyID;
            HatID = hatID;
            SkinColorHex = skinColorHex;
        }
    }

    /// <summary>
    /// Class that represents the data of a room, such as its name, current player count and maximum player count.
    /// </summary>
    public class RoomData
    {
        public string Name;
        public int PlayerCount;
        public int MaxPlayers;
    }

    [System.Serializable]
    public struct Cosmetic
    {
        public string Name;
        public bool Unlocked;
        public GameObject Prefab;
    }

    /// <summary>
    /// Interface for objects that can be interacted with by the player. 
    /// The implementation of the Interact method will depend on the specific behavior of the object, 
    /// such as what happens when the player interacts with it, how it affects the player, etc.
    /// </summary>
    public interface IInteractable
    {
        public enum Type
        {
            Simple,
            Hold,
            Still,
        }

        public Type InteractionType { get; }
        public float HoldTime { get; }
        public void Interact(PlayerController player);
        public void CancelInteraction(PlayerController player);
    }

    /// <summary>
    /// Interface for objects that can be picked up and dropped by the player. 
    /// The implementation of these methods will depend on the specific behavior of the object, such as how it is held, how it affects the player, etc.
    /// </summary>
    public interface IPickupable
    {
        public void PickUp(PlayerController player);
        public void Drop(PlayerController player);
    }

    /// <summary>
    /// Interface that represents an issue that can occur in the game, such as a flat tire or a dirty car.
    /// </summary>
    public interface IIssue
    {
        /// <summary>
        /// Specifies the category or status type for an issue.
        /// </summary>
        public enum Type
        {
            Tires,
            Dirty,
            //Engine,
        }

        public Type IssueType { get; }
        public bool IsFixed { get; }

        /// <summary>
        /// Cleans up the issue, such as hiding UI elements, resetting variables, etc. This method is called after the issue has been fixed and is no longer needed.
        /// </summary>
        void CleanUp();
        /// <summary>
        /// Coroutine that handles the process of fixing the issue. This method is called when the player starts interacting with the issue.
        /// </summary>
        IEnumerator FixingCoroutine();
    }
}

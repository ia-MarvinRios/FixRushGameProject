using FixRushGame;
using System.Collections.Generic;
using UnityEngine;

namespace FixRushGame
{
    public class AuxPlayer : MonoBehaviour
    {
        public readonly List<FixRushGame.Interactable> FocusCandidates = new();
        public GameObject GrabbedObj { get; set; }
        public Interactable FocusedObj { get; set; }
    }
}

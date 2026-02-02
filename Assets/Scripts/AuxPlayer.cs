using FixRushGame;
using System.Collections.Generic;
using UnityEngine;

public class AuxPlayer : MonoBehaviour
{
    public readonly List<Interactable> FocusCandidates = new();
    public GameObject GrabbedObj { get; set; }
    public Interactable FocusedObj { get; set; }
}

using FixRush;
using System.Collections;
using UnityEngine;

public class Bucket : Pickable
{
    [Header("Bucket Settings")]
    [SerializeField] private bool _isFull = false;
    internal bool IsFull { get => _isFull; set => _isFull = value; }
    
}

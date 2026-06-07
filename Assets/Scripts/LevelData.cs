using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Level Settings")]
    [SerializeField, Range(MIN_DIFFICULTY, MAX_DIFFICULTY)] internal int LevelDifficulty;
    [SerializeField] internal float TargetCash;
    [SerializeField] internal float TimeLimitSeconds;
    [SerializeField] internal int Level2Price;
    [SerializeField] internal Vector3[] Spawnpoints;

    internal const int MIN_DIFFICULTY = 1;
    internal const int MAX_DIFFICULTY = 20;

    private void OnValidate()
    {
        // Check if the level difficulty is within the allowed range
        if (LevelDifficulty < MIN_DIFFICULTY)
        {
            Debug.LogWarning($"Level difficulty cannot be less than {MIN_DIFFICULTY}. Resetting to minimum.");
            LevelDifficulty = MIN_DIFFICULTY;
        }
        else if (LevelDifficulty > MAX_DIFFICULTY)
        {
            Debug.LogWarning($"Level difficulty cannot be greater than {MAX_DIFFICULTY}. Resetting to maximum.");
            LevelDifficulty = MAX_DIFFICULTY;
        }
    }
}

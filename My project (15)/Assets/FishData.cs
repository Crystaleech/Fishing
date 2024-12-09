using UnityEngine;

[CreateAssetMenu(fileName = "FishData", menuName = "Fishing/FishData")]
public class FishData : ScriptableObject
{
    public string fishName;
    public float spawnChance; // Probability of spawning in its assigned zone
    public float escapeChance; // Base chance of escape
    public float minAngle; // Minimum rod angle for spawning in this fish's zone
    public float maxAngle; // Maximum rod angle for spawning in this fish's zone
    public GameObject fishPrefab; // Fish prefab to instantiate

    // New fields for the fishing minigame
    public Sprite fishSprite; // Icon representing this fish in the minigame
    public float maxProgress; // Maximum progress needed to successfully catch this fish
    public float minLength;
    public float maxLength;

    // Check if the fish is eligible based on the casting angle
    public bool IsEligible(float angle)
    {
        return angle >= minAngle && angle <= maxAngle;
    }

    public string GetSizeLabel(float length)
    {
        if (length <= minLength + (maxLength - minLength) / 3)
            return "S";
        else if (length <= minLength + 2 * (maxLength - minLength) / 3)
            return "M";
        else
            return "L";
    }

    // Determine if the fish escapes based on escapeChance
}

using System.Collections.Generic;
using UnityEngine;

public class FishManager : MonoBehaviour
{
    public static FishManager Instance { get; private set; }
    public List<FishData> fishTypes; // List of all available fish types

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public (FishData, float) GetFishDataByAngle(float angle)
    {
        FishData selectedFish = null;
        float fishLength = 0f;

        if (angle <= 15f)
        {
            selectedFish = GetFishDataByType("Square Fish");
        }
        else if (angle > 15f && angle <= 25f)
        {
            selectedFish = GetFishDataByType("Cylinder Fish");
        }
        else if (angle > 25f && angle <= 30f)
        {
            selectedFish = GetFishDataByType("Capsule Fish");
        }

        if (selectedFish != null)
        {
            fishLength = Mathf.Round(Random.Range(selectedFish.minLength, selectedFish.maxLength) * 10000f) / 10000f;
        }

        return (selectedFish, fishLength);
    }

    private FishData GetFishDataByType(string fishType)
    {
        foreach (var fishData in fishTypes)
        {
            if (fishData.fishName == fishType)
            {
                return fishData;
            }
        }
        Debug.LogWarning($"Fish type {fishType} not found!");
        return null;
    }
}

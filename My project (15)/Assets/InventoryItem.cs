using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItem : MonoBehaviour
{
    public Image itemImage; // UI Image to show fish sprite
    public TMP_Text sizeLabel; // Text to show size label (S, M, L)

    private FishData fishData; // The fish data associated with this item
    private float length; // The length of the fish
    private InventoryManager inventoryManager; // Reference to the InventoryManager

    // Initializes the item with fish data and length
    public void Initialize(FishData data, float fishLength, InventoryManager manager)
    {
        fishData = data;
        length = fishLength;
        inventoryManager = manager;

        // Set the item image and size label
        itemImage.sprite = fishData.fishSprite;
        sizeLabel.text = fishData.GetSizeLabel(length);
    }

    // Method called when the item is clicked
    public void OnItemClick()
    {
        if (inventoryManager != null)
        {
            inventoryManager.ShowItemDetails(fishData, length);
        }
    }
}

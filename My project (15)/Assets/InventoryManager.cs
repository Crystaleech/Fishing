using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryPanel; // 背包面板
    public GameObject itemSlotPrefab; // 背包格子的预制体
    public Transform itemSlotContainer; // 背包格子容器
    public GameObject itemDetailPanel; // 详细信息面板
    public TMP_Text itemDetailText; // 显示鱼信息的文本
    public Color selectedSlotColor = Color.yellow; // 选中的格子颜色
    public Color defaultSlotColor = Color.white; // 默认格子颜色

    private List<GameObject> itemSlots = new List<GameObject>(); // 所有背包格子实例
    private int selectedIndex = -1; // 当前选中格子的索引
    private bool isInventoryOpen = false; // 是否进入背包模式

    private List<(FishData fishData, float length)> inventoryItems = new List<(FishData, float)>(); // 存储鱼数据和长度

    private void Start()
    {
        // 初始化时隐藏详细信息面板，但确保背包面板始终存在
        itemDetailPanel.SetActive(false);
        inventoryPanel.SetActive(true);

        // 确保背包容器具有 GridLayoutGroup 组件
        if (itemSlotContainer.GetComponent<GridLayoutGroup>() == null)
        {
            itemSlotContainer.gameObject.AddComponent<GridLayoutGroup>();
        }
    }

    void Update()
    {
        // 检测按键以打开/关闭背包
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleInventory();
        }

        // 处理背包导航
        if (isInventoryOpen)
        {
            HandleSlotNavigation();
        }
    }

    public void AddItem(FishData item)
    {
        // 为物品生成随机长度
        float itemLength = Mathf.Round(Random.Range(item.minLength, item.maxLength) * 10000f) / 10000f;
        inventoryItems.Add((item, itemLength));

        // 创建背包格子
        GameObject newItemSlot = Instantiate(itemSlotPrefab, itemSlotContainer);
        Transform fishImageTransform = newItemSlot.transform.Find("FishImage");
        Image fishImage = fishImageTransform != null ? fishImageTransform.GetComponent<Image>() : null;

        // 设置图标
        if (fishImage != null)
        {
            fishImage.sprite = item.fishSprite;
        }

        itemSlots.Add(newItemSlot);

        // 设置大小标识
        TMP_Text sizeText = newItemSlot.GetComponentInChildren<TMP_Text>();
        string sizeLabel = item.GetSizeLabel(itemLength);
        sizeText.text = sizeLabel;

        // 设置点击事件以显示详细信息
        int currentIndex = itemSlots.Count - 1;
        Button itemButton = newItemSlot.GetComponent<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(() => SelectSlot(currentIndex));
        }
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (isInventoryOpen)
        {
            // 锁定游戏其他动作，暂停时间
            Time.timeScale = 0f;
            if (itemSlots.Count > 0)
            {
                SelectSlot(0); // 打开时自动选中第一个格子
            }
            else
            {
                Debug.LogWarning("Inventory is empty. No slots to select.");
                selectedIndex = -1; // 重置索引
                itemDetailPanel.SetActive(false); // 隐藏详细信息面板
            }
        }
        else
        {
            // 恢复游戏，取消锁定
            Time.timeScale = 1f;
            DeselectSlot();
            itemDetailPanel.SetActive(false); // 关闭详细信息面板
        }
    }

    private void HandleSlotNavigation()
    {
        if (itemSlots.Count == 0) return;

        // 键盘导航
        if (Input.GetKeyDown(KeyCode.D))
        {
            SelectSlot((selectedIndex + 1) % itemSlots.Count); // 向右循环
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SelectSlot((selectedIndex - 1 + itemSlots.Count) % itemSlots.Count); // 向左循环
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            int newIndex = selectedIndex + 3;
            if (newIndex < itemSlots.Count)
            {
                SelectSlot(newIndex); // 向下跳一行
            }
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            int newIndex = selectedIndex - 3;
            if (newIndex >= 0)
            {
                SelectSlot(newIndex); // 向上跳一行
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory(); // 退出背包模式
        }
    }

    private void SelectSlot(int index)
    {
        if (itemSlots.Count == 0)
        {
            Debug.LogWarning("No items in the inventory.");
            return;
        }

        if (index < 0 || index >= itemSlots.Count)
        {
            Debug.LogWarning("Index out of range: " + index);
            return;
        }

        // 重置上一个选中格子的颜色
        if (selectedIndex >= 0 && selectedIndex < itemSlots.Count)
        {
            itemSlots[selectedIndex].GetComponent<Image>().color = defaultSlotColor;
        }

        // 更新当前选中格子的颜色
        selectedIndex = index;
        itemSlots[selectedIndex].GetComponent<Image>().color = selectedSlotColor;

        // 更新显示详细信息的面板
        if (selectedIndex < inventoryItems.Count)
        {
            FishData selectedFishData = inventoryItems[selectedIndex].fishData;
            float length = inventoryItems[selectedIndex].length;

            // 更新显示图标
            Transform fishImageTransform = itemSlots[selectedIndex].transform.Find("FishImage");
            if (fishImageTransform != null)
            {
                Image fishImage = fishImageTransform.GetComponent<Image>();
                if (fishImage != null)
                {
                    fishImage.sprite = selectedFishData.fishSprite;
                }
            }

            ShowItemDetails(selectedFishData, length);
        }
    }

    private void DeselectSlot()
    {
        if (selectedIndex >= 0 && selectedIndex < itemSlots.Count)
        {
            itemSlots[selectedIndex].GetComponent<Image>().color = defaultSlotColor; // 重置颜色
        }
        selectedIndex = -1;
    }

    public void ShowItemDetails(FishData item, float length)
    {
        itemDetailPanel.SetActive(true);
        itemDetailText.text = $"Fish Type: {item.fishName}\nLength: {length} cm\nSize: {item.GetSizeLabel(length)}";
    }
}

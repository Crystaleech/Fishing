using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryPanel; // �������
    public GameObject itemSlotPrefab; // �������ӵ�Ԥ����
    public Transform itemSlotContainer; // ������������
    public GameObject itemDetailPanel; // ��ϸ��Ϣ���
    public TMP_Text itemDetailText; // ��ʾ����Ϣ���ı�
    public Color selectedSlotColor = Color.yellow; // ѡ�еĸ�����ɫ
    public Color defaultSlotColor = Color.white; // Ĭ�ϸ�����ɫ

    private List<GameObject> itemSlots = new List<GameObject>(); // ���б�������ʵ��
    private int selectedIndex = -1; // ��ǰѡ�и��ӵ�����
    private bool isInventoryOpen = false; // �Ƿ���뱳��ģʽ

    private List<(FishData fishData, float length)> inventoryItems = new List<(FishData, float)>(); // �洢�����ݺͳ���

    private void Start()
    {
        // ��ʼ��ʱ������ϸ��Ϣ��壬��ȷ���������ʼ�մ���
        itemDetailPanel.SetActive(false);
        inventoryPanel.SetActive(true);

        // ȷ�������������� GridLayoutGroup ���
        if (itemSlotContainer.GetComponent<GridLayoutGroup>() == null)
        {
            itemSlotContainer.gameObject.AddComponent<GridLayoutGroup>();
        }
    }

    void Update()
    {
        // ��ⰴ���Դ�/�رձ���
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleInventory();
        }

        // ����������
        if (isInventoryOpen)
        {
            HandleSlotNavigation();
        }
    }

    public void AddItem(FishData item)
    {
        // Ϊ��Ʒ�����������
        float itemLength = Mathf.Round(Random.Range(item.minLength, item.maxLength) * 10000f) / 10000f;
        inventoryItems.Add((item, itemLength));

        // ������������
        GameObject newItemSlot = Instantiate(itemSlotPrefab, itemSlotContainer);
        Transform fishImageTransform = newItemSlot.transform.Find("FishImage");
        Image fishImage = fishImageTransform != null ? fishImageTransform.GetComponent<Image>() : null;

        // ����ͼ��
        if (fishImage != null)
        {
            fishImage.sprite = item.fishSprite;
        }

        itemSlots.Add(newItemSlot);

        // ���ô�С��ʶ
        TMP_Text sizeText = newItemSlot.GetComponentInChildren<TMP_Text>();
        string sizeLabel = item.GetSizeLabel(itemLength);
        sizeText.text = sizeLabel;

        // ���õ���¼�����ʾ��ϸ��Ϣ
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
            // ������Ϸ������������ͣʱ��
            Time.timeScale = 0f;
            if (itemSlots.Count > 0)
            {
                SelectSlot(0); // ��ʱ�Զ�ѡ�е�һ������
            }
            else
            {
                Debug.LogWarning("Inventory is empty. No slots to select.");
                selectedIndex = -1; // ��������
                itemDetailPanel.SetActive(false); // ������ϸ��Ϣ���
            }
        }
        else
        {
            // �ָ���Ϸ��ȡ������
            Time.timeScale = 1f;
            DeselectSlot();
            itemDetailPanel.SetActive(false); // �ر���ϸ��Ϣ���
        }
    }

    private void HandleSlotNavigation()
    {
        if (itemSlots.Count == 0) return;

        // ���̵���
        if (Input.GetKeyDown(KeyCode.D))
        {
            SelectSlot((selectedIndex + 1) % itemSlots.Count); // ����ѭ��
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SelectSlot((selectedIndex - 1 + itemSlots.Count) % itemSlots.Count); // ����ѭ��
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            int newIndex = selectedIndex + 3;
            if (newIndex < itemSlots.Count)
            {
                SelectSlot(newIndex); // ������һ��
            }
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            int newIndex = selectedIndex - 3;
            if (newIndex >= 0)
            {
                SelectSlot(newIndex); // ������һ��
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory(); // �˳�����ģʽ
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

        // ������һ��ѡ�и��ӵ���ɫ
        if (selectedIndex >= 0 && selectedIndex < itemSlots.Count)
        {
            itemSlots[selectedIndex].GetComponent<Image>().color = defaultSlotColor;
        }

        // ���µ�ǰѡ�и��ӵ���ɫ
        selectedIndex = index;
        itemSlots[selectedIndex].GetComponent<Image>().color = selectedSlotColor;

        // ������ʾ��ϸ��Ϣ�����
        if (selectedIndex < inventoryItems.Count)
        {
            FishData selectedFishData = inventoryItems[selectedIndex].fishData;
            float length = inventoryItems[selectedIndex].length;

            // ������ʾͼ��
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
            itemSlots[selectedIndex].GetComponent<Image>().color = defaultSlotColor; // ������ɫ
        }
        selectedIndex = -1;
    }

    public void ShowItemDetails(FishData item, float length)
    {
        itemDetailPanel.SetActive(true);
        itemDetailText.text = $"Fish Type: {item.fishName}\nLength: {length} cm\nSize: {item.GetSizeLabel(length)}";
    }
}

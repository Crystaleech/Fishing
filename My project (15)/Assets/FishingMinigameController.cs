using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class FishingMinigameController : MonoBehaviour
{
    [Header("Minigame UI Elements")]
    public Image progressBarFill; // Left bar for progress
    public Image fishIcon; // Fish sprite in the right bar
    public Image successZone; // Player's controlled success area in the right bar

    [Header("Fish Counters")]
    public int squareFishCount = 0;
    public int cylinderFishCount = 0;
    public int capsuleFishCount = 0;
    private string currentFishType;

    [Header("Fish Count Display")]
    public TMP_Text squareFishCountText;
    public TMP_Text cylinderFishCountText;
    public TMP_Text capsuleFishCountText;

    [Header("Gameplay Control")]
    public FishingLineController fishingLineController; // Reference to the FishingLineController script
    public GameObject minigamePanel; // Reference to the minigame panel

    public float fishMoveSpeedMin = 270; // ���������С�ƶ��ٶ�
    public float fishMoveSpeedMax = 200f; // �����������ƶ��ٶ�
    public float successZoneSpeed = 100f;
    public float baseProgressDecreaseRate = 20f; // ���ӽ����½�����
    public float squareProgressIncreaseRate = 5f; // ������������
    public float cylinderProgressIncreaseRate = 80f; // Medium fill rate
    public float capsuleProgressIncreaseRate = 60f; // Slowest fill rate

    private RectTransform fishRect;
    private RectTransform successZoneRect;
    private RectTransform progressBarRect;
    private float fishMoveSpeed;
    private float fishProgress = 20f; // ��ʼֵ����Ϊ20
    private float maxProgress;
    private float progressIncreaseRate; // Current increase rate based on fish type
    private bool fishCaught = false;
    private FishingRodController rodController;
    private FishingMinigameController minigameController;
    private float caughtFishLength;



    private float minY = -284f;
    private float maxY = 248f;

    public Action<bool> OnMinigameComplete;

    private void Start()
    {
        rodController = FindObjectOfType<FishingRodController>();
        minigameController = FindObjectOfType<FishingMinigameController>();

        fishRect = fishIcon.GetComponent<RectTransform>();
        successZoneRect = successZone.GetComponent<RectTransform>();
        progressBarRect = progressBarFill.GetComponent<RectTransform>();
        // Set initial position of fish icon
        fishRect.anchoredPosition = new Vector2(fishRect.anchoredPosition.x, -248f);

        fishMoveSpeed = UnityEngine.Random.Range(fishMoveSpeedMin, fishMoveSpeedMax);
        gameObject.SetActive(false); // Hide minigame panel by default
        UpdateFishCountUI();
    }

    private void Update()
    {
        if (fishCaught) return;

        FishRandomMovement();
        ControlSuccessZone();

        // �ڳɹ������ڽ��н��ȸ���
        if (IsFishInSuccessZone())
        {
            // ������������
            fishProgress += progressIncreaseRate * 0.5f * Time.deltaTime; // ÿ���ڳɹ�����Χ�ڵ���������
        }
        else
        {
            // ���ȿ����½�
            fishProgress -= baseProgressDecreaseRate * Time.deltaTime; // ���ȿ����½�
        }

        fishProgress = Mathf.Clamp(fishProgress, 0, maxProgress);
        UpdateProgressBar(fishProgress / maxProgress);

        if (fishProgress >= maxProgress)
        {
            CompleteMinigame(true);
        }
        else if (fishProgress <= 0)
        {
            CompleteMinigame(false);
        }
    }

    private void ControlSuccessZone()
    {
        // �趨�ɹ�������ƶ���Χ
        float successZoneMinY = -247f; // ���óɹ��������СY��λ��
        float successZoneMaxY = 198f; // ���óɹ���������Y��λ��

        if (Input.GetMouseButton(0))
        {
            float newY = successZoneRect.anchoredPosition.y + successZoneSpeed * Time.deltaTime;
            newY = Mathf.Clamp(newY, successZoneMinY, successZoneMaxY); // �������趨�ķ�Χ��
            successZoneRect.anchoredPosition = new Vector2(successZoneRect.anchoredPosition.x, newY);
        }
        else
        {
            float newY = successZoneRect.anchoredPosition.y - successZoneSpeed * Time.deltaTime;
            newY = Mathf.Clamp(newY, successZoneMinY, successZoneMaxY); // �������趨�ķ�Χ��
            successZoneRect.anchoredPosition = new Vector2(successZoneRect.anchoredPosition.x, newY);
        }
    }


    private bool IsFishInSuccessZone()
    {
        float fishY = fishRect.anchoredPosition.y;
        float successZoneMinY = successZoneRect.anchoredPosition.y - (successZoneRect.rect.height / 2);
        float successZoneMaxY = successZoneRect.anchoredPosition.y + (successZoneRect.rect.height / 2);
        return fishY >= successZoneMinY && fishY <= successZoneMaxY;
    }

    private void UpdateProgressBar(float normalizedProgress)
    {
        progressBarFill.fillAmount = normalizedProgress;
    }

    private void ResetMinigame()
    {
        gameObject.SetActive(false); // Hide UI initially
        fishProgress = 20f; // Reset progress to initial value of 20
        fishCaught = false;
        fishMoveSpeed = UnityEngine.Random.Range(fishMoveSpeedMin, fishMoveSpeedMax);
        fishRect.anchoredPosition = new Vector2(fishRect.anchoredPosition.x, minY); // Reset fish position to initial
        UpdateProgressBar(fishProgress / maxProgress);
    }

    public void StartMinigame(float angle)
    {
        // ��ȡ�����ݺ��㳤��
        var (fishData, fishLength) = FishManager.Instance.GetFishDataByAngle(angle);
        if (fishData != null)
        {
            caughtFishLength = fishLength; // ȷ������������������������Դ洢��ǰץ������ĳ���
            SetFishType(fishData);
            gameObject.SetActive(true); // ��ʾС��Ϸ UI
            fishProgress = 20f; // ��ʼ��������Ϊ20
            fishCaught = false;

            // ���� fishIcon λ�ã�ȷ��ÿ��С��Ϸ��ʼʱλ����ȷ
            fishRect.anchoredPosition = new Vector2(fishRect.anchoredPosition.x, minY);

            UpdateProgressBar(fishProgress / maxProgress);

            if (fishingLineController != null)
            {
                fishingLineController.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("Failed to start minigame: No fish data found for the given angle.");
        }
    }



    public void SetFishType(FishData fishData)
    {
        fishIcon.sprite = fishData.fishSprite; // �������ͼ��
        currentFishType = fishData.fishName;

        // ����������͵��� maxProgress �� progressIncreaseRate
        switch (fishData.fishName)
        {
            case "Square Fish":
                maxProgress = 200f; // ���ӷ������������
                progressIncreaseRate = squareProgressIncreaseRate * 0.5f; // ������������
                break;
            case "Cylinder Fish":
                maxProgress = 120f; // �����ʵ�����
                progressIncreaseRate = cylinderProgressIncreaseRate * 0.8f; // ��΢����
                break;
            case "Capsule Fish":
                maxProgress = 150f; // �����ʵ�����
                progressIncreaseRate = capsuleProgressIncreaseRate * 0.8f; // ��΢����
                break;
            default:
                maxProgress = 100f;
                progressIncreaseRate = squareProgressIncreaseRate;
                break;
        }
    }
    private void CompleteMinigame(bool success)
    {
        fishCaught = success;

        if (success)
        {
            switch (currentFishType)
            {
                case "Square Fish":
                    squareFishCount++;
                    break;
                case "Cylinder Fish":
                    cylinderFishCount++;
                    break;
                case "Capsule Fish":
                    capsuleFishCount++;
                    break;
            }
            UpdateFishCountUI();

            // ��¼������������ݵ� GameController
            GameController.Instance.RecordFishCaught(currentFishType, caughtFishLength);
        }

        minigamePanel.SetActive(false);
        StartCoroutine(ReelInCoroutine());
    }

    private IEnumerator ReelInCoroutine()
    {
        float reelInTime = 3f; // �ջ�ʱ��Ϊ3��
        Vector2 targetPosition = new Vector2(fishRect.anchoredPosition.x, minY); // ʹ�� Vector2 ������ Vector3
        float elapsedTime = 0f;

        while (elapsedTime < reelInTime)
        {
            fishRect.anchoredPosition = Vector2.Lerp(fishRect.anchoredPosition, targetPosition, (elapsedTime / reelInTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fishRect.anchoredPosition = targetPosition; // ȷ������λ��
        if (fishingLineController != null)
        {
            fishingLineController.enabled = true; // ���������׸Ϳ���
        }

        OnMinigameComplete?.Invoke(fishCaught); // ֪ͨ������������Ϸ�����

        // ��Э�̽�����������ͼ��λ�ã��Ա��´�С��Ϸ���Դ���ȷλ�ÿ�ʼ
        ResetMinigame(); // �����������ȷ��ͼ��λ�ú�״̬��������
    }

    private void UpdateFishCountUI()
    {
        squareFishCountText.text = "Square Fish: " + squareFishCount;
        cylinderFishCountText.text = "Cylinder Fish: " + cylinderFishCount;
        capsuleFishCountText.text = "Capsule Fish: " + capsuleFishCount;
    }

    private void FishRandomMovement()
    {
        if (fishRect.anchoredPosition.y <= minY || fishRect.anchoredPosition.y >= maxY)
        {
            fishMoveSpeed = UnityEngine.Random.Range(fishMoveSpeedMin, fishMoveSpeedMax) * (fishRect.anchoredPosition.y >= maxY ? -1 : 1);
        }

        float newY = fishRect.anchoredPosition.y + fishMoveSpeed * Time.deltaTime;
        newY = Mathf.Clamp(newY, minY, maxY);
        fishRect.anchoredPosition = new Vector2(fishRect.anchoredPosition.x, newY);
    }

    private void ReleaseCast()
    {
        var (selectedFishData, fishLength) = FishManager.Instance.GetFishDataByAngle(rodController.CurrentAngle);
        if (selectedFishData != null)
        {
            minigameController.StartMinigame(rodController.CurrentAngle);
            minigameController.SetFishType(selectedFishData);
            minigameController.caughtFishLength = fishLength; // ����������ɵ��㳤��
        }
    }
}

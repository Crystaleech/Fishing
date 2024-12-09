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

    public float fishMoveSpeedMin = 270; // 增加鱼的最小移动速度
    public float fishMoveSpeedMax = 200f; // 增加鱼的最大移动速度
    public float successZoneSpeed = 100f;
    public float baseProgressDecreaseRate = 20f; // 增加进度下降速率
    public float squareProgressIncreaseRate = 5f; // 减少增长速率
    public float cylinderProgressIncreaseRate = 80f; // Medium fill rate
    public float capsuleProgressIncreaseRate = 60f; // Slowest fill rate

    private RectTransform fishRect;
    private RectTransform successZoneRect;
    private RectTransform progressBarRect;
    private float fishMoveSpeed;
    private float fishProgress = 20f; // 初始值设置为20
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

        // 在成功区域内进行进度更新
        if (IsFishInSuccessZone())
        {
            // 减少增长幅度
            fishProgress += progressIncreaseRate * 0.5f * Time.deltaTime; // 每次在成功区域范围内的增长减少
        }
        else
        {
            // 进度快速下降
            fishProgress -= baseProgressDecreaseRate * Time.deltaTime; // 进度快速下降
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
        // 设定成功区域的移动范围
        float successZoneMinY = -247f; // 设置成功区域的最小Y轴位置
        float successZoneMaxY = 198f; // 设置成功区域的最大Y轴位置

        if (Input.GetMouseButton(0))
        {
            float newY = successZoneRect.anchoredPosition.y + successZoneSpeed * Time.deltaTime;
            newY = Mathf.Clamp(newY, successZoneMinY, successZoneMaxY); // 限制在设定的范围内
            successZoneRect.anchoredPosition = new Vector2(successZoneRect.anchoredPosition.x, newY);
        }
        else
        {
            float newY = successZoneRect.anchoredPosition.y - successZoneSpeed * Time.deltaTime;
            newY = Mathf.Clamp(newY, successZoneMinY, successZoneMaxY); // 限制在设定的范围内
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
        // 获取鱼数据和鱼长度
        var (fishData, fishLength) = FishManager.Instance.GetFishDataByAngle(angle);
        if (fishData != null)
        {
            caughtFishLength = fishLength; // 确保在类中声明了这个变量，以存储当前抓到的鱼的长度
            SetFishType(fishData);
            gameObject.SetActive(true); // 显示小游戏 UI
            fishProgress = 20f; // 初始进度设置为20
            fishCaught = false;

            // 重置 fishIcon 位置，确保每次小游戏开始时位置正确
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
        fishIcon.sprite = fishData.fishSprite; // 更新鱼的图标
        currentFishType = fishData.fishName;

        // 根据鱼的类型调整 maxProgress 和 progressIncreaseRate
        switch (fishData.fishName)
        {
            case "Square Fish":
                maxProgress = 200f; // 增加方形鱼的最大进度
                progressIncreaseRate = squareProgressIncreaseRate * 0.5f; // 正常增长速率
                break;
            case "Cylinder Fish":
                maxProgress = 120f; // 可以适当增加
                progressIncreaseRate = cylinderProgressIncreaseRate * 0.8f; // 稍微降低
                break;
            case "Capsule Fish":
                maxProgress = 150f; // 可以适当增加
                progressIncreaseRate = capsuleProgressIncreaseRate * 0.8f; // 稍微降低
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

            // 记录钓到的鱼的数据到 GameController
            GameController.Instance.RecordFishCaught(currentFishType, caughtFishLength);
        }

        minigamePanel.SetActive(false);
        StartCoroutine(ReelInCoroutine());
    }

    private IEnumerator ReelInCoroutine()
    {
        float reelInTime = 3f; // 收回时间为3秒
        Vector2 targetPosition = new Vector2(fishRect.anchoredPosition.x, minY); // 使用 Vector2 而不是 Vector3
        float elapsedTime = 0f;

        while (elapsedTime < reelInTime)
        {
            fishRect.anchoredPosition = Vector2.Lerp(fishRect.anchoredPosition, targetPosition, (elapsedTime / reelInTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fishRect.anchoredPosition = targetPosition; // 确保最终位置
        if (fishingLineController != null)
        {
            fishingLineController.enabled = true; // 重新启用抛竿控制
        }

        OnMinigameComplete?.Invoke(fishCaught); // 通知监听者迷你游戏已完成

        // 在协程结束后重置鱼图标位置，以便下次小游戏可以从正确位置开始
        ResetMinigame(); // 在这里调用以确保图标位置和状态都被重置
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
            minigameController.caughtFishLength = fishLength; // 传递随机生成的鱼长度
        }
    }
}

using UnityEngine;
using System.Collections;
using TMPro;

public class FishingLineController : MonoBehaviour
{
    public TMP_Text squareFishCountText;
    public TMP_Text CylinderFishCountText;
    public TMP_Text capsuleFishCountText;
    public Transform rodTip;
    public Transform fishingHook;
    public LineRenderer fishingLine;
    public float maxCastDistance = 20f;
    public float gravity = -9.8f;
    public float baseCastForce = 5f;
    public float forceIncrementPerDegree = 0.5f;
    public float waterSurfaceHeight = 0f;
    public float sinkDepth = 2f;
    public GameObject minigamePanel;

    private bool isCasting = false;
    private bool isFishing = false;
    private bool isPoweringUp = false;
    private bool fishSpawned = false;
    private Vector3 initialVelocity;
    private FishingRodController rodController;
    private Camera mainCamera;
    private FishData currentFish;
    private GameObject currentFishInstance;
    private Coroutine fishEscapeCoroutine;
    public FishingMinigameController minigameController;
    public float resetSpeed = 10f;

    public InventoryManager inventoryManager; // 引用 InventoryManager 脚本


    void Start()
    {
        minigameController.OnMinigameComplete += OnMinigameComplete; // Subscribe to minigame completion event
        fishingLine.positionCount = 0;
        fishingHook.gameObject.SetActive(false);
        rodController = rodTip.GetComponentInParent<FishingRodController>();
        mainCamera = Camera.main;
        UpdateFishCountUI();
        minigamePanel.SetActive(false);
    }

    void Update()
    {
        if (isFishing && Input.GetMouseButtonDown(0) && fishSpawned)
        {
            Debug.Log("Fish catch attempt triggered.");
            HandleFishCatchAttempt();
        }

        if (!enabled) return;

        if (Input.GetMouseButtonDown(0) && !isCasting && !isFishing)
        {
            StartPowerUp();
        }

        if (Input.GetMouseButtonUp(0) && isPoweringUp)
        {
            ReleaseCast();
        }

        UpdateLineRendererPositions();
    }

    private void StartPowerUp()
    {
        isPoweringUp = true;
        fishingLine.positionCount = 0;
        fishingHook.gameObject.SetActive(false);
    }

    private void ReleaseCast()
    {
        isPoweringUp = false;
        isCasting = true;

        float rodAngle = rodController.CurrentAngle;
        float castForce = baseCastForce + rodAngle * forceIncrementPerDegree;

        float angle = 45f * Mathf.Deg2Rad;
        Vector3 forwardDirection = rodTip.forward;
        Vector3 verticalComponent = Vector3.up * Mathf.Sin(angle) * castForce;
        Vector3 horizontalComponent = forwardDirection * Mathf.Cos(angle) * castForce;

        initialVelocity = horizontalComponent + verticalComponent;
        fishingHook.gameObject.SetActive(true);
        fishingLine.positionCount = 2;

        StartCoroutine(CastFishingHook());
    }

    private IEnumerator CastFishingHook()
    {
        Vector3 startPosition = rodTip.position;
        Vector3 position = startPosition;
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime;
            position = startPosition + initialVelocity * time + 0.5f * Vector3.up * gravity * time * time;
            fishingHook.position = position;

            if (position.y <= waterSurfaceHeight - sinkDepth)
            {
                fishingHook.position = new Vector3(position.x, waterSurfaceHeight - sinkDepth, position.z);
                isCasting = false;
                isFishing = true;

                LockRodRotation();
                StartCoroutine(DelayedFishSpawn());
                break;
            }
            yield return null;
        }
    }

    private IEnumerator DelayedFishSpawn()
    {
        float delay = Random.Range(1f, 5f);
        yield return new WaitForSeconds(delay);

        float angle = rodController.CurrentAngle; // 获取当前角度
        currentFishInstance = SpawnFishBasedOnAngle(angle); // 根据角度生成鱼

        if (currentFishInstance != null)
        {
            fishSpawned = true;
            currentFishInstance.transform.SetParent(fishingHook);
            StartFishEscapeTimer();

            // 这里设置 minigame 的鱼类型
            if (minigameController != null)
            {
                minigameController.SetFishType(currentFish); // 设置鱼类型
            }

            Debug.Log("Fish has spawned, initiating camera shake.");
            SimulateCameraShake(); // 屏幕抖动提示
        }
    }

    private GameObject SpawnFishBasedOnAngle(float angle)
    {
        // 解构元组为 fishData 和 fishLength
        var (selectedFishData, fishLength) = FishManager.Instance.GetFishDataByAngle(angle);

        if (selectedFishData != null)
        {
            currentFish = selectedFishData;
            Debug.Log($"Spawning fish: {currentFish.fishName} with length: {fishLength} cm at angle: {angle}"); // 添加长度的 Debug

            // 在生成鱼的实例之前，你可以在这里设置鱼的长度信息（如果需要）
            return Instantiate(selectedFishData.fishPrefab, fishingHook.position, Quaternion.identity);
        }

        Debug.LogWarning("No fish data found for the given angle.");
        return null;
    }


    private void StartFishEscapeTimer()
    {
        if (fishEscapeCoroutine != null)
            StopCoroutine(fishEscapeCoroutine);
        fishEscapeCoroutine = StartCoroutine(FishEscapeTimer(5f));
    }

    private IEnumerator FishEscapeTimer(float responseTime)
    {
        yield return new WaitForSeconds(responseTime);

        if (fishSpawned)
        {
            Destroy(currentFishInstance);
            fishSpawned = false;
            StartReeling();
        }
    }

    public void StartReeling()
    {
        if (isFishing)
            StartCoroutine(ReelInWithFish(currentFish));
        else
            StartCoroutine(ReelInFishingHook());
    }

    private IEnumerator ReelInWithFish(FishData fishData)
    {
        Vector3 targetPosition = rodTip.position;
        while (Vector3.Distance(fishingHook.position, targetPosition) > 0.1f)
        {
            fishingHook.position = Vector3.MoveTowards(fishingHook.position, targetPosition, resetSpeed * Time.deltaTime);
            fishingLine.SetPosition(1, fishingHook.position); // 更新线的位置
            yield return null;
        }

        AddFishToCount(fishData);
        Destroy(currentFishInstance);
        fishingLine.positionCount = 0; // 隐藏鱼线
        fishingHook.gameObject.SetActive(false); // 隐藏钩子
        isFishing = false;

        UnlockRodRotation(); // 解锁杆的旋转
    }

    private IEnumerator ReelInFishingHook()
    {
        Vector3 targetPosition = rodTip.position; // Position to reel back to (rod tip)

        while (Vector3.Distance(fishingHook.position, targetPosition) > 0.1f)
        {
            // Move the hook toward the rod tip smoothly
            fishingHook.position = Vector3.MoveTowards(fishingHook.position, targetPosition, resetSpeed * Time.deltaTime);

            // Update the line renderer to show the line reeling back
            fishingLine.SetPosition(1, fishingHook.position);
            yield return null;
        }

        // Once reeling is complete, hide the hook and line
        fishingLine.positionCount = 0; // Reset line count to hide it
        fishingHook.gameObject.SetActive(false); // Hide the hook
        isFishing = false;

        UnlockRodRotation(); // Unlock rod rotation
    }

    private void UpdateFishCountUI()
    {
        // Update fish count UI text
    }

    private void AddFishToCount(FishData caughtFish)
    {
        // Update fish count logic
    }

    private void LockRodRotation()
    {
        if (rodController != null)
            rodController.enabled = false;
    }

    private void UnlockRodRotation()
    {
        if (rodController != null)
            rodController.enabled = true;
    }

    private void UpdateLineRendererPositions()
    {
        if (fishingLine.positionCount == 2)
        {
            fishingLine.SetPosition(0, rodTip.position);
            fishingLine.SetPosition(1, fishingHook.position);
        }
    }

    private void SimulateCameraShake()
    {
        StartCoroutine(CameraShake(0.2f, 0.1f));
    }

    private void HandleFishCatchAttempt()
    {
        if (!fishSpawned) return; // Return if no fish is spawned

        // Stop the escape timer if it is running
        StopFishEscapeTimer();

        if (Random.value <= 0.1f) // 10% chance to escape
        {
            Debug.Log("Fish escaped!");
            Destroy(currentFishInstance);
            currentFishInstance = null;
            fishSpawned = false;

            // 鱼逃脱后收回鱼线
            StartReeling(); // 调用收线方法
        }
        else
        {
            // Fish didn’t escape, start the minigame and reset the rod
            if (minigameController != null)
            {
                minigameController.SetFishType(currentFish); // Set fish type for minigame
                minigameController.StartMinigame(rodController.CurrentAngle); // Pass the angle instead of fish data
                minigamePanel.SetActive(true); // Display minigame panel
            }
        }
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            mainCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPos;
    }

    private void StopFishEscapeTimer()
    {
        if (fishEscapeCoroutine != null)
        {
            StopCoroutine(fishEscapeCoroutine);
            fishEscapeCoroutine = null;
        }
    }

    private void OnMinigameComplete(bool success)
    {
        // Hide the minigame panel
        minigameController.gameObject.SetActive(false);

        // Destroy the fish instance if it exists
        if (currentFishInstance != null)
        {
            if (success)
            {
                inventoryManager.AddItem(currentFish);
                Debug.Log($"Fish {currentFish.fishName} added to inventory.");
                // 如果成功，先收线
                StartReeling();
            }
            else
            {
                // 如果失败，立即销毁鱼
                Destroy(currentFishInstance);
                currentFishInstance = null;
            }
        }

        // Reset fishing rod and line position immediately
        isFishing = false;
        isCasting = false;
        fishSpawned = false;
        currentFish = null;

        // Unlock the rod rotation to allow casting again
        UnlockRodRotation();

        Debug.Log("Minigame completed. Resetting fishing state and unlocking rod.");
    }

    public void StartFishing()
    {
        // Initial state setup to allow for casting when ready
        isCasting = false;
        isFishing = false;
        fishSpawned = false;

        // 其他初始化代码
        fishingLine.positionCount = 0; // 确保鱼线处于初始状态
        fishingHook.gameObject.SetActive(false); // 确保钩子处于隐藏状态
    }

    public void SetFishType(FishData fishData)
    {
        currentFish = fishData; // Set the current fish to the provided fish data
                                // Additional logic can be added here if needed
    }
}

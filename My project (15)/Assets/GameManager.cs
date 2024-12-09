using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    public GameObject endGamePanel;
    public TMP_Text endGameText;
    public TMP_Text timerText; // 新增倒计时显示的文本
    public float gameDuration = 300f; // Game duration in seconds
    private float timer;
    private int totalFishCaught = 0;
    private float longestFish = 0f;
    private Dictionary<string, int> fishCounts = new Dictionary<string, int>();

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

    private void Start()
    {
        timer = gameDuration;
        endGamePanel.SetActive(false);
        UpdateTimerText(); // 初始化时更新一次倒计时文本
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimerText(); // 更新倒计时文本

            if (timer <= 0)
            {
                EndGame();
            }
        }

        if (endGamePanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        timerText.text = $"Time Left: {minutes:00}:{seconds:00}";
    }

    public void RecordFishCaught(string fishType, float length)
    {
        totalFishCaught++;
        if (length > longestFish)
        {
            longestFish = length;
        }

        if (fishCounts.ContainsKey(fishType))
        {
            fishCounts[fishType]++;
        }
        else
        {
            fishCounts[fishType] = 1;
        }
    }

    private void EndGame()
    {
        Time.timeScale = 0; // Pause the game
        endGamePanel.SetActive(true);
        string fishDetails = $"Total Fish Caught: {totalFishCaught}\nLongest Fish: {longestFish} cm\n";
        foreach (var fishType in fishCounts)
        {
            fishDetails += $"{fishType.Key}: {fishType.Value}\n";
        }
        endGameText.text = fishDetails + "\nPress SPACE to restart.";
    }

    private void RestartGame()
    {
        Time.timeScale = 1; // Resume the game
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}

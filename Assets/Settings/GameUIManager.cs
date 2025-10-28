using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject gameClearUI;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button titleButton;

    [Header("Timer")]
    [SerializeField] private float startTime = 60f;
    public float StartTime => startTime; // Player側から取得できるように

    [Header("Color")]
    [SerializeField] private Color normalColor = Color.white;     // 通常色
    [SerializeField] private Color selectedColor = Color.yellow;  // 選択中の色

    private bool isGameOver = false;
    private bool isGameClear = false;
    private int currentSelected = 0; // 0:リスタート, 1:タイトル

    private void Start()
    {
        gameOverUI?.SetActive(false);
        gameClearUI?.SetActive(false);

        // ボタンイベント登録
        restartButton?.onClick.AddListener(Restart);
        titleButton?.onClick.AddListener(ReturnToTitle);
    }

    private void Update()
    {
        if (isGameOver)
        {
            HandleGameOverInput();
        }
        else if (isGameClear)
        {
            // クリア時は Enter でタイトルに戻る
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ReturnToTitle();
            }
        }
    }

    // --- HP表示更新 ---
    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth}/{maxHealth}";
        }
    }

    // --- タイマー更新 ---
    public void UpdateTimerUI(float currentTime)
    {
        if (timerText != null)
        {
            timerText.text = $"TIME: {currentTime:00}s";
        }
    }

    // --- ゲームクリア処理 ---
    public void GameClear()
    {
        if (isGameClear) return;

        isGameClear = true;

        if (gameClearUI != null)
            gameClearUI.SetActive(true);
    }

    // --- ゲームオーバー処理 ---
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            SelectRestartButton(); // 初期選択状態
        }
    }

    // 最終スコアを表示するメソッド
    public void DisplayFinalScore(int finalScore)
    {
        // ゲームオーバー画面やクリア画面で最終スコアを表示
        if (gameOverUI != null || gameClearUI != null)
        {
            scoreText.text = $"最終スコア: {finalScore}";
        }
    }

    // --- リスタート ---
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- タイトルに戻る ---
    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // ← タイトルシーン名に変更
    }

    // --- 選択操作処理 ---
    private void HandleGameOverInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            SelectRestartButton();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            SelectTitleButton();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentSelected == 0) Restart();
            else ReturnToTitle();
        }
    }

    // --- ボタン選択処理 ---
    private void SelectRestartButton()
    {
        currentSelected = 0;
        restartButton.Select();

        // 色変更
        var restartImage = restartButton.GetComponent<Image>();
        var titleImage = titleButton.GetComponent<Image>();
        if (restartImage != null) restartImage.color = selectedColor;
        if (titleImage != null) titleImage.color = normalColor;
    }

    private void SelectTitleButton()
    {
        currentSelected = 1;
        titleButton.Select();

        // 色変更
        var restartImage = restartButton.GetComponent<Image>();
        var titleImage = titleButton.GetComponent<Image>();
        if (restartImage != null) restartImage.color = normalColor;
        if (titleImage != null) titleImage.color = selectedColor;
    }
}

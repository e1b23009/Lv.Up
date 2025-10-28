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
    public float StartTime => startTime; // Player������擾�ł���悤��

    [Header("Color")]
    [SerializeField] private Color normalColor = Color.white;     // �ʏ�F
    [SerializeField] private Color selectedColor = Color.yellow;  // �I�𒆂̐F

    private bool isGameOver = false;
    private bool isGameClear = false;
    private int currentSelected = 0; // 0:���X�^�[�g, 1:�^�C�g��

    private void Start()
    {
        gameOverUI?.SetActive(false);
        gameClearUI?.SetActive(false);

        // �{�^���C�x���g�o�^
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
            // �N���A���� Enter �Ń^�C�g���ɖ߂�
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ReturnToTitle();
            }
        }
    }

    // --- HP�\���X�V ---
    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth}/{maxHealth}";
        }
    }

    // --- �^�C�}�[�X�V ---
    public void UpdateTimerUI(float currentTime)
    {
        if (timerText != null)
        {
            timerText.text = $"TIME: {currentTime:00}s";
        }
    }

    // --- �Q�[���N���A���� ---
    public void GameClear()
    {
        if (isGameClear) return;

        isGameClear = true;

        if (gameClearUI != null)
            gameClearUI.SetActive(true);
    }

    // --- �Q�[���I�[�o�[���� ---
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            SelectRestartButton(); // �����I�����
        }
    }

    // �ŏI�X�R�A��\�����郁�\�b�h
    public void DisplayFinalScore(int finalScore)
    {
        // �Q�[���I�[�o�[��ʂ�N���A��ʂōŏI�X�R�A��\��
        if (gameOverUI != null || gameClearUI != null)
        {
            scoreText.text = $"�ŏI�X�R�A: {finalScore}";
        }
    }

    // --- ���X�^�[�g ---
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- �^�C�g���ɖ߂� ---
    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // �� �^�C�g���V�[�����ɕύX
    }

    // --- �I�𑀍쏈�� ---
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

    // --- �{�^���I������ ---
    private void SelectRestartButton()
    {
        currentSelected = 0;
        restartButton.Select();

        // �F�ύX
        var restartImage = restartButton.GetComponent<Image>();
        var titleImage = titleButton.GetComponent<Image>();
        if (restartImage != null) restartImage.color = selectedColor;
        if (titleImage != null) titleImage.color = normalColor;
    }

    private void SelectTitleButton()
    {
        currentSelected = 1;
        titleButton.Select();

        // �F�ύX
        var restartImage = restartButton.GetComponent<Image>();
        var titleImage = titleButton.GetComponent<Image>();
        if (restartImage != null) restartImage.color = normalColor;
        if (titleImage != null) titleImage.color = selectedColor;
    }
}

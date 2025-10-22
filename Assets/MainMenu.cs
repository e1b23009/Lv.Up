using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;   // Start�{�^��
    [SerializeField] private Button quitButton;    // Quit�{�^��
    [SerializeField] private Image fadePanel;      // �t�F�[�h�p���p�l��

    private void Start()
    {
        // Start�{�^���ݒ�
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);

        // Quit�{�^���ݒ�
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitGame);

        // �N�����t�F�[�h�C��
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            Color c = fadePanel.color;
            c.a = 1f;
            fadePanel.color = c;
            StartCoroutine(FadeIn());
        }

        if (startButton != null)
        {
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
    }

    // �{�^���ړ�����炷
    public AudioSource uiAudio;
    public AudioClip moveSound, decideSound;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            uiAudio.PlayOneShot(moveSound);

        if (Input.GetKeyDown(KeyCode.Return))
            uiAudio.PlayOneShot(decideSound);
    }

    /// <summary>
    /// Start�{�^�����������Ƃ�
    /// </summary>
    private void OnStartGame()
    {
        Debug.Log("Start button clicked!");
        StartCoroutine(FadeOutAndLoad());
    }

    /// <summary>
    /// Quit�{�^�����������Ƃ�
    /// </summary>
    private void OnQuitGame()
    {
        Debug.Log("Quit Game!");
        StartCoroutine(FadeOutAndQuit());
    }

    /// <summary>
    /// �N�����̃t�F�[�h�C���i���������j
    /// </summary>
    private IEnumerator FadeIn()
    {
        fadePanel.gameObject.SetActive(true);

        float fadeTime = 1.2f; // ���]�ɂ����鎞�ԁi�b�j
        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeTime); // ����1��0��
            fadePanel.color = c;
            yield return null;
        }

        // �Ō�Ɋm���ɓ����ɂ��ď���
        c.a = 0f;
        fadePanel.color = c;
        fadePanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// �Q�[���J�n���̃t�F�[�h�A�E�g�i���������j
    /// </summary>
    private IEnumerator FadeOutAndLoad()
    {
        fadePanel.gameObject.SetActive(true);
        Color c = fadePanel.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            c.a = t;
            fadePanel.color = c;
            yield return null;
        }
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// �I�����̃t�F�[�h�A�E�g����
    /// </summary>
    private IEnumerator FadeOutAndQuit()
    {
        fadePanel.gameObject.SetActive(true);
        Color c = fadePanel.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            c.a = t;
            fadePanel.color = c;
            yield return null;
        }

        // �A�v���I��
        Application.Quit();

#if UNITY_EDITOR
        // Unity�G�f�B�^��ł͍Đ���~
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
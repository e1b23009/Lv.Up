using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;   // Start�{�^��
    [SerializeField] private Image fadePanel;      // �t�F�[�h�p���p�l��

    private void Start()
    {
        // Start�{�^���ɃN���b�N�C�x���g��o�^
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);

        // �t�F�[�h�p�l�����ݒ肳��Ă���ꍇ
        if (fadePanel != null)
        {
            // �N�����͍��p�l����\�����Ă����t�F�[�h�C���J�n
            fadePanel.gameObject.SetActive(true);
            Color c = fadePanel.color;
            c.a = 1f;
            fadePanel.color = c;

            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// �Q�[���J�n���̏����i�t�F�[�h�A�E�g���V�[���J�ځj
    /// </summary>
    private void OnStartGame()
    {
        Debug.Log("Start button clicked!");
        if (fadePanel != null)
            StartCoroutine(FadeOutAndLoad());
        else
            SceneManager.LoadScene("GameScene"); // �t�F�[�h�p�l�����ݒ�ł��J�ډ\
    }

    /// <summary>
    /// �^�C�g���N�����̃t�F�[�h�C���i���������j
    /// </summary>
    private IEnumerator FadeIn()
    {
        float t = 1f;
        Color c = fadePanel.color;
        while (t > 0f)
        {
            t -= Time.deltaTime;       // �t�F�[�h���Ԃ�1�b
            c.a = Mathf.Clamp01(t);
            fadePanel.color = c;
            yield return null;
        }

        fadePanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// �Q�[���J�n���̃t�F�[�h�A�E�g�i���������j��GameScene��
    /// </summary>
    private IEnumerator FadeOutAndLoad()
    {
        fadePanel.gameObject.SetActive(true);
        float t = 0f;
        Color c = fadePanel.color;

        while (t < 1f)
        {
            t += Time.deltaTime;       // �t�F�[�h���Ԃ�1�b
            c.a = Mathf.Clamp01(t);
            fadePanel.color = c;
            yield return null;
        }

        SceneManager.LoadScene("GameScene");
    }
}
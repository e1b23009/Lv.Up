using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSelector : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private RectTransform rect;
    private Image image;

    [Header("Effect Settings")]
    [SerializeField] private float scaleUp = 1.15f;        // �g��{��
    [SerializeField] private float scaleSpeed = 8f;        // �g��X�s�[�h
    [SerializeField] private Color highlightColor = Color.yellow; // �����F
    private Color normalColor;
    private Vector3 originalScale;

    private bool isSelected = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        originalScale = rect.localScale;
        if (image != null)
            normalColor = image.color;
    }

    void Update()
    {
        // �g��E�k���A�j���[�V����
        Vector3 targetScale = isSelected ? originalScale * scaleUp : originalScale;
        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);

        // �F�ύX�i���点��j
        if (image != null)
        {
            Color targetColor = isSelected ? highlightColor : normalColor;
            image.color = Color.Lerp(image.color, targetColor, Time.unscaledDeltaTime * scaleSpeed);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
    }
}
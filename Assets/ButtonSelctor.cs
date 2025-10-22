using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSelector : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private RectTransform rect;
    private Image image;

    [Header("Effect Settings")]
    [SerializeField] private float scaleUp = 1.15f;        // 拡大倍率
    [SerializeField] private float scaleSpeed = 8f;        // 拡大スピード
    [SerializeField] private Color highlightColor = Color.yellow; // 発光色
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
        // 拡大・縮小アニメーション
        Vector3 targetScale = isSelected ? originalScale * scaleUp : originalScale;
        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);

        // 色変更（光らせる）
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
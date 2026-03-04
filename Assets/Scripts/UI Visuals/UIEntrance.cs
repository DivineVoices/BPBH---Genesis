using UnityEngine;

public class UIEntrance : MonoBehaviour
{

    [Header("UI Settings")]
    public RectTransform uiElement;
    [Header("Animation")]
    public float slideSpeed = 5f;
    [Header("Positions")]
    public Vector2 shownPosition;
    private bool isAnimating = true;


    void Start()
    {
        uiElement = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (isAnimating)
        {
            uiElement.anchoredPosition = Vector2.Lerp(uiElement.anchoredPosition, shownPosition, Time.deltaTime * slideSpeed);

            // Stop animating when close enough
            if (Vector2.Distance(uiElement.anchoredPosition, shownPosition) < 0.1f)
            {
                uiElement.anchoredPosition = shownPosition;
                isAnimating = false;
            }
        }
    }
}

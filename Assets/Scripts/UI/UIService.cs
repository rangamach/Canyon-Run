using TMPro;
using UnityEngine;
public class UIService : MonoBehaviour
{
    [Header("GameplayUI")]
    [SerializeField] private ButtonState leftButton; 
    [SerializeField] private ButtonState rightButton;
    [SerializeField] private ButtonState jumpButton;

    [SerializeField] private TextMeshProUGUI fpsText;

    private float deltaTime = 0f; 
    public bool IsLeftPressed() => leftButton.IsPressed;
    public bool IsRightPressed() => rightButton.IsPressed;
    public bool IsJumpPressed() => jumpButton.IsPressed;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        float fps = 1.0f / deltaTime;
        fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
    }
}

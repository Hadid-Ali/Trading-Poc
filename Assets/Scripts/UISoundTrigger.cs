using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to any GameObject with a Button component to auto-wire
/// a ButtonClick SFX through the AudioEvents system.
/// No per-button setup required beyond adding this component.
/// </summary>
[RequireComponent(typeof(Button))]
public class UISoundTrigger : MonoBehaviour
{
    [Tooltip("Override the default ButtonClick sound for this specific button")]
    [SerializeField] private SoundType soundOverride = SoundType.BtnClick;

    private Button _button;

    private void Awake() => _button = GetComponent<Button>();

    private void OnEnable() => _button.onClick.AddListener(OnButtonClicked);

    private void OnDisable() => _button.onClick.RemoveListener(OnButtonClicked);

    /// <summary>Raises the PlaySFX event with the configured sound type.</summary>
    private void OnButtonClicked()
    {
        AudioManager.Instance.PlaySFX(soundOverride);
    }
}

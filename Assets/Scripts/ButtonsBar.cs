using UnityEngine;

public class ButtonsBar : MonoBehaviour
{
    [SerializeField] private BarButtonObject[] _barButtons;

    public void UnFocusAll()
    {
        foreach (var barButton in _barButtons)
        {
            barButton.SetFocus(false);
        }
    }
}

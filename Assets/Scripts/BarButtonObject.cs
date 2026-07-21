using UnityEngine;
using UnityEngine.UI;

public class BarButtonObject : MonoBehaviour
{
    [SerializeField] private GameObject _normalState;
  [SerializeField] private GameObject _focusedState;
  
  [SerializeField] private Button _button;
  [SerializeField] private ButtonsBar _barButtons;

  void Start()
  {
      _button.onClick.AddListener(OnButtonTap);
  }

  private void OnButtonTap()
  {
      _barButtons.UnFocusAll();
      SetFocus(true);
  }
  
  public void SetFocus(bool status)
  {
      _normalState.SetActive(!status);
      _focusedState.SetActive(status);
  }
}

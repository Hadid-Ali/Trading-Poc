using System;
using UnityEngine;

public class HomePanel : MonoBehaviour
{
   [SerializeField] private GameObject _menu;
   
   private void OnEnable()
   {
      HideMenu();
   }

   private void OnDisable()
   {
      HideMenu();
   }

   public void HideMenu()
   {
      _menu.SetActive(false);
   }
}

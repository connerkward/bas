using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Handles swapping between menus.
    /// </summary>
    public class DebugMenus : MonoBehaviour
    {
        [SerializeField] private GameObject menuToggle;
        [SerializeField] private GameObject[] windows;

        public bool MenusOpen => menuToggle.activeInHierarchy;

        private List<object> settingsOldStateCache = new();

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => Tracking4All.Instance);
            OnMenuDropdownValueChanged(0);
        }

        public void OnPressMenuButton()
        {
            if (MenusOpen && settingsOldStateCache.Count > 0)
            {
                if (SettingsManager.Instance.IsSettingStateDirty(settingsOldStateCache))
                {
                    Tracking4All.Instance.RestartSolutions();
                }
            }
            else
            {
                settingsOldStateCache = SettingsManager.Instance.GetCompleteSettingsState();
                SettingsManager.Instance.RefreshDisplay();
            }

            menuToggle.SetActive(!menuToggle.activeSelf);
            SettingsManager.Instance.SaveSettings();
        }

        public void OnMenuDropdownValueChanged(int index)
        {
            foreach (var w in windows)
            {
                w.SetActive(false);
            }

            windows[index].SetActive(true);
        }
    }
}
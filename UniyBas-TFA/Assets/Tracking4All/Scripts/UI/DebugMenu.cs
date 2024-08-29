using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Main debug menu class.
    /// </summary>
    public class DebugMenu : MonoBehaviour
    {
        [SerializeField] private GameObject menuToggle;
        [SerializeField] private GameObject[] windows;

        public bool MenusOpen => menuToggle.activeInHierarchy;

        private List<object> settingsOldState = new();

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => Tracking4All.Instance);
            OnMenuDropdownValueChanged(0);
        }

        public void OnPressMenuButton()
        {
            if (MenusOpen && settingsOldState.Count>0)
            {
                if (SettingsManager.Instance.IsSettingStateDirty(settingsOldState))
                {
                    Tracking4All.Instance.RestartSolutions();
                }
            }
            else
            {
                settingsOldState = SettingsManager.Instance.GetSettingsState();
                SettingsManager.Instance.RefreshDisplay();
            }

            menuToggle.SetActive(!menuToggle.activeSelf);
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
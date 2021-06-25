using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Komodo.IMPRESS 
{
    public class TabManager : MonoBehaviour
    {

        List<TabButton> tabButtons;

        public Sprite tabIdle;

        public Sprite tabHover;

        public Sprite tabActive;

        public Sprite tabHoverActive;

        private TabButton _selectedTab;

        public void Subscribe (TabButton tab) 
        {

            if (tabButtons == null)
            {
                tabButtons = new List<TabButton>();
            }

            tabButtons.Add(tab);
        }

        public void OnTabEnter (TabButton tab) 
        {
            ResetTabs();

            if (_selectedTab == tab) 
            {
                tab.background.sprite = tabHoverActive;

                return;
            }

            tab.background.sprite = tabHover;
        }

        public void OnTabExit (TabButton tab) 
        {
            ResetTabs();

            if (_selectedTab == tab) 
            {
                tab.background.sprite = tabActive;

                return;
            }

            tab.background.sprite = tabIdle;
        }

        public void OnTabToggled (TabButton tab) 
        {
            if (_selectedTab == tab) {

                _selectedTab.Deselect();

                _selectedTab = null;

                ResetTabs();

                tab.background.sprite = tabIdle;

                return;
            }

            tab.Select();

            _selectedTab = tab;

            ResetTabs();

            tab.background.sprite = tabActive;
        }

        public void ResetTabs () 
        {
            foreach (TabButton tab in tabButtons)
            {
                if (tab != _selectedTab) 
                {
                    tab.background.sprite = tabIdle;

                    tab.Deselect();
                }
            }
        }
    }
}
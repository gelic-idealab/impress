using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Komodo.Utilities;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class ImpressEventManager : MonoBehaviour
    {
        public PlayerReferences playerRefs;

        public MainUIReferences uiRefs;

        public Alternate_Button_Function groupButton;

        public Alternate_Button_Function ungroupButton;

        public Alternate_Button_Function sunPointLightButton;

        public Alternate_Button_Function spotLightModelButton;

        public GameObject togglesContainer;

        public GameObject drawTab;

        void OnValidate () 
        {
            if (uiRefs == null) {
                throw new System.NullReferenceException("uiRefs");
            }

            if (playerRefs == null) {
                throw new System.NullReferenceException("playerRefs");
            }

            if (groupButton == null) {
                throw new System.NullReferenceException("groupButton");
            }

            if (ungroupButton == null) 
            {
                throw new System.NullReferenceException("ungroupButton");
            }

            if (sunPointLightButton == null) 
            {
                throw new System.NullReferenceException("sunPointLightButton");
            }
            
            if (spotLightModelButton == null) 
            {
                throw new System.NullReferenceException("spotLightModelButton");
            }
            
            if (togglesContainer == null) 
            {
                throw new System.NullReferenceException("togglesContainer");
            }
            
            if (drawTab == null) 
            {
                throw new System.NullReferenceException("drawTab");
            }
        }

        public void Start()
        {
            if (GameObject.FindGameObjectWithTag("Player").TryGetComponent(out ImpressPlayer player))
            {
                groupButton.onFirstClick.AddListener(() => player.leftTriggerGroup.gameObject.SetActive(true));

                groupButton.onSecondClick.AddListener(() => player.leftTriggerGroup.gameObject.SetActive(false));

                ungroupButton.onFirstClick.AddListener(() => player.leftTriggerUngroup.gameObject.SetActive(true));

                ungroupButton.onSecondClick.AddListener(() => player.leftTriggerUngroup.gameObject.SetActive(false));
            }

            sunPointLightButton.onFirstClick.AddListener(() => LightsManager.Instance.sunPointLightModel.SetActive(true));

            sunPointLightButton.onSecondClick.AddListener(() => LightsManager.Instance.sunPointLightModel.SetActive(false));

            spotLightModelButton.onFirstClick.AddListener(() => LightsManager.Instance.spotLightModel.SetActive(true));

            spotLightModelButton.onSecondClick.AddListener(() => LightsManager.Instance.spotLightModel.SetActive(false));

            TabButton tab = drawTab.GetComponent<TabButton>();

            if (tab) 
            {
                tab.onTabSelected.AddListener(() =>
                {
                    uiRefs.OnDrawButtonFirstClick(playerRefs);
                });

                tab.onTabDeselected.AddListener(() =>
                {
                    uiRefs.OnDrawButtonSecondClick(playerRefs);
                });
            }
        }
    }
}

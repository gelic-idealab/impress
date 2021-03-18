using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Komodo.Utilities;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class MainUIIMPRESSReferences : MonoBehaviour
    {

       // [Header("UIButton Setip")]
        public Alternate_Button_Function groupButton;

        public Alternate_Button_Function makeGroupLinksButton;
        public Alternate_Button_Function removeGroupLinksButton;





        public Alternate_Button_Function primitiveButton;

        public Transform primitiveToggleListParent;

        //light feature setup
        public Alternate_Button_Function lightsButton;
        public Alternate_Button_Function sunPointLightButton;
        public Alternate_Button_Function spotLightModelButton;

        public Transform lightPanelParent;



        public Slider playerScaleSetting;

        public void Start()
        {
            groupButton.gameObject.SetActive(true);
            primitiveButton.gameObject.SetActive(true);
            lightsButton.gameObject.SetActive(true);

            if (GameObject.FindGameObjectWithTag("Player").TryGetComponent(out TeleportPlayer pIM))
            {
                playerScaleSetting.onValueChanged.AddListener((val) => pIM.UpdatePlayerScale(val));

            }

            lightsButton.onFirstClick.AddListener(() => lightPanelParent.gameObject.SetActive(true));
            lightsButton.onSecondClick.AddListener(() => lightPanelParent.gameObject.SetActive(false));


            if (GameObject.FindGameObjectWithTag("Player").TryGetComponent(out PlayerIMPRESSReferences pIR))
            {
                makeGroupLinksButton.onFirstClick.AddListener(() => pIR.leftTriggerLink.gameObject.SetActive(true));
                makeGroupLinksButton.onSecondClick.AddListener(() => pIR.leftTriggerLink.gameObject.SetActive(false));

                removeGroupLinksButton.onFirstClick.AddListener(() => pIR.leftTriggerUnLink.gameObject.SetActive(true));
                removeGroupLinksButton.onSecondClick.AddListener(() => pIR.leftTriggerUnLink.gameObject.SetActive(false));

            }


            //if (GameObject.FindGameObjectWithTag("UIMenu").TryGetComponent(out UI pIR))
            //{
                sunPointLightButton.onFirstClick.AddListener(() => LightsManager.Instance.sunPointLightModel.SetActive(true));
                sunPointLightButton.onSecondClick.AddListener(() => LightsManager.Instance.sunPointLightModel.SetActive(false));

              spotLightModelButton.onFirstClick.AddListener(() => LightsManager.Instance.spotLightModel.SetActive(true));
              spotLightModelButton.onSecondClick.AddListener(() => LightsManager.Instance.spotLightModel.SetActive(false));

            //      }


        }
    }
}

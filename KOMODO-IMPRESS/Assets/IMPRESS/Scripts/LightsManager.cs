using Komodo.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

struct LightProperties
{
   public LightType lightType;
   public bool isRendering;
}


namespace Komodo.IMPRESS
{
    public class LightsManager : SingletonComponent<LightsManager>
    {
        public static LightsManager Instance
        {
            get { return ((LightsManager)_Instance); }
            set { _Instance = value; }
        }

        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;

            //subscribe type and funcion to receive light updates from others
            GlobalMessageManager.Instance.Subscribe("light", (mes) => LightRefresh(mes));
        
        }

        public GameObject spotLightModel;
        public GameObject sunPointLightModel;


        public void ToggleSpotLight(bool toggleOn)
        {
            spotLightModel.SetActive(toggleOn);

            //send networkMessage
            KomodoMessage message = new KomodoMessage("light", JsonUtility.ToJson(new LightProperties { isRendering = toggleOn, lightType = LightType.Spot }));
            message.Send();
        }

        public void ToggleSunPointLightModel(bool toggleOn)
        {
            sunPointLightModel.SetActive(toggleOn);

            //send networkMessage
            KomodoMessage message = new KomodoMessage("light", JsonUtility.ToJson(new LightProperties { isRendering = toggleOn, lightType = LightType.Point }));
            message.Send();
        }



        public void LightRefresh(string message)
        {
            LightProperties lp = JsonUtility.FromJson<LightProperties>(message);

            switch (lp.lightType)
            {
                case LightType.Spot:
                    spotLightModel.SetActive(lp.isRendering);
                    break;

                case LightType.Point:
                    sunPointLightModel.SetActive(lp.isRendering);
                    break;
            }
        }
    }
}

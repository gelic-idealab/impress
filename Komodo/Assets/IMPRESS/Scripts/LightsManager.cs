using Komodo.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

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
        }

        public GameObject spotLightModel;
        public GameObject sunPointLightModel;


    }
}

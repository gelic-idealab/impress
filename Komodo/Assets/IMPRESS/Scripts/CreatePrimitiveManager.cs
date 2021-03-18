using Komodo.Runtime;
using Komodo.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Komodo.IMPRESS
{
    public class CreatePrimitiveManager : SingletonComponent<CreatePrimitiveManager>
    {
        public static CreatePrimitiveManager Instance
        {
            get { return ((CreatePrimitiveManager)_Instance); }
            set { _Instance = value; }
        }


        public Transform primitivesToDisplayParent;

        public Toggle[] toggleButtons;
        public ToggleGroup toggleGroup;
        public Toggle currentToggle;

        EntityManager entityManager;

        Transform primitiveCreationParent;
        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            primitiveCreationParent = new GameObject("Users Primitives").transform;

            TryGetSetPlayerRefences();

        }

        public void TryGetSetPlayerRefences()
        {
            TriggerCreatePrimitive primitiveHandTrigger = default;

            if (GameObject.FindGameObjectWithTag("Player").TryGetComponent(out PlayerReferences pR))
            {
                //set our displays parent to our hand
                primitivesToDisplayParent.SetParent(pR.handL.transform, true);

                //primitiveHandTrigger = pIM

            }

            if (GameObject.FindGameObjectWithTag("Player").TryGetComponent(out PlayerIMPRESSReferences pIM))
            {
                //set our displays parent to our hand
                //  primitivesToDisplayParent.SetParent(pIM.handL.transform, true);

                primitiveHandTrigger = pIM.leftShapeTrigger;

            }


            if (GameObject.FindGameObjectWithTag("MenuUI").TryGetComponent(out MainUIIMPRESSReferences mUIRef))
            {
                toggleButtons =  mUIRef.primitiveToggleListParent.GetComponentsInChildren<Toggle>(true);

                toggleGroup = mUIRef.primitiveToggleListParent.GetComponentInChildren<ToggleGroup>(true);


                //set links
                //for (int i = 0; i < primitivesToDisplayParent.transform.childCount; i++)
                //{
                //    toggleButtons[i].onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); primitivesToDisplayParent.GetChild(i).gameObject.SetActive(true); });
                //}
                toggleButtons[0].onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); primitivesToDisplayParent.GetChild(0).gameObject.SetActive(true); });
                toggleButtons[1].onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); primitivesToDisplayParent.GetChild(1).gameObject.SetActive(true); });
                toggleButtons[2].onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); primitivesToDisplayParent.GetChild(2).gameObject.SetActive(true); });
                toggleButtons[3].onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); primitivesToDisplayParent.GetChild(3).gameObject.SetActive(true); });
                toggleButtons[4].onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); primitivesToDisplayParent.GetChild(4).gameObject.SetActive(true); });



                mUIRef.primitiveButton.onFirstClick.AddListener(() => { primitivesToDisplayParent.gameObject.SetActive(true); primitiveHandTrigger.gameObject.SetActive(true); });
                mUIRef.primitiveButton.onSecondClick.AddListener(() => { primitivesToDisplayParent.gameObject.SetActive(false); primitiveHandTrigger.gameObject.SetActive(false); });


                //foreach (var item in primitivesToDisplayParent.transform)
                //{
                ////    sphereToggle.onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); parentOfPrimitiveObjectsToDisplay.GetChild(0).gameObject.SetActive(true); });
                //}






            }



        }

        public void UpdateCurrentToggleOn(bool state)
        {
            currentToggle = toggleGroup.GetFirstActiveToggle();
        }

        public void DeactivateAllChildren()
        {
            foreach (Transform item in primitivesToDisplayParent)
                item.gameObject.SetActive(false);
        }

        private PrimitiveType currentPrimitiveType;

        public void CreatePrimitive()
        {
            GameObject primitive = default;
            var rot = Quaternion.identity;
            var scale = Vector3.one * 0.2f;

            //foreach (var toggle in toggleButtons)
            //{
            for (int i = 0; i < toggleButtons.Length; i++)
            {

                if (currentToggle.GetInstanceID() == toggleButtons[i].GetInstanceID())
                {
                    //var vals = Enum.GetValues(typeof(PrimitiveType));

                    if(0 == i)
                    currentPrimitiveType = PrimitiveType.Sphere;
                    else if(1 == i)
                    currentPrimitiveType = PrimitiveType.Capsule;
                        else if (2 == i)
                            currentPrimitiveType = PrimitiveType.Cylinder;
                        else if (3 == i)
                            currentPrimitiveType = PrimitiveType.Cube;
                        else if (4 == i)
                            currentPrimitiveType = PrimitiveType.Plane;


                        rot = primitivesToDisplayParent.GetChild(i).rotation;
                    scale = primitivesToDisplayParent.GetChild(i).lossyScale;

                    
                    break;
                }
            }


          
         //   }



            //if (currentToggle.GetInstanceID() == sphereToggle.GetInstanceID())
            //{
            //    currentPrimitiveType = PrimitiveType.Sphere;
            //    rot = parentOfPrimitiveObjectsToDisplay.GetChild(0).rotation;
            //    scale = parentOfPrimitiveObjectsToDisplay.GetChild(0).lossyScale;
            //}
            //else if (currentToggle.GetInstanceID() == capsuleToggle.GetInstanceID())
            //{
            //    currentPrimitiveType = PrimitiveType.Capsule;
            //    rot = parentOfPrimitiveObjectsToDisplay.GetChild(1).rotation;
            //    scale = parentOfPrimitiveObjectsToDisplay.GetChild(1).lossyScale;
            //}
            //else if (currentToggle.GetInstanceID() == CylinderToggle.GetInstanceID())
            //{
            //    currentPrimitiveType = PrimitiveType.Cylinder;
            //    rot = parentOfPrimitiveObjectsToDisplay.GetChild(2).rotation;
            //    scale = parentOfPrimitiveObjectsToDisplay.GetChild(2).lossyScale;
            //}
            //else if (currentToggle.GetInstanceID() == CubeToggle.GetInstanceID())
            //{
            //    currentPrimitiveType = PrimitiveType.Cube;
            //    rot = parentOfPrimitiveObjectsToDisplay.GetChild(3).rotation;
            //    scale = parentOfPrimitiveObjectsToDisplay.GetChild(3).lossyScale;
            //}
            //else if (currentToggle.GetInstanceID() == PlaneToggle.GetInstanceID())
            //{
            //    currentPrimitiveType = PrimitiveType.Plane;
            //    rot = parentOfPrimitiveObjectsToDisplay.GetChild(4).rotation;
            //    scale = parentOfPrimitiveObjectsToDisplay.GetChild(4).lossyScale;
            //}

            primitive = GameObject.CreatePrimitive(currentPrimitiveType);
            NetworkedGameObject nAGO = ClientSpawnManager.Instance.CreateNetworkedGameObject(primitive);

            //tag it to be used with ECS system
            entityManager.AddComponentData(nAGO.Entity, new PrimitiveTag { });
            primitive.tag = "Interactable";
            //   entityManager.AddComponentData(nAGO.Entity, new NetworkEntityIdentificationComponentData { clientID = NetworkUpdateHandler.Instance.client_id, entityID = 0, sessionID = NetworkUpdateHandler.Instance.session_id, current_Entity_Type = Entity_Type.none });


            primitive.transform.position = primitivesToDisplayParent.position;

            primitive.transform.SetGlobalScale(scale);

            primitive.transform.rotation = rot;
            primitive.transform.SetParent(primitiveCreationParent.transform, true);


        }

    }
}

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

        private int primitiveID = 0;
        private int strokeIndex = 0;

        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            primitiveCreationParent = new GameObject("Users Primitives").transform;

            TryGetSetPlayerRefences();


            //register our funcion to call through the network
            GlobalMessageManager.Instance.Subscribe("primitive", (str) => PrimitiveRefresh(str));

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


            
                //sphere
                toggleButtons[0].onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); primitivesToDisplayParent.GetChild(0).gameObject.SetActive(true); });
                toggleButtons[1].onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); primitivesToDisplayParent.GetChild(1).gameObject.SetActive(true); });
                toggleButtons[2].onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); primitivesToDisplayParent.GetChild(2).gameObject.SetActive(true); });
                toggleButtons[3].onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); primitivesToDisplayParent.GetChild(3).gameObject.SetActive(true); });
                toggleButtons[4].onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); primitivesToDisplayParent.GetChild(4).gameObject.SetActive(true); });



                mUIRef.primitiveButton.onFirstClick.AddListener(() => { primitivesToDisplayParent.gameObject.SetActive(true); primitiveHandTrigger.gameObject.SetActive(true); });
                mUIRef.primitiveButton.onSecondClick.AddListener(() => { primitivesToDisplayParent.gameObject.SetActive(false); primitiveHandTrigger.gameObject.SetActive(false); });



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

        public GameObject CreatePrimitive()
        {
            GameObject primitive = default;
            var rot = Quaternion.identity;
            var scale = Vector3.one * 0.2f;

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


          
            primitive = GameObject.CreatePrimitive(currentPrimitiveType);
            
            //add a box collider instead, cant grab objects with different colliders in WebGL build for some reason
            primitive.TryGetComponent<Collider>(out Collider col);
            Destroy(col);
            primitive.AddComponent<BoxCollider>();


            NetworkedGameObject nAGO = ClientSpawnManager.Instance.CreateNetworkedGameObject(primitive);

            //tag it to be used with ECS system
            entityManager.AddComponentData(nAGO.Entity, new PrimitiveTag { });
            primitive.tag = "Interactable";
            //   entityManager.AddComponentData(nAGO.Entity, new NetworkEntityIdentificationComponentData { clientID = NetworkUpdateHandler.Instance.client_id, entityID = 0, sessionID = NetworkUpdateHandler.Instance.session_id, current_Entity_Type = Entity_Type.none });


            primitive.transform.position = primitivesToDisplayParent.position;

            primitive.transform.SetGlobalScale(scale);

            primitive.transform.rotation = rot;
            primitive.transform.SetParent(primitiveCreationParent.transform, true);




            //network call
            primitiveID = 100000000 + 10000000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;

            strokeIndex++;

            var rot2 = primitive.transform.rotation;

            SendPrimitiveNetworkUpdate(primitiveID, (int) currentPrimitiveType, primitive.transform.lossyScale.x, primitive.transform.position,
            new Vector4(rot2.x, rot2.y, rot2.z, rot2.w)
                );



            if (UndoRedoManager.IsAlive)
                //save undoing process for ourselves and others
                UndoRedoManager.Instance.savedStrokeActions.Push(() =>
                {
                    primitive.SetActive(false);

                    //send network update call for everyone else
                    SendPrimitiveNetworkUpdate(primitiveID, -9);
                });

            return primitive;
        }

        public void SendPrimitiveNetworkUpdate(int sID, int primitiveType, float scale = 1, Vector3 primitivePos = default, Vector4 primitiveRot = default)
        {
            var drawUpdate = new Primitive((int)NetworkUpdateHandler.Instance.client_id, sID
               , (int)primitiveType, scale, primitivePos,
               primitiveRot);

            var primSer = JsonUtility.ToJson(drawUpdate);

            KomodoMessage komodoMessage = new KomodoMessage("primitive", primSer);
            komodoMessage.Send();
        }

        public void PrimitiveRefresh(string stringData)
        {
            Primitive newData = JsonUtility.FromJson<Primitive>(stringData);

            //detect if we should render or notrender it
            if (newData.primitiveType == 9)
            {

                if (ClientSpawnManager.Instance.networkedObjectFromEntityId.ContainsKey(newData.primitiveId))
                    ClientSpawnManager.Instance.networkedObjectFromEntityId[newData.primitiveId].gameObject.SetActive(true);


                return;
            }
            else if (newData.primitiveType == -9)
            {

                if (ClientSpawnManager.Instance.networkedObjectFromEntityId.ContainsKey(newData.primitiveId))
                    ClientSpawnManager.Instance.networkedObjectFromEntityId[newData.primitiveId].gameObject.SetActive(false);

                return;
            }




            PrimitiveType primitiveToInstantiate = PrimitiveType.Sphere;

      

        
                switch (newData.primitiveType)
                {
                    case 0:
                        primitiveToInstantiate = PrimitiveType.Sphere;
                        break;
                    case 1:
                        primitiveToInstantiate = PrimitiveType.Capsule;
                        break;
                    case 2:
                        primitiveToInstantiate = PrimitiveType.Cylinder;
                        break;
                    case 3:
                        primitiveToInstantiate = PrimitiveType.Cube;
                        break;
                    case 4:
                        primitiveToInstantiate = PrimitiveType.Plane;
                        break;
                    case 5:
                        primitiveToInstantiate = PrimitiveType.Quad;
                        break;

                   
                }

         //   }


            var primitive = GameObject.CreatePrimitive(primitiveToInstantiate);
            NetworkedGameObject nAGO = ClientSpawnManager.Instance.CreateNetworkedGameObject(primitive);

            //tag it to be used with ECS system
            entityManager.AddComponentData(nAGO.Entity, new PrimitiveTag { });
            primitive.tag = "Interactable";

            var pos = newData.primitivePos;
            var rot = newData.primitiveRot;
            var scale = newData.scale;

            primitive.transform.position = pos;

            primitive.transform.SetGlobalScale(Vector3.one * scale);

            primitive.transform.rotation = new Quaternion(rot.x, rot.y, rot.z, rot.w);
            primitive.transform.SetParent(primitiveCreationParent.transform, true);


        }

    }

    public struct Primitive
    {
        public int clientId;
        public int primitiveId;
        public int primitiveType;
        public float scale;
        public Vector3 primitivePos;
        public Vector4 primitiveRot;

        public Primitive(int clientId, int primitiveId, int primitiveType, float scale, Vector3 primitivePos, Vector4 primitiveRot)
        {
            this.clientId = clientId;
            this.primitiveId = primitiveId;
            this.primitiveType = primitiveType;
            this.scale = scale;
            this.primitivePos = primitivePos;
            this.primitiveRot = primitiveRot;
        }
    }
}

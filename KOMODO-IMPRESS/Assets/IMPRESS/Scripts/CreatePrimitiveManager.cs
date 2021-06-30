using Komodo.Runtime;
using Komodo.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Komodo.IMPRESS
{
    public class CreatePrimitiveManager : SingletonComponent<CreatePrimitiveManager>
    {
        public static CreatePrimitiveManager Instance
        {
            get { return (CreatePrimitiveManager)_Instance; }
            set { _Instance = value; }
        }

        public Transform ghostPrimitivesParent;

        public GameObject ghostSphere;

        public GameObject ghostCapsule;

        public GameObject ghostCylinder;

        public GameObject ghostPlane;

        public GameObject ghostCube;

        public Toggle[] toggleButtons;

        public ToggleGroup toggleGroup;

        public Toggle currentToggle;

        public PlayerReferences playerRefs;

        public ImpressPlayer player;

        EntityManager entityManager;

        Transform primitiveCreationParent;

        private TriggerCreatePrimitive _primitiveAnchor;

        private int _primitiveID = 0;

        private int _strokeIndex = 0;

        private bool _isEnabled;

        private UnityAction _enable;

        private UnityAction _disable;

        private UnityAction _selectSphere;

        private UnityAction _selectCylinder;

        private UnityAction _selectCube;

        private UnityAction _selectPlane;

        private UnityAction _selectCapsule;

        private UnityAction _deselectSphere;

        private UnityAction _deselectCylinder;

        private UnityAction _deselectCube;

        private UnityAction _deselectPlane;

        private UnityAction _deselectCapsule;

        private PrimitiveType _currentType;

        public void OnValidate ()
        {
            if (ghostCapsule == null)
            {
                throw new MissingReferenceException("ghostCapsule");
            }

            if (ghostCube == null)
            {
                throw new MissingReferenceException("ghostCube");
            }
            
            if (ghostCylinder == null)
            {
                throw new MissingReferenceException("ghostCylinder");
            }
            
            if (ghostPlane == null)
            {
                throw new MissingReferenceException("ghostPlane");
            }
            
            if (ghostSphere == null)
            {
                throw new MissingReferenceException("ghostSphere");
            }
        }

        public void Awake()
        {
            _isEnabled = false;

            // force-create an instance
            var initManager = Instance;

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            primitiveCreationParent = new GameObject("CreatedPrimitives").transform;

            InitializeAnchors();

            InitializeListeners();

            GlobalMessageManager.Instance.Subscribe("primitive", (str) => ReceivePrimitiveUpdate(str));
        }

        public void InitializeAnchors ()
        {
            _primitiveAnchor = default;

            //set our display's parent to our hand
            ghostPrimitivesParent.SetParent(playerRefs.handL.transform, true);

            _primitiveAnchor = player.triggerCreatePrimitiveLeft;
        }

        private void _Enable ()
        {
            _isEnabled = true;

            _ToggleGhostPrimitive(true);

            _primitiveAnchor.gameObject.SetActive(true);
        }

        private void _Disable ()
        {
            _isEnabled = false;

            _currentType = default;

            _ToggleGhostPrimitive(false);

            _primitiveAnchor.gameObject.SetActive(false);
        }

        private void _DeselectSphere ()
        {
            ghostSphere.SetActive(false);
        }

        private void _DeselectCapsule ()
        {
            ghostCapsule.SetActive(false);
        }

        private void _DeselectCube ()
        {
            ghostCube.SetActive(false);
        }

        private void _DeselectPlane ()
        {
            ghostPlane.SetActive(false);
        }

        private void _DeselectCylinder ()
        {
            ghostCylinder.SetActive(false);
        }

        private void _SelectSphere ()
        {
            _currentType = PrimitiveType.Sphere;

            ghostSphere.SetActive(true);
        }

        private void _SelectCapsule ()
        {
            _currentType = PrimitiveType.Capsule;

            ghostCapsule.SetActive(true);
        }

        private void _SelectCube ()
        {
            _currentType = PrimitiveType.Cube;

            ghostCube.SetActive(true);
        }

        private void _SelectPlane ()
        {
            _currentType = PrimitiveType.Plane;

            ghostPlane.SetActive(true);
        }

        private void _SelectCylinder ()
        {
            _currentType = PrimitiveType.Cylinder;

            ghostCylinder.SetActive(true);
        }

        public void InitializeListeners ()
        {
            _enable += _Enable;

            ImpressEventManager.StartListening("primitiveTool.enable", _enable);

            _disable += _Disable;

            ImpressEventManager.StartListening("primitiveTool.disable", _disable);

            _selectSphere += _SelectSphere;

            ImpressEventManager.StartListening("primitiveTool.selectSphere", _selectSphere);

            _selectCapsule += _SelectCapsule;

            ImpressEventManager.StartListening("primitiveTool.selectCapsule", _selectCapsule);

            _selectCube += _SelectCube;

            ImpressEventManager.StartListening("primitiveTool.selectCube", _selectCube);

            _selectPlane += _SelectPlane;

            ImpressEventManager.StartListening("primitiveTool.selectPlane", _selectPlane);

            _selectCylinder += _SelectCylinder;

            ImpressEventManager.StartListening("primitiveTool.selectCylinder", _selectCylinder);

            _deselectSphere += _DeselectSphere;

            ImpressEventManager.StartListening("primitiveTool.deselectSphere", _deselectSphere);

            _deselectCapsule += _DeselectCapsule;

            ImpressEventManager.StartListening("primitiveTool.deselectCapsule", _deselectCapsule);

            _deselectCube += _DeselectCube;

            ImpressEventManager.StartListening("primitiveTool.deselectCube", _deselectCube);

            _deselectPlane += _DeselectPlane;

            ImpressEventManager.StartListening("primitiveTool.deselectPlane", _deselectPlane);

            _deselectCylinder += _DeselectCylinder;

            ImpressEventManager.StartListening("primitiveTool.deselectCylinder", _deselectCylinder);
        }

        private void TogglePrimitive (bool state, int index) 
        {
            GetCurrentToggle(state); 

            DeactivateAllChildren(); 

            ghostPrimitivesParent.GetChild(index).gameObject.SetActive(true);

            _primitiveAnchor.gameObject.SetActive(state);
        }

        private void _ToggleGhostPrimitive (bool state)
        {
            ghostPrimitivesParent.gameObject.SetActive(state);
        }

        public void GetCurrentToggle (bool state)
        {
            currentToggle = toggleGroup.GetFirstActiveToggle();
        }

        public void DeactivateAllChildren ()
        {
            foreach (Transform item in ghostPrimitivesParent)
            {
                item.gameObject.SetActive(false);
            }
        }

        public GameObject GetGhostPrimitive (PrimitiveType type)
        {
            if (type == PrimitiveType.Sphere)
            {
                return ghostSphere;
            }

            if (type == PrimitiveType.Cube)
            {
                return ghostCube;
            }

            if (type == PrimitiveType.Capsule)
            {
                return ghostCapsule;
            }

            if (type == PrimitiveType.Cylinder)
            {
                return ghostCylinder;
            }

            if (type == PrimitiveType.Plane)
            {
                return ghostPlane;
            }

            return null;
        }

        public GameObject CreatePrimitive ()
        {
            GameObject primitive = GetGhostPrimitive(_currentType);

            var rot = primitive.transform.rotation;

            var scale = primitive.transform.lossyScale;

            /* TODO(Brandon) - remove
            for (int i = 0; i < toggleButtons.Length; i += 1)
            {
                if (currentToggle.GetInstanceID() == toggleButtons[i].GetInstanceID())
                {
                    //var vals = Enum.GetValues(typeof(PrimitiveType));
                    if(0 == i)
                    _currentType = PrimitiveType.Sphere;

                    else if(1 == i)
                    _currentType = PrimitiveType.Capsule;

                    else if (2 == i)
                        _currentType = PrimitiveType.Cylinder;

                    else if (3 == i)
                        _currentType = PrimitiveType.Cube;
                        
                    else if (4 == i)
                        _currentType = PrimitiveType.Plane;

                    rot = ghostPrimitivesParent.GetChild(i).rotation;

                    scale = ghostPrimitivesParent.GetChild(i).lossyScale;

                    break;
                }
            }
            */

            primitive = GameObject.CreatePrimitive(_currentType);

            //add a box collider instead, cant grab objects with different colliders in WebGL build for some reason
            primitive.TryGetComponent<Collider>(out Collider col);

            Destroy(col);

            primitive.AddComponent<BoxCollider>();

            NetworkedGameObject netObject = ClientSpawnManager.Instance.CreateNetworkedGameObject(primitive);

            //tag it to be used with ECS system
            entityManager.AddComponentData(netObject.Entity, new PrimitiveTag { });

            primitive.tag = "Interactable";

            primitive.transform.position = ghostPrimitivesParent.position;

            primitive.transform.SetGlobalScale(scale);

            primitive.transform.rotation = rot;

            primitive.transform.SetParent(primitiveCreationParent.transform, true);

            _primitiveID = 100000000 + 10000000 + NetworkUpdateHandler.Instance.client_id * 10000 + _strokeIndex;

            _strokeIndex++;

            var rot2 = primitive.transform.rotation;

            SendPrimitiveUpdate(
                _primitiveID, 
                (int) _currentType, 
                primitive.transform.lossyScale.x, 
                primitive.transform.position,
                new Vector4(rot2.x, rot2.y, rot2.z, rot2.w)
            );

            if (UndoRedoManager.IsAlive)
            {
                //save undoing process for ourselves and others
                UndoRedoManager.Instance.savedStrokeActions.Push(() =>
                {
                    primitive.SetActive(false);

                    //send network update call for everyone else
                    SendPrimitiveUpdate(_primitiveID, -9);
                });
            }

            return primitive;
        }

        public void SendPrimitiveUpdate(int sID, int primitiveType, float scale = 1, Vector3 primitivePos = default, Vector4 primitiveRot = default)
        {
            var drawUpdate = new Primitive(
                (int)NetworkUpdateHandler.Instance.client_id, 
                sID, 
                (int)primitiveType,
                scale, 
                primitivePos,
                primitiveRot
            );

            var primSer = JsonUtility.ToJson(drawUpdate);

            KomodoMessage komodoMessage = new KomodoMessage("primitive", primSer);

            komodoMessage.Send();
        }

        public void ReceivePrimitiveUpdate(string stringData)
        {
            Primitive newData = JsonUtility.FromJson<Primitive>(stringData);

            //detect if we should render or notrender it
            if (newData.primitiveType == 9)
            {
                if (ClientSpawnManager.Instance.networkedObjectFromEntityId.ContainsKey(newData.primitiveId))
                {
                    ClientSpawnManager.Instance.networkedObjectFromEntityId[newData.primitiveId].gameObject.SetActive(true);
                }

                return;
            }
            else if (newData.primitiveType == -9)
            {
                if (ClientSpawnManager.Instance.networkedObjectFromEntityId.ContainsKey(newData.primitiveId))
                {
                    ClientSpawnManager.Instance.networkedObjectFromEntityId[newData.primitiveId].gameObject.SetActive(false);
                }

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

            var primitive = GameObject.CreatePrimitive(primitiveToInstantiate);

            NetworkedGameObject nAGO = ClientSpawnManager.Instance.CreateNetworkedGameObject(primitive);

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

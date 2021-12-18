using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;

namespace Komodo.IMPRESS
{
    public class WorldPulling : MonoBehaviour, IUpdatable
    {
        public float scaleMin = 0.835f;

        public float scaleMax = 1.94f;

        public GameObject debugAxes;

        public bool showDebugAxes;

        /* 
            A convenient way to declare a variable that has an initial value and current value.
            To use this, do 

                T varName = new UpdatingValue<T>(initialValueHere); 

            to set the initial value. Then do

                varName.Current = currentValueHere;

            to update it. Then, you can refer to varName.Initial and varName.Current.
        */
        [Serializable]
        public struct UpdatingValue<T>
        {
            public UpdatingValue(T initial)
            {
                Initial = initial;

                Current = initial;
            }

            public T Initial { get; }

            public T Current { get; set; }
        }

        [SerializeField]
        private UpdatingValue<float> playerLocalScaleX;

        [SerializeField]
        private UpdatingValue<float> handDistance;

        [SerializeField]
        private Vector3 initialLeftEyePosition;

        // With respect to the playspace.
        [SerializeField]
        private UpdatingValue<Vector3> handsAverageLocalPosition;

        [SerializeField]
        private UpdatingValue<Quaternion> rotationInInitialPlayspace;

        private LineRenderer handToHandLine;

        public MeshRenderer animalRulerMesh;

        public Transform animalRuler;

        public GameObject physicalFloor;

        public LayerVisibility layerManager;

        //get parent if we are switching objects between hands we want to keep track of were to place it back, to avoid hierachy parenting displacement
        public Transform originalParentOfFirstHandTransform;

        public Transform[] hands = new Transform[2];

        private Transform initialPivotPointInPlayspace;

        public Transform currentPivotPointInPlayspace;

        // Connect this action as a callback in Unity.
        public Action onDoubleTriggerPress;

        // Connect this action as a callback in Unity.
        public Action onDoubleTriggerRelease;

        public Transform player;

        public Transform leftEye;

        public Transform playspace;

        public TeleportPlayer teleportPlayer;

        public Vector3 initialPlayspacePosition;

        public Quaternion initialPlayspaceRotation;

        public float initialScale = 1;

        private GameObject currentPlayspaceObject;

        public Material[] materials;

        private GameObject initialPlayspaceAxes;

        private GameObject currentPlayspaceAxes;

        private Vector3 copyOfInitialPivotPointPosition;

        public void Awake()
        {
            initialPivotPointInPlayspace = new GameObject("InitialPivotPoint").transform;

            initialPivotPointInPlayspace.parent = playspace;

            currentPivotPointInPlayspace = new GameObject("CurrentPivotPoint").transform;

            currentPivotPointInPlayspace.parent = playspace;

            if (!physicalFloor)
            {
                throw new UnassignedReferenceException("physicalFloorReference");
            }

            currentPlayspaceObject = new GameObject();

            initialPlayspacePosition = new Vector3();

            initialPlayspaceRotation = new Quaternion();
        }

        public void Start()
        {
            // This is the playspace.
            if (playspace == null)
            {
                playspace = GameObject.FindGameObjectWithTag("XRCamera").transform;
            }

            player = GameObject.FindGameObjectWithTag("Player").transform;

            if (!leftEye)
            {
                throw new UnassignedReferenceException("leftEye");
            }

            if (!player)
            {
                throw new UnassignedReferenceException("player");
            }

            if (player.TryGetComponent(out PlayerReferences playerRefs))
            {
                hands[0] = playerRefs.handL;
                hands[1] = playerRefs.handR;
            }
            else
            {
                throw new MissingComponentException("PlayerReferences on player");
            }

            teleportPlayer = player.GetComponent<TeleportPlayer>();

            if (!teleportPlayer)
            {
                throw new MissingComponentException("TeleportPlayer on player");
            }

            InitializeDebugAxes();

            handToHandLine = gameObject.GetComponent<LineRenderer>();

            if (!handToHandLine)
            {
                throw new MissingComponentException("LineRenderer on handToHandLine");
            }

            handToHandLine.enabled = false;

            if (!layerManager)
            {
                throw new UnassignedReferenceException("layerManager");
            }

            HidePhysicalFloor();

            if (!animalRulerMesh)
            {
                throw new UnassignedReferenceException("animalRulerMesh");
            }

            animalRulerMesh.material.SetTextureOffset("_MainTex", Vector2.zero);

            if (!animalRuler)
            {
                throw new UnassignedReferenceException("animalRuler");
            }

            animalRuler.gameObject.SetActive(false);

            onDoubleTriggerPress += StartWorldPulling;

            onDoubleTriggerRelease += StopWorldPulling;
        }

        private void InitializeDebugAxes()
        {
            if (!showDebugAxes)
            {
                return;
            }

            var initialPivotPointAxes = Instantiate(debugAxes);

            initialPivotPointAxes.transform.parent = initialPivotPointInPlayspace;

            initialPivotPointAxes.transform.localPosition = Vector3.zero;

            initialPivotPointAxes.transform.localRotation = Quaternion.identity;

            initialPivotPointAxes.transform.GetChild(0).GetComponent<Renderer>().material = materials[0];

            initialPivotPointAxes.transform.GetChild(1).GetComponent<Renderer>().material = materials[0];

            initialPivotPointAxes.transform.GetChild(2).GetComponent<Renderer>().material = materials[0];

            var currentPivotPointAxes = Instantiate(debugAxes);

            currentPivotPointAxes.transform.parent = currentPivotPointInPlayspace;

            currentPivotPointAxes.transform.localPosition = Vector3.zero;

            currentPivotPointAxes.transform.localRotation = Quaternion.identity;

            currentPivotPointAxes.transform.GetChild(0).GetComponent<Renderer>().material = materials[1];

            currentPivotPointAxes.transform.GetChild(1).GetComponent<Renderer>().material = materials[1];

            currentPivotPointAxes.transform.GetChild(2).GetComponent<Renderer>().material = materials[1];

            initialPlayspaceAxes = Instantiate(debugAxes);

            initialPlayspaceAxes.transform.GetChild(0).GetComponent<Renderer>().material = materials[2];

            initialPlayspaceAxes.transform.GetChild(1).GetComponent<Renderer>().material = materials[2];

            initialPlayspaceAxes.transform.GetChild(2).GetComponent<Renderer>().material = materials[2];

            currentPlayspaceAxes = Instantiate(debugAxes);

            currentPlayspaceAxes.transform.parent = playspace;

            currentPlayspaceAxes.transform.GetChild(0).GetComponent<Renderer>().material = materials[3];

            currentPlayspaceAxes.transform.GetChild(1).GetComponent<Renderer>().material = materials[3];

            currentPlayspaceAxes.transform.GetChild(2).GetComponent<Renderer>().material = materials[3];

            var hand0Axes = Instantiate(debugAxes);

            hand0Axes.transform.parent = hands[0];

            hand0Axes.transform.localPosition = Vector3.zero;

            hand0Axes.transform.localRotation = Quaternion.identity;

            hand0Axes.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            hand0Axes.transform.GetChild(0).GetComponent<Renderer>().material = materials[4];

            hand0Axes.transform.GetChild(1).GetComponent<Renderer>().material = materials[4];

            hand0Axes.transform.GetChild(2).GetComponent<Renderer>().material = materials[4];

            var hand1Axes = Instantiate(debugAxes);

            hand1Axes.transform.parent = hands[1];

            hand1Axes.transform.localPosition = Vector3.zero;

            hand1Axes.transform.localRotation = Quaternion.identity;

            hand1Axes.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            hand1Axes.transform.GetChild(0).GetComponent<Renderer>().material = materials[5];

            hand1Axes.transform.GetChild(1).GetComponent<Renderer>().material = materials[5];

            hand1Axes.transform.GetChild(2).GetComponent<Renderer>().material = materials[5];
        }

        private void UpdateDebugAxes ()
        {
            if (!showDebugAxes)
            {
                return;
            }

            initialPlayspaceAxes.transform.position = initialPlayspacePosition;

            initialPlayspaceAxes.transform.rotation = initialPlayspaceRotation;
        }

        [ContextMenu("Start World Pulling")]
        public void StartWorldPulling()
        {
            Debug.Log("started world pulling"); //TODO Remove

            SetInitialValues();

            animalRuler.gameObject.SetActive(true);

            handToHandLine.enabled = true;

            ShowPhysicalFloor();

            layerManager.HideLayers();

            //register our update loop to be called
            if (GameStateManager.IsAlive)
            {
                GameStateManager.Instance.RegisterUpdatableObject(this);
            }
        }

        [ContextMenu("Stop World Pulling")]
        public void StopWorldPulling()
        {
            animalRuler.gameObject.SetActive(false);

            handToHandLine.enabled = false;

            HidePhysicalFloor();

            layerManager.ShowLayers();

            //deregister our update loop not be called when not in use
            if (GameStateManager.IsAlive)
            {
                GameStateManager.Instance.DeRegisterUpdatableObject(this);
            }

            Debug.Log("stopped world pulling"); //TODO Remove
        }

        private void ShowPhysicalFloor ()
        {
            physicalFloor.SetActive(true);
        }

        private void HidePhysicalFloor ()
        {
            physicalFloor.SetActive(false);
        }

        public Vector3 ComputePositionDifference (UpdatingValue<Vector3> handsAveragePosition)
        {
            return handsAveragePosition.Initial - handsAveragePosition.Current;
        }

        public float ComputeScale (UpdatingValue<float> handDistance, UpdatingValue<float> playerLocalScaleX)
        {
            return handDistance.Initial / handDistance.Current * (playerLocalScaleX.Initial * playerLocalScaleX.Current);
        }

        public float ComputeDiffRotationY (Quaternion initial, Quaternion current)
        {
            float result = (current.eulerAngles - initial.eulerAngles).y;

            if (result > -0.001f && result < 0.001f)
            {
                result = 0.0f;
            }

            return result;
        }

        public void UpdateLocalPivotPoint (Transform pivotPointInPlayspace, Vector3 hand0Position, Vector3 hand1Position)
        {
            pivotPointInPlayspace.localPosition = playspace.InverseTransformPoint((hand0Position + hand1Position) / 2);

            Vector3 deltaHandPositionsXZ = new Vector3(
                (hand1Position - hand0Position).x,
                0,
                (hand1Position - hand0Position).z
            );

            pivotPointInPlayspace.localRotation = Quaternion.Inverse(playspace.rotation) * Quaternion.LookRotation(deltaHandPositionsXZ, Vector3.up);
        }

        public void UpdatePlayerPosition (Vector3 teleportLocation)
        {
            var finalPlayspacePosition = playspace.position;

            Vector3 deltaPosition = teleportLocation - leftEye.position;

            finalPlayspacePosition += deltaPosition;

            playspace.position = finalPlayspacePosition;
        }

        public void UpdatePlayspaceRotation (float amount)
        {
            // Copy the transform to a new gameObject.

            Transform currentPlayspace = currentPlayspaceObject.transform;

            currentPlayspace.position = initialPlayspacePosition;

            currentPlayspace.rotation = initialPlayspaceRotation;

            amount *= -1.0f; // Make our own client rotate in the opposite direction that our hands did

            currentPlayspace.RotateAround(
            copyOfInitialPivotPointPosition, Vector3.up, amount);

            playspace.position = currentPlayspace.position;

            playspace.rotation = currentPlayspace.rotation;
        }

        public void UpdateRulerPose (Vector3 hand0Position, Vector3 hand1Position, float scale)
        {
            animalRuler.position = ((hand0Position + hand1Position) / 2);

            animalRuler.localScale = Vector3.one * scale;
        }

        public void UpdateHandToHandLineEndpoints (Vector3 hand0Position, Vector3 hand1Position)
        {
            handToHandLine.SetPosition(0, hand0Position);

            handToHandLine.SetPosition(1, hand1Position);
        }

        public float ComputeRulerValue (float playerScale)
        {
            return (playerScale - 0.9f) / 1.3f;
        }

        public void UpdateRulerValue (float newScaleRatio)
        {
            var rulerValue = ComputeRulerValue(newScaleRatio);

            float min = ComputeRulerValue(scaleMin);

            float max = ComputeRulerValue(scaleMax);

            animalRulerMesh.material.SetTextureOffset("_MainTex", new Vector2(Mathf.Clamp(rulerValue, min, max), 0));
        }

        protected void SetInitialValues ()
        {
            UpdateLocalPivotPoint(initialPivotPointInPlayspace, hands[0].transform.position, hands[1].transform.position);

            copyOfInitialPivotPointPosition = initialPivotPointInPlayspace.position;

            UpdateLocalPivotPoint(currentPivotPointInPlayspace, hands[0].transform.position, hands[1].transform.position);

            // Scale

            playerLocalScaleX = new UpdatingValue<float>(playspace.localScale.x * initialScale);

            handDistance = new UpdatingValue<float>(Vector3.Distance(hands[0].position, hands[1].position));

            // Rotation

            initialPlayspacePosition = playspace.position;

            initialPlayspaceRotation = playspace.rotation;

            UpdateDebugAxes();

            // Position

            initialLeftEyePosition = leftEye.position;

            handsAverageLocalPosition = new UpdatingValue<Vector3>(currentPivotPointInPlayspace.position - playspace.position);
        }

        public void UpdatePlayspaceScale (float scale)
        {
            // TODO: scale around a point here
        }

        public void UpdateLineRenderersScale (float scale)
        {
            // TODO: update size of drawing strokes here 
        }

        public void OnUpdate (float realltime)
        {
            // Rotation

            UpdateLocalPivotPoint(currentPivotPointInPlayspace, hands[0].transform.position, hands[1].transform.position);

            float rotateAmount = ComputeDiffRotationY(initialPivotPointInPlayspace.rotation, currentPivotPointInPlayspace.rotation);

            UpdateDebugAxes();

            UpdatePlayspaceRotation(rotateAmount);

            // Scale

            handDistance.Current = Vector3.Distance(hands[0].transform.position, hands[1].transform.position);

            playerLocalScaleX.Current = playspace.localScale.x;

            float newScale = ComputeScale(handDistance, playerLocalScaleX);

            newScale = Mathf.Clamp(newScale, scaleMin, scaleMax);

            //TODO put back UpdatePlayspaceScale(newScale);

            //TODO put back UpdateLineRenderersScale(newScale);

            // Ruler

            UpdateRulerValue(newScale);

            UpdateRulerPose(hands[0].transform.position, hands[1].transform.position, newScale);

            UpdateHandToHandLineEndpoints(hands[0].transform.position, hands[1].transform.position);

            // Position

            handsAverageLocalPosition.Current = currentPivotPointInPlayspace.position - playspace.position;

            Vector3 newPosition = ComputePositionDifference(handsAverageLocalPosition) + initialLeftEyePosition;

            //TODO put back UpdatePlayerPosition(newPosition);
        }

        public void OnDestroy ()
        {
            Destroy(currentPlayspaceObject);
        }
    }
}





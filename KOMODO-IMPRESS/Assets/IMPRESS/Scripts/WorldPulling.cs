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

        public Material[] debugAxesMaterials;

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
        private UpdatingValue<float> handsRotationY;

        private LineRenderer handToHandLine;

        public MeshRenderer animalRulerMesh;

        public Transform animalRuler;

        public GameObject physicalFloor;

        public LayerVisibility layerManager;

        //get parent if we are switching objects between hands we want to keep track of were to place it back, to avoid hierachy parenting displacement
        public Transform originalParentOfFirstHandTransform;

        public Transform[] hands = new Transform[2];

        public Transform pivotPointsParent;

        public Transform pivotPoint;

        // Connect this action as a callback in Unity.
        public Action onDoubleTriggerPress;

        // Connect this action as a callback in Unity.
        public Action onDoubleTriggerRelease;

        public Transform player;

        public Transform leftEye;

        public Transform playspace;

        public TeleportPlayer teleportPlayer;

        public Quaternion initialPlayspaceRotation;

        public float initialScale = 1;

        public void Awake()
        {
            pivotPoint = new GameObject("WorldPullingPivotPoint").transform;

            if (!physicalFloor)
            {
                throw new UnassignedReferenceException("physicalFloorReference");
            }
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

            var _updatingPivotPointAxes = Instantiate(debugAxes);

            _updatingPivotPointAxes.transform.parent = pivotPoint;

            _updatingPivotPointAxes.transform.localPosition = Vector3.zero;

            _updatingPivotPointAxes.transform.localRotation = Quaternion.identity;

            _updatingPivotPointAxes.transform.GetChild(0).GetComponent<Renderer>().material = debugAxesMaterials[0];

            _updatingPivotPointAxes.transform.GetChild(1).GetComponent<Renderer>().material = debugAxesMaterials[0];

            _updatingPivotPointAxes.transform.GetChild(2).GetComponent<Renderer>().material = debugAxesMaterials[0];

            var _playspaceAxes = Instantiate(debugAxes);

            _playspaceAxes.transform.parent = playspace;

            _playspaceAxes.transform.localPosition = Vector3.zero;

            _playspaceAxes.transform.localRotation = Quaternion.identity;

            _updatingPivotPointAxes.transform.GetChild(0).GetComponent<Renderer>().material = debugAxesMaterials[1];

            _updatingPivotPointAxes.transform.GetChild(1).GetComponent<Renderer>().material = debugAxesMaterials[1];

            _updatingPivotPointAxes.transform.GetChild(2).GetComponent<Renderer>().material = debugAxesMaterials[1];
        }

        [ContextMenu("Start World Pulling")]
        public void StartWorldPulling()
        {
            UpdateInitialValues();

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

        public float ComputeRotationDifference (UpdatingValue<float> handsRotationY)
        {
            return handsRotationY.Initial - handsRotationY.Current;
        }

        public void UpdatePivotPoint (Transform pivotPoint, Vector3 hand0Position, Vector3 hand1Position)
        {
            pivotPoint.position = (hand0Position + hand1Position) / 2;

            Vector3 deltaHandPositionsXZ = new Vector3(
                (hand1Position - hand0Position).x,
                0,
                (hand1Position - hand0Position).z
            );

            Vector3 deltaHandPositionsXZLocal = playspace.InverseTransformDirection(deltaHandPositionsXZ);

            pivotPoint.rotation = Quaternion.LookRotation(deltaHandPositionsXZ, Vector3.up);
        }

        public void UpdatePlayerPosition (Vector3 teleportLocation)
        {
            var finalPlayspacePosition = playspace.position;

            Vector3 deltaPosition = teleportLocation - leftEye.position;

            finalPlayspacePosition += deltaPosition;

            playspace.position = finalPlayspacePosition;
        }

        public void UpdatePlayerRotation (float rotateAmount)
        {
            playspace.rotation = Quaternion.AngleAxis(rotateAmount, Vector3.up) * initialPlayspaceRotation;
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

        protected void UpdateInitialValues ()
        {
            UpdatePivotPoint(pivotPoint, hands[0].transform.position, hands[1].transform.position);

            // Scale

            playerLocalScaleX = new UpdatingValue<float>(playspace.localScale.x * initialScale);

            handDistance = new UpdatingValue<float>(Vector3.Distance(hands[0].position, hands[1].position));

            // Rotation

            initialPlayspaceRotation = playspace.rotation; // TODO REMOVE Quaternion.Inverse(playspace.localRotation) * playspace.localRotation;

            handsRotationY = new UpdatingValue<float>(pivotPoint.eulerAngles.y);

            // Position

            initialLeftEyePosition = leftEye.position;

            handsAverageLocalPosition = new UpdatingValue<Vector3>(pivotPoint.position - playspace.position);
        }

        public void OnUpdateOld(float realltime)
        {
            // Scale

            handDistance.Current = Vector3.Distance(hands[0].transform.position, hands[1].transform.position);

            playerLocalScaleX.Current = playspace.localScale.x;

            float newScale = ComputeScale(handDistance, playerLocalScaleX);

            newScale = Mathf.Clamp(newScale, scaleMin, scaleMax);

            UpdateRulerValue(newScale);

            teleportPlayer.UpdatePlayerScale(newScale);

            // Rotation

            UpdatePivotPoint(pivotPoint, hands[0].transform.position, hands[1].transform.position);

            handsRotationY.Current = pivotPoint.eulerAngles.y;

            float rotateAmount = ComputeRotationDifference(handsRotationY);

            UpdatePlayerRotation(rotateAmount);

            UpdateRulerPose(hands[0].transform.position, hands[1].transform.position, newScale);

            UpdateHandToHandLineEndpoints(hands[0].transform.position, hands[1].transform.position);

            // Position

            handsAverageLocalPosition.Current = pivotPoint.position - playspace.position;

            Vector3 newPosition = ComputePositionDifference(handsAverageLocalPosition) + initialLeftEyePosition;

            UpdatePlayerPosition(newPosition);
        }

        public void OnUpdate(float realltime)
        {
            // Scale

            handDistance.Current = Vector3.Distance(hands[0].transform.position, hands[1].transform.position);

            playerLocalScaleX.Current = playspace.localScale.x;

            float newScale = ComputeScale(handDistance, playerLocalScaleX);

            newScale = Mathf.Clamp(newScale, scaleMin, scaleMax);

            UpdateRulerValue(newScale);

            //TODO put back teleportPlayer.UpdatePlayerScale(newScale);

            // Rotation

            UpdatePivotPoint(pivotPoint, hands[0].transform.position, hands[1].transform.position);

            handsRotationY.Current = pivotPoint.eulerAngles.y;

            float rotateAmount = ComputeRotationDifference(handsRotationY);

            UpdatePlayerRotation(rotateAmount);

            UpdateRulerPose(hands[0].transform.position, hands[1].transform.position, newScale);

            UpdateHandToHandLineEndpoints(hands[0].transform.position, hands[1].transform.position);

            // Position

            handsAverageLocalPosition.Current = pivotPoint.position - playspace.position;

            Vector3 newPosition = ComputePositionDifference(handsAverageLocalPosition) + initialLeftEyePosition;

            //TODO put back UpdatePlayerPosition(newPosition);
        }
    }
}





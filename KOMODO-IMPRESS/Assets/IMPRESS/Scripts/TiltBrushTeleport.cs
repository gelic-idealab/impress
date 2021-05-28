using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;

namespace Komodo.IMPRESS
{
    public class TiltBrushTeleport : MonoBehaviour, IUpdatable
    {
        public float scaleMin = 0.835f;

        public float scaleMax = 1.94f;

        public GameObject debugAxes;

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
        private UpdatingValue<float> handsRotationY;

        public MeshRenderer animalRulerMesh;

        private LineRenderer handToHandLine;

        public Transform animalRuler;
        
        //get parent if we are switching objects between hands we want to keep track of were to place it back, to avoid hierachy parenting displacement
        public Transform originalParentOfFirstHandTransform;

        public Transform[] hands = new Transform[2];

        public Transform pivotPointsParent;

        //coordinate system to use to tilt double grand object appropriately: pulling, pushing, hand lift, and hand lower
        public Transform pivotPoint0;
        
        public bool doUpdateInitialValues;

        // Connect this action as a callback in Unity.
        public Action onDoubleTriggerPress;

        // Connect this action as a callback in Unity.
        public Action onDoubleTriggerRelease;

        Transform player;

        Transform xrPlayer;

        TeleportPlayer teleportPlayer;

        Quaternion initialPlayerRotation;

        public float initialScale = 1;

        public void Awake()
        {
            //create hierarchy to rotate double grab objects appropriately
            //create root parent and share it through scripts by setting it to a static field
            pivotPointsParent = new GameObject("PivotPoints").transform;

            //construct coordinate system to reference for tilting double grab object 
            pivotPoint0 = new GameObject("PivotPoint1").transform;
        }

        public void Start()
        {
            var _debugAxes = Instantiate(debugAxes);

            _debugAxes.transform.parent = pivotPoint0;

            _debugAxes.transform.localPosition = Vector3.zero;

            _debugAxes.transform.localRotation = Quaternion.identity;

            player = GameObject.FindGameObjectWithTag("Player").transform;

            if (player)
            {
                if (player.TryGetComponent(out PlayerReferences pR))
                {
                    hands[0] = pR.handL;
                    hands[1] = pR.handR;
                }

                teleportPlayer = player.GetComponent<TeleportPlayer>();
            }

            handToHandLine = gameObject.GetComponent<LineRenderer>();

            animalRulerMesh.material.SetTextureOffset("_MainTex", Vector2.zero);

            animalRuler.gameObject.SetActive(false);

            handToHandLine.enabled = false;

            onDoubleTriggerPress += StartWorldPulling;

            onDoubleTriggerRelease += StopWorldPulling;
        }

        [ContextMenu("Start World Pulling")]
        public void StartWorldPulling()
        {
            Debug.Log("Starting world pulling");

            doUpdateInitialValues = true;

            animalRuler.gameObject.SetActive(true);

            handToHandLine.enabled = true;

            //register our update loop to be called
            if (GameStateManager.IsAlive)
            {
                GameStateManager.Instance.RegisterUpdatableObject(this);
            }
        }

        [ContextMenu("Stop World Pulling")]
        public void StopWorldPulling()
        {
            Debug.Log("Stopping world pulling");

            animalRuler.gameObject.SetActive(false);

            handToHandLine.enabled = false;

            //deregister our update loop not be called when not in use
            if (GameStateManager.IsAlive)
            {
                GameStateManager.Instance.DeRegisterUpdatableObject(this);
            }
        }

        protected void UpdateInitialValues ()
        {
            playerLocalScaleX = new UpdatingValue<float>(xrPlayer.localScale.x * initialScale);

            initialPlayerRotation = xrPlayer.localRotation * Quaternion.Inverse(xrPlayer.localRotation);

            //grab values to know how we should start affecting our object 
            handDistance = new UpdatingValue<float>(Vector3.Distance(hands[0].position, hands[1].position));

            UpdatePivotPoint(pivotPoint0, hands[0].transform.position, hands[1].transform.position);
            
            handsRotationY = new UpdatingValue<float>(pivotPoint0.localEulerAngles.y);
        }

        public float ComputeScale (UpdatingValue<float> handDistance, UpdatingValue<float> playerLocalScaleX) 
        {
            return (handDistance.Initial / handDistance.Current ) * (playerLocalScaleX.Initial * playerLocalScaleX.Current);
        }

        public float ComputeRotationAmount (UpdatingValue<float> handsRotationY)
        {
            return handsRotationY.Initial - handsRotationY.Current;
        }

        public void UpdatePivotPoint (Transform pivotPoint, Vector3 hand0Position, Vector3 hand1Position)
        {
            pivotPoint.position = hand0Position;

            Vector3 deltaHandPositionsXZ = new Vector3(
                (hand1Position - hand0Position).x,
                0,
                (hand1Position - hand0Position).z
            );

            Vector3 deltaHandPositionsXZLocal = xrPlayer.InverseTransformDirection(deltaHandPositionsXZ);

            pivotPoint.localRotation = Quaternion.LookRotation(deltaHandPositionsXZLocal, Vector3.up);
        }

        public void UpdatePlayerRotation (float rotateAmount) 
        {
            xrPlayer.localRotation = initialPlayerRotation * Quaternion.AngleAxis(rotateAmount, Vector3.up);
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
            return ((playerScale - 0.9f) / 1.3f);
        }

        public void UpdateRulerValue (float newScaleRatio)
        {
            var rulerValue = ComputeRulerValue(newScaleRatio);

            float min = ComputeRulerValue(scaleMin);

            float max = ComputeRulerValue(scaleMax);

            animalRulerMesh.material.SetTextureOffset("_MainTex", new Vector2(Mathf.Clamp(rulerValue, min, max), 0));
        }

        public void OnUpdate(float realltime)
        {
            if (xrPlayer == null)
            {
                xrPlayer = GameObject.FindGameObjectWithTag("XRCamera").transform;
            }

            if (doUpdateInitialValues == true)
            {
                UpdateInitialValues();

                doUpdateInitialValues = false;

                return;
            }

            // Scale

            handDistance.Current = Vector3.Distance(hands[0].transform.position, hands[1].transform.position);

            playerLocalScaleX.Current = xrPlayer.localScale.x;

            float newScale = ComputeScale(handDistance, playerLocalScaleX);

            newScale = Mathf.Clamp(newScale, scaleMin, scaleMax);

            UpdateRulerValue(newScale);

            teleportPlayer.UpdatePlayerScale(newScale);

            // Rotation

            UpdatePivotPoint(pivotPoint0, hands[0].transform.position, hands[1].transform.position);
            
            handsRotationY.Current = pivotPoint0.localEulerAngles.y;

            float rotateAmount = ComputeRotationAmount(handsRotationY);

            UpdatePlayerRotation(rotateAmount);

            UpdateRulerPose(hands[0].transform.position, hands[1].transform.position, newScale);

            UpdateHandToHandLineEndpoints(hands[0].transform.position, hands[1].transform.position);
        }
    }
}





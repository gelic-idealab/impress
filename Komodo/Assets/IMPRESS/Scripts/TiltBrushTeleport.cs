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

        public Vector2 offsetLimits = new Vector2(-0.05f, 0.8f);

        public MeshRenderer animalRulerMesh;

        private LineRenderer handToHandLine;

        public Transform animalRuler;

        //check for left hand grabbing
        public Transform currentEnvironment;
        
        //get parent if we are switching objects between hands we want to keep track of were to place it back, to avoid hierachy parenting displacement
        public Transform originalParentOfFirstHandTransform;

        public Transform[] hands = new Transform[2];

        public Transform pivotPointsParent;

        //coordinate system to use to tilt double grand object appropriately: pulling, pushing, hand lift, and hand lower
        public Transform pivotPoint1;
        
        public bool isInitialDoubleGrab;
        
        private float initialHandDistance;

        private Vector3 initialPlayerScale = Vector3.one;

        private Quaternion initialPlayerRotation;

        public Action onDoubleTriggerPress;

        public Action onDoubleTriggerRelease;

        Transform player;

        Transform xrPlayer;

        TeleportPlayer teleportPlayer;

        public GameObject orientedPlayerTest;

        public float initialY;

        public float initialXRY;

        float currentTurn;

        Quaternion initialRotation;

        Quaternion invertedRot;

        private float currentScale = 1;

        float scaleRatio = 1;
        
        public Transform desktopCamera;

        public void Awake()
        {
            //create hierarchy to rotate double grab objects appropriately
            //create root parent and share it through scripts by setting it to a static field
            pivotPointsParent = new GameObject("PIVOT_ROOT").transform;

            //construct coordinate system to reference for tilting double grab object 
            pivotPoint1 = new GameObject("DoubleGrabCoordinateForObjectTilt").transform;

        }

        public void Start()
        {

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
            isInitialDoubleGrab = false;

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
            animalRuler.gameObject.SetActive(false);

            handToHandLine.enabled = false;

            //deregister our update loop not be called when not in use
            if (GameStateManager.IsAlive)
            {
                GameStateManager.Instance.DeRegisterUpdatableObject(this);
            }
        }

        public float ComputeNewScaleRatio () 
        {
            var handDistance = Vector3.Distance(hands[0].transform.position, hands[1].transform.position);

            scaleRatio = (handDistance / (xrPlayer.localScale.x * initialHandDistance));

            scaleRatio *= initialPlayerScale.x;

            return scaleRatio;
        }

        public void UpdateRotation (float scaleClamp2) 
        {
            pivotPoint1.position = hands[0].transform.position;

            pivotPoint1.rotation = Quaternion.LookRotation(xrPlayer.InverseTransformPoint(hands[1].transform.position - hands[0].transform.position), Vector3.up);

            currentTurn = (pivotPoint1.localEulerAngles.y - initialY);
            
            xrPlayer.localRotation = Quaternion.AngleAxis(currentTurn, Vector3.up) * initialRotation;
        }

        public void UpdateRulerPose (float scale)
        {
            animalRuler.position = ((hands[1].transform.position + hands[0].transform.position) / 2);

            animalRuler.localScale = Vector3.one * scale;

            handToHandLine.SetPosition(0, hands[0].transform.position);

            handToHandLine.SetPosition(1, hands[1].transform.position);
        }

        public void UpdateRulerValue (float newScaleRatio)
        {

            var rulerValue = ((newScaleRatio - 0.9f) / 1.3f);

            animalRulerMesh.material.SetTextureOffset("_MainTex", new Vector2(Mathf.Clamp(rulerValue, offsetLimits.x, offsetLimits.y), 0));
        }

        public void InitializeGrabCoordinates ()
        {
            //grab values to know how we should start affecting our object 
            initialHandDistance = Vector3.Distance(hands[0].position, hands[1].position);

            initialPlayerScale = xrPlayer.localScale.x * (Vector3.one * scaleRatio);

            var initialHandDistance2 = Vector3.Distance(hands[0].transform.position, hands[1].transform.position);

            pivotPoint1.position = hands[0].transform.position; 
            
            pivotPoint1.rotation = Quaternion.LookRotation(xrPlayer.InverseTransformPoint(hands[1].transform.position - hands[0].transform.position), Vector3.up);

            initialRotation = xrPlayer.localRotation * Quaternion.Inverse(xrPlayer.localRotation);
            
            initialY = pivotPoint1.localEulerAngles.y;
        }

        public void OnUpdate(float realltime)
        {
            if (!currentEnvironment)
            {
                currentEnvironment = orientedPlayerTest.transform;
            }

            if (xrPlayer == null)
            {
                xrPlayer = GameObject.FindGameObjectWithTag("XRCamera").transform;
            }

            if (isInitialDoubleGrab == false)
            {
                //run this only once to get initial values to use in update loop
                InitializeGrabCoordinates();

                isInitialDoubleGrab = true;

                return;
            }

            // Scale

            float newScaleRatio = ComputeNewScaleRatio();

            UpdateRulerValue(newScaleRatio);

            if (float.IsNaN(currentEnvironment.localScale.y)) 
            {
                return;
            }

            var scaleClamp2 = Mathf.Clamp(newScaleRatio, 0.35f, 5f);

            teleportPlayer.UpdatePlayerScale(scaleClamp2);

            // Rotation

            UpdateRotation(scaleClamp2);

            UpdateRulerPose(scaleClamp2);
        }
    }
}





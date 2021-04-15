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
        public MeshRenderer animalScaleMaterial;

        private LineRenderer lineRenderer;
        public Transform animalRuler;

        //check for left hand grabbing
        public Transform currentEnvironment;
        //get parent if we are switching objects between hands we want to keep track of were to place it back, to avoid hierachy parenting displacement
        public Transform originalParentOfFirstHandTransform;

        //do the same as above for the right Hand
        public Transform secondObjectGrabbed;
        public Transform originalParentOfSecondHandTransform;

        public Transform[] handReferences = new Transform[2];//(2, Allocator.Persistent);

        //away to keep track if we are double grabbing a gameobject to call event;
        public bool isDoubleGrabbing;

        //Fields to rotate object appropriately
        //Hierarchy used to set correct Pivot points for scalling and rotating objects on DoubleGrab
        public Transform pivotRootTransform;             ///PARENT SCALE PIVOT1 CONTAINER
       // private static Transform pivotChildTransform;             ///-CHILD SCALE PIVOT2 CONTAINER
        //public Transform doubleGrabRotationTransform;       //--Child for rotations

       // public Transform handParentForContainerPlacement;

        //coordinate system to use to tilt double grand object appropriately: pulling, pushing, hand lift, and hand lower
        public Transform doubleGrabRotateCoordinate;

        //initial Data when Double Grabbing -scalling and rotation 
        public bool isInitialDoubleGrab;
        private Quaternion doubleGrabInitialRotation;
        private float doubleGrabinitialDistance;
        private Vector3 doublGrabinitialScale = Vector3.one;
        private Quaternion initialPlayerRotation;
    

        //away to keep track if we are double grabbing a gameobject to call event;
        public Action onDoubleTriggerPress;
        public Action onDoubleTriggerRelease;

        public void Awake()
        {
            //create hierarchy to rotate double grab objects appropriately
            //create root parent and share it through scripts by setting it to a static field
            pivotRootTransform = new GameObject("PIVOT_ROOT").transform;
        
            //construct coordinate system to reference for tilting double grab object 
            doubleGrabRotateCoordinate = new GameObject("DoubleGrabCoordinateForObjectTilt").transform;
         
        }

        Transform player;
        Transform xrPlayer;

        TeleportPlayer teleportPlayer;

        public Vector3 orientedPlayer;
        public GameObject orientedPlayerTest;
        public void Start()
        {

            player = GameObject.FindGameObjectWithTag("Player").transform;

            if (player)
            {
                if (player.TryGetComponent(out PlayerReferences pR))
                {
                    handReferences[0] = pR.handL;
                    handReferences[1] = pR.handR;
                }

                teleportPlayer = player.GetComponent<TeleportPlayer>();
            }


            lineRenderer = gameObject.GetComponent<LineRenderer>();

            animalScaleMaterial.material.SetTextureOffset("_MainTex", Vector2.zero);

            animalRuler.gameObject.SetActive(false);
            lineRenderer.enabled = false;// (false);
           

            onDoubleTriggerPress += SetDoubleTriggerStateOn;
            onDoubleTriggerRelease += SetDoubleTriggerStateOff;//  .AddListener(() => SetDoubleTriggerStateOff());

        }




        [ContextMenu("Double Grab State ON")]
        public void SetDoubleTriggerStateOn()
        {
            isInitialDoubleGrab = false;

            animalRuler.gameObject.SetActive(true);
            lineRenderer.enabled = true;

            //register our update loop to be called
            if (GameStateManager.IsAlive)
                GameStateManager.Instance.RegisterUpdatableObject(this);
        }

        [ContextMenu("Double Grab State OFF")]
        public void SetDoubleTriggerStateOff()
        {
            animalRuler.gameObject.SetActive(false);
            lineRenderer.enabled = false;


          //  ffController.enabled = true;
            //deregister our update loop not be called when not in use
            if (GameStateManager.IsAlive)
                GameStateManager.Instance.DeRegisterUpdatableObject(this);
        }


        // public FreeFlightController ffController;
        public Quaternion rotationToRotateTo;
        public float initialY;
        public float initialXRY;

        float currentTurn;
        Quaternion initialRot;
        Quaternion invertedRot;
        private float currentScale = 1;
        float newinvertedscaleRatioBasedOnDistance = 1;

        public void OnUpdate(float realltime)
        {
            if (!currentEnvironment)
                currentEnvironment = orientedPlayerTest.transform; //GameObject.Find("Environment").transform;

            if (xrPlayer == null)
                xrPlayer = GameObject.FindGameObjectWithTag("XRCamera").transform;

            #region DoubleHand Funcionality


            if (isInitialDoubleGrab == false)
            {
                //inficates to run this only once at start to get initial values to use in update loop
                isInitialDoubleGrab = true;

                //grab values to know how we should start affecting our object 
                doubleGrabinitialDistance =  Vector3.Distance(handReferences[0].position, handReferences[1].position);


                doublGrabinitialScale = xrPlayer.localScale.x * (Vector3.one * newinvertedscaleRatioBasedOnDistance) ;//xrPlayer.localScale;
              //  doublGrabinitialScale *= 2;

                var handDistance2 = Vector3.Distance(handReferences[0].transform.position, handReferences[1].transform.position);
             


                doubleGrabRotateCoordinate.position = handReferences[0].transform.position; //(handReferences[0].transform.position + handReferences[1].transform.position) / 2;
                doubleGrabRotateCoordinate.rotation = Quaternion.LookRotation(xrPlayer.InverseTransformPoint(handReferences[1].transform.position - handReferences[0].transform.position), Vector3.up);//Quaternion.LookRotation(handReferences[1].transform.position - handReferences[0].transform.position, Vector3.up);


                initialRot = xrPlayer.localRotation * Quaternion.Inverse(xrPlayer.localRotation);// xrPlayer.localRotation; //  Quaternion.Inverse(xrPlayer.rotation);
            //    invertedRot = Quaternion.Inverse(xrPlayer.rotation);
                initialY = doubleGrabRotateCoordinate.localEulerAngles.y;
               // initialXRY = xrPlayer.localEulerAngles.y;

                return;
            }

            //scale
            var handDistance = Vector3.Distance(handReferences[0].transform.position, handReferences[1].transform.position);

            newinvertedscaleRatioBasedOnDistance = (handDistance / (xrPlayer.localScale.x * doubleGrabinitialDistance));

            newinvertedscaleRatioBasedOnDistance *= doublGrabinitialScale.x;
            //Debug.Log((newinvertedscaleRatioBasedOnDistance - 0.5f) / 1.3f);

            var rulerValue = ((newinvertedscaleRatioBasedOnDistance - 0.9f) / 1.3f); // Mathf.Max(, Mathf.Max(,))

            animalScaleMaterial.material.SetTextureOffset("_MainTex", new Vector2(Mathf.Clamp(rulerValue, offsetLimits.x, offsetLimits.y), 0));

            if (float.IsNaN(currentEnvironment.localScale.y)) return;

            //var tapeMeasureConv = (newinvertedscaleRatioBasedOnDistance - 0.5f) / 1.3f;

            var scaleClamp2 = Mathf.Clamp(newinvertedscaleRatioBasedOnDistance, 0.35f, 5f);

        //    var scaleClamp2 = Mathf.Clamp((5 * (tapeMeasureConv / 0.8f)) - 1.4f, 0.35f, 5f);

            teleportPlayer.UpdatePlayerScale(scaleClamp2 );




            ///rotate

            doubleGrabRotateCoordinate.position = handReferences[0].transform.position;

            doubleGrabRotateCoordinate.rotation = Quaternion.LookRotation(xrPlayer.InverseTransformPoint(handReferences[1].transform.position - handReferences[0].transform.position), Vector3.up);

            currentTurn = (doubleGrabRotateCoordinate.localEulerAngles.y - initialY);
            // Debug.Log(currentTurn - initialY);
                 xrPlayer.localRotation =Quaternion.AngleAxis(currentTurn, Vector3.up) * initialRot;
         //   xrPlayer.localRotation = Quaternion.FromToRotation(xrPlayer.localEulerAngles, doubleGrabRotateCoordinate.localEulerAngles);
            //     teleportPlayer.SetXRAndSpectatorRotation((initialRot) * Quaternion.AngleAxis(currentTurn - initialY, Vector3.up));





            animalRuler.position = ((handReferences[1].transform.position + handReferences[0].transform.position) / 2);
            animalRuler.localScale = Vector3.one * scaleClamp2;

            lineRenderer.SetPosition(0, handReferences[0].transform.position);
            lineRenderer.SetPosition(1, handReferences[1].transform.position);



            #endregion
        }
        public Transform desktopCamera;
    }
}





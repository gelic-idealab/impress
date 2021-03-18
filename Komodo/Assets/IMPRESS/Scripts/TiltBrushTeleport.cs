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
        //public bool isBothHandsClicked;
        //public Transform handL;
        //public Transform handR;

        //private Vector3 midLocation;
        //public Transform currentEnvironment;
        //public TeleportPlayer teleportPlayer;

        //public void Start()
        //{
        //    SceneManagerExtensions.Instance.onSceneChange += GetAndSetSceneInfo;
        //}


        //public void GetAndSetSceneInfo()
        //{
        // //   currentEnvironment = GameObject.Find("Environment");


        //}
        //// Update is called once per frame
        //void Update()
        //{
        //    if (!isBothHandsClicked)
        //        return;

        //    midLocation = (handL.position + handR.position) / 2;

        //    teleportPlayer.SetXRAndSpectatorRotation
        //}
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
        private static Transform pivotChildTransform;             ///-CHILD SCALE PIVOT2 CONTAINER
        public Transform doubleGrabRotationTransform;       //--Child for rotations

        public Transform handParentForContainerPlacement;

        //coordinate system to use to tilt double grand object appropriately: pulling, pushing, hand lift, and hand lower
        public Transform doubleGrabRotateCoordinate;

        //initial Data when Double Grabbing -scalling and rotation 
        public bool isInitialDoubleGrab;
        private Quaternion doubleGrabInitialRotation;
        private float doubleGrabinitialDistance;
        private Vector3 doublGrabinitialScale;
        private Vector3 initialOffsetFromHandToGrabbedObject;
        private Quaternion initialPlayerRotation;
        private float initialScaleRatioBasedOnDistance;
        float initialZCoord;
        float initialYCoord;

        //away to keep track if we are double grabbing a gameobject to call event;
        public Action onDoubleTriggerPress;
        public Action onDoubleTriggerRelease;

        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            //  var initManager = Instance;

            //create hierarchy to rotate double grab objects appropriately
            //create root parent and share it through scripts by setting it to a static field
            pivotRootTransform = new GameObject("PIVOT_ROOT").transform;
            ////place object one level up from hand to avoid getting our hand rotations
            pivotRootTransform.parent = transform.parent;

            //construct coordinate system to reference for tilting double grab object 
            doubleGrabRotateCoordinate = new GameObject("DoubleGrabCoordinateForObjectTilt").transform;
            doubleGrabRotateCoordinate.SetParent(transform.root.parent, true);
            doubleGrabRotateCoordinate.localPosition = Vector3.zero;

            //parent used to set secondary hand pivot for scalling objects properly
            pivotChildTransform = new GameObject("Pivot Point 2 Parent").transform;
            pivotChildTransform.SetParent(pivotRootTransform, true);
            pivotChildTransform.localPosition = Vector3.zero;

            //parent used for rotating or doble grab object
            doubleGrabRotationTransform = new GameObject("Rotation Parent_3").transform;
            doubleGrabRotationTransform.SetParent(pivotChildTransform, true);
            doubleGrabRotationTransform.localPosition = Vector3.zero;
        }

        Transform player;
        Transform xrPlayer;

        TeleportPlayer teleportPlayer;

        public void Start()
        {
            //if (GameStateManager.IsAlive)
            //    GameStateManager.Instance.RegisterUpdatableObject(this);

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

            //set references for parent
            handParentForContainerPlacement = pivotRootTransform.parent; //player.GetChild(0).transform.rotation

            lineRenderer = gameObject.GetComponent<LineRenderer>();

            animalScaleMaterial.material.SetTextureOffset("_MainTex", Vector2.zero);

            animalRuler.gameObject.SetActive(false);
            lineRenderer.enabled = false;// (false);
            //       off = animalScaleMaterial.material.GetTextureOffset("_MainTex");

            SceneManager.sceneLoaded += GetAndSetSceneInfo;
            // SceneManagerExtensions.Instance.sceneManager.onSceneChange += 

            onDoubleTriggerPress += SetDoubleTriggerStateOn;
            //AddListener(() => SetDoubleTriggerStateOn());
            onDoubleTriggerRelease += SetDoubleTriggerStateOff;//  .AddListener(() => SetDoubleTriggerStateOff());

        }

        //public Transform currentEnvironment;
        public void GetAndSetSceneInfo(Scene sce,  LoadSceneMode lsm)
        {
            currentEnvironment = GameObject.Find("Environment").transform;
        }


        [ContextMenu("Double Grab State ON")]
        public void SetDoubleTriggerStateOn()
        {
            // isDoubleGrabbing = true;
            animalRuler.gameObject.SetActive(true);
            lineRenderer.enabled = true;

            //register our update loop to be called
            if (GameStateManager.IsAlive)
                GameStateManager.Instance.RegisterUpdatableObject(this);
        }

        [ContextMenu("Double Grab State OFF")]
        public void SetDoubleTriggerStateOff()
        {
            //   isDoubleGrabbing = false;
            animalRuler.gameObject.SetActive(false);
            lineRenderer.enabled = false;

            //deregister our update loop not be called when not in use
            if (GameStateManager.IsAlive)
                GameStateManager.Instance.DeRegisterUpdatableObject(this);
        }

     

        public void OnUpdate(float realltime)
        {
            if (!currentEnvironment)
                currentEnvironment = GameObject.Find("Environment").transform;

            #region DoubleHand Funcionality

            // Debug.Log("outside");
            //Called every update when grabbing same item
            //if (isDoubleGrabbing)
            //{
            //if (currentEnvironment == null)
            //{
            //    isDoubleGrabbing = false;
            //    return;
            //}
            //set values 
            if (isInitialDoubleGrab == false)
            {
                //inficates to run this only once at start to get initial values to use in update loop
                isInitialDoubleGrab = true;

                //grab values to know how we should start affecting our object 
                doubleGrabinitialDistance = Vector3.Distance(handReferences[0].position, handReferences[1].position);
                doublGrabinitialScale = pivotRootTransform.localScale;
                pivotChildTransform.rotation = handParentForContainerPlacement.rotation;

                //reset values for our container objects that we use to deform and rotate objects
                doubleGrabRotationTransform.rotation = Quaternion.identity;
                pivotRootTransform.localScale = Vector3.one;

                //set reference vector to tilt our grabed object on - left hand looks at right and sets tilt according to movement of origin or lookat target 
                doubleGrabRotateCoordinate.LookAt((handReferences[1].transform.position - handReferences[0].transform.position), Vector3.up);

                //Get the inverse of the initial rotation to use in update loop to avoid moving the object when grabbing   
                doubleGrabInitialRotation = Quaternion.Inverse(doubleGrabRotateCoordinate.rotation * handParentForContainerPlacement.rotation);

                //get rotational difference to be able to offset it apropriately in update loop
                var tiltRotation = doubleGrabInitialRotation * doubleGrabRotateCoordinate.rotation;

                //our initial orientation to use to tilt object, due to the way lookat behavior behaves we have to set x as Z 
                initialZCoord = tiltRotation.eulerAngles.x - doubleGrabRotationTransform.transform.eulerAngles.x;
                initialYCoord = tiltRotation.eulerAngles.y - doubleGrabRotationTransform.transform.eulerAngles.y;

                ////to fix parenting scalling down issue between centerpoint of hands and object
                initialOffsetFromHandToGrabbedObject = currentEnvironment.position - ((handReferences[1].transform.position + handReferences[0].transform.position) / 2);// - handParentForContainerPlacement.position;

                //pick up the rotation of our client to know when to update our offsets from hands to grab object
                initialPlayerRotation = handParentForContainerPlacement.rotation;
                return;
            }
            var handDistance = Vector3.Distance(handReferences[0].transform.position, handReferences[1].transform.position);

            //a ratio between our current distance divided by our initial distance
            var invertedscaleRatioBasedOnDistance = (handDistance / doubleGrabinitialDistance);

            //modify to have scalling consistent with animal ruller
            var originalScaleRatioBasedOnDistance = (doubleGrabinitialDistance / (handDistance * 5.5f));


            animalScaleMaterial.material.SetTextureOffset("_MainTex", new Vector2(Mathf.Clamp((invertedscaleRatioBasedOnDistance - 0.5f)/1.2f, offsetLimits.x, offsetLimits.y), 0));


            if (float.IsNaN(currentEnvironment.localScale.y)) return;

            //we multiply our ratio with our initial scale
            //set limits on our scalling the player

            var scaleClamp = Mathf.Clamp((doublGrabinitialScale.x * originalScaleRatioBasedOnDistance ),  0.1f, 3f);
            pivotRootTransform.localScale = scaleClamp * Vector3.one;


        //    teleportPlayer.UpdatePlayerScale(scaleClamp);



            //place our grabbed object and second pivot away from the influeces of scale and rotation at first
            currentEnvironment.SetParent(handParentForContainerPlacement, true);
            pivotChildTransform.SetParent(handParentForContainerPlacement, true);

            if (xrPlayer == null)
                xrPlayer = GameObject.FindGameObjectWithTag("XRCamera").transform;


            if (currentEnvironment == null)
                currentEnvironment = GameObject.Find("Environment").transform;


            //SET PIVOT Location through our parents
            pivotRootTransform.position = xrPlayer.position;//handReferences[1].transform.position;// secondControllerInteraction.thisTransform.position;
            pivotChildTransform.position = xrPlayer.position; //handReferences[0].transform.position;// firstControllerInteraction.thisTransform.position;

            //place position of rotations to be in the center of both hands to rotate according to center point of hands not object center
            doubleGrabRotationTransform.position = xrPlayer.position;//currentEnvironment.transform.position;// player.transform.position;//((handReferences[1].transform.position + handReferences[0].transform.position) / 2);

            //set our second pivot as a child of first to have a pivot for each hands
            pivotChildTransform.SetParent(pivotRootTransform, true);

            //set it to parent to modify rotation
            currentEnvironment.SetParent(doubleGrabRotationTransform, true);

            // provides how an object should behave when double grabbing, object looks at one hand point of hand
            doubleGrabRotateCoordinate.LookAt((handReferences[1].transform.position - handReferences[0].transform.position), Vector3.up);

            //offset our current rotation from our initial difference to set
            var lookRot = doubleGrabInitialRotation * doubleGrabRotateCoordinate.rotation;

            animalRuler.position = ((handReferences[1].transform.position + handReferences[0].transform.position) / 2);
            lineRenderer.SetPosition(0, handReferences[0].transform.position);
            lineRenderer.SetPosition(1, handReferences[1].transform.position);

            //rotate y -> Yaw bring/push objects by pulling or pushing hand towards 
            var quat3 = Quaternion.AngleAxis(FreeFlightController.ClampAngle(lookRot.eulerAngles.y - initialYCoord, -360, 360), Vector3.up);
            //rotate z -> Roll shift objects right and left by lifting and lowering hands 
            // var quat4 = Quaternion.AngleAxis(FreeFlightController.ClampAngle(initialZCoord - lookRot.eulerAngles.x, -360, 360), -doubleGrabRotateCoordinate.right);

            //add our rotatations
            pivotRootTransform.rotation = quat3; //* quat4;// Quaternion.RotateTowards(doubleGrabRotationTransform.rotation, quat3 * quat4,60);// * handParentForContainerPlacement.rotation;

            //check for shifting of our player rotation to adjust our offset to prevent us from accumulating offsets that separates our grabbed object from hand
            if (handParentForContainerPlacement.eulerAngles.y != initialPlayerRotation.eulerAngles.y)
            {
                initialPlayerRotation = handParentForContainerPlacement.rotation;
                initialOffsetFromHandToGrabbedObject = (currentEnvironment.position) - ((handReferences[1].transform.position + handReferences[0].transform.position) / 2);
                initialOffsetFromHandToGrabbedObject /= invertedscaleRatioBasedOnDistance;
            }

            //modify object spacing offset when scalling using ratio between initial scale and currentscale
            // currentEnvironment.position = ((handReferences[1].transform.position + handReferences[0].transform.position) / 2) + (initialOffsetFromHandToGrabbedObject * scaleRatioBasedOnDistance);
            //}
            #endregion
        }
    }
}




//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Komodo.Runtime;
//using UnityEngine.Events;

//namespace Komodo.IMPRESS
//{
//    public class TiltBrushTeleport : MonoBehaviour, IUpdatable
//    {

//        public Vector2 offsetLimits = new Vector2(-0.05f, 0.8f);
//        public MeshRenderer animalScaleMaterial;

//        public TeleportPlayer teleportPlayer;

//        private LineRenderer lineRenderer;
//        public Transform animalRuler;

//        //check for left hand grabbing
//        public Transform currentEnvironment;
//        //get parent if we are switching objects between hands we want to keep track of were to place it back, to avoid hierachy parenting displacement
//        public Transform originalParentOfFirstHandTransform;

//        //do the same as above for the right Hand
//        public Transform secondObjectGrabbed;
//        public Transform originalParentOfSecondHandTransform;

//        public Transform[] handReferences = new Transform[2];//(2, Allocator.Persistent);

//        //away to keep track if we are double grabbing a gameobject to call event;
//        public bool isDoubleGrabbing;

//        //Fields to rotate object appropriately
//        //Hierarchy used to set correct Pivot points for scalling and rotating objects on DoubleGrab
//        public Transform pivotRootTransform;             ///PARENT SCALE PIVOT1 CONTAINER
//        private static Transform pivotChildTransform;             ///-CHILD SCALE PIVOT2 CONTAINER
//        public Transform doubleGrabRotationTransform;       //--Child for rotations

//        public Transform handParentForContainerPlacement;

//        //coordinate system to use to tilt double grand object appropriately: pulling, pushing, hand lift, and hand lower
//        public Transform doubleGrabRotateCoordinate;

//        //initial Data when Double Grabbing -scalling and rotation 
//        public bool isInitialDoubleGrab;
//        private Quaternion doubleGrabInitialRotation;
//        private float doubleGrabinitialDistance;
//        private Vector3 doublGrabinitialScale;
//        private Vector3 initialOffsetFromHandToGrabbedObject;
//        private Quaternion initialPlayerRotation;
//        private float initialScaleRatioBasedOnDistance;
//        float initialZCoord;
//        float initialYCoord;

//        //away to keep track if we are double grabbing a gameobject to call event;
//        public UnityEvent onDoubleTriggerPress;
//        public UnityEvent onDoubleTriggerRelease;


//        public void Awake()
//        {
//            //used to set our managers alive state to true to detect if it exist within scene
//            //  var initManager = Instance;


//            //create hierarchy to rotate double grab objects appropriately
//            //create root parent and share it through scripts by setting it to a static field
//            pivotRootTransform = new GameObject("PIVOT_ROOT").transform;
//            ////place object one level up from hand to avoid getting our hand rotations
//            pivotRootTransform.parent = transform.parent;

//            //construct coordinate system to reference for tilting double grab object 
//            doubleGrabRotateCoordinate = new GameObject("DoubleGrabCoordinateForObjectTilt").transform;
//            doubleGrabRotateCoordinate.SetParent(transform.root.parent, true);
//            doubleGrabRotateCoordinate.localPosition = Vector3.zero;

//            //parent used to set secondary hand pivot for scalling objects properly
//            //pivotChildTransform = new GameObject("Pivot Point 2 Parent").transform;
//            //pivotChildTransform.SetParent(pivotRootTransform, true);
//            //pivotChildTransform.localPosition = Vector3.zero;

//            //parent used for rotating or doble grab object
//            doubleGrabRotationTransform = new GameObject("Rotation Parent_3").transform;
//            doubleGrabRotationTransform.SetParent(pivotChildTransform, true);
//            doubleGrabRotationTransform.localPosition = Vector3.zero;
//        }

//        Transform player;
//        Transform xrPlayer;

//        public void Start()
//        {
//            //if (GameStateManager.IsAlive)
//            //    GameStateManager.Instance.RegisterUpdatableObject(this);

//            player = GameObject.FindGameObjectWithTag("Player").transform;

//            if (player)
//            {
//                if (player.TryGetComponent(out PlayerReferences pR))
//                {
//                    handReferences[0] = pR.handL;
//                    handReferences[1] = pR.handR;
//                }
//                teleportPlayer = player.GetComponent<TeleportPlayer>();
//            }

//            //set references for parent
//            handParentForContainerPlacement = pivotRootTransform.parent;

//            lineRenderer = gameObject.GetComponent<LineRenderer>();

//            animalScaleMaterial.material.SetTextureOffset("_MainTex", Vector2.zero);

//            animalRuler.gameObject.SetActive(false);
//            lineRenderer.enabled = false;// (false);
//            //       off = animalScaleMaterial.material.GetTextureOffset("_MainTex");

//            SceneManagerExtensions.Instance.onSceneChange += GetAndSetSceneInfo;

//            onDoubleTriggerPress.AddListener(() => SetDoubleTriggerStateOn());
//            onDoubleTriggerRelease.AddListener(() => SetDoubleTriggerStateOff());

//        }

//        [ContextMenu("Double Grab State ON")]
//        public void SetDoubleTriggerStateOn()
//        {
//            // isDoubleGrabbing = true;
//            animalRuler.gameObject.SetActive(true);
//            lineRenderer.enabled = true;

//            //register our update loop to be called
//            if (GameStateManager.IsAlive)
//                GameStateManager.Instance.RegisterUpdatableObject(this);
//        }

//        [ContextMenu("Double Grab State OFF")]
//        public void SetDoubleTriggerStateOff()
//        {
//            //   isDoubleGrabbing = false;
//            animalRuler.gameObject.SetActive(false);
//            lineRenderer.enabled = false;

//            //deregister our update loop not be called when not in use
//            if (GameStateManager.IsAlive)
//                GameStateManager.Instance.DeRegisterUpdatableObject(this);
//        }

//        //public Transform currentEnvironment;
//        public void GetAndSetSceneInfo()
//        {
//            currentEnvironment = GameObject.Find("Environment").transform;
//        }



//        public void OnUpdate(float realltime)
//        {
//            if (!currentEnvironment)
//                currentEnvironment = GameObject.Find("Environment").transform;

//            #region DoubleHand Funcionality

//            // Debug.Log("outside");
//            //Called every update when grabbing same item
//            //if (isDoubleGrabbing)
//            //{
//            //if (currentEnvironment == null)
//            //{
//            //    isDoubleGrabbing = false;
//            //    return;
//            //}
//            //set values 
//            if (isInitialDoubleGrab == false)
//            {
//                //inficates to run this only once at start to get initial values to use in update loop
//                isInitialDoubleGrab = true;

//                //grab values to know how we should start affecting our object 
//                doubleGrabinitialDistance = Vector3.Distance(handReferences[0].position, handReferences[1].position);
//                doublGrabinitialScale = pivotRootTransform.localScale;
//                pivotChildTransform.rotation = Quaternion.identity;// player.transform.GetChild(0).rotation;

//                //reset values for our container objects that we use to deform and rotate objects
//                doubleGrabRotationTransform.rotation = Quaternion.identity;
//                pivotRootTransform.localScale = Vector3.one;

//                doubleGrabRotateCoordinate.rotation = player.transform.GetChild(0).rotation;

//                //set reference vector to tilt our grabed object on - left hand looks at right and sets tilt according to movement of origin or lookat target 
//                doubleGrabRotateCoordinate.LookAt((handReferences[1].transform.position - handReferences[0].transform.position), Vector3.up);

//                //Get the inverse of the initial rotation to use in update loop to avoid moving the object when grabbing   
//                doubleGrabInitialRotation = Quaternion.Inverse(doubleGrabRotateCoordinate.rotation * player.transform.GetChild(0).rotation);

//                //get rotational difference to be able to offset it apropriately in update loop
//                var tiltRotation = doubleGrabInitialRotation * doubleGrabRotateCoordinate.rotation;

//                //our initial orientation to use to tilt object, due to the way lookat behavior behaves we have to set x as Z 
//                //    initialZCoord = tiltRotation.eulerAngles.x - doubleGrabRotationTransform.transform.eulerAngles.x;
//                initialYCoord = tiltRotation.eulerAngles.y - doubleGrabRotationTransform.transform.eulerAngles.y;

//                ////to fix parenting scalling down issue between centerpoint of hands and object
//                initialOffsetFromHandToGrabbedObject = currentEnvironment.position - ((handReferences[1].transform.position + handReferences[0].transform.position) / 2);// - handParentForContainerPlacement.position;

//                //pick up the rotation of our client to know when to update our offsets from hands to grab object
//                initialPlayerRotation = player.transform.GetChild(0).rotation;//handParentForContainerPlacement.rotation;
//                return;
//            }

//            //a ratio between our current distance divided by our initial distance
//            var scaleRatioBasedOnDistance = Vector3.Distance(handReferences[0].transform.position, handReferences[1].transform.position) / doubleGrabinitialDistance;
//            animalScaleMaterial.material.SetTextureOffset("_MainTex", new Vector2(Mathf.Clamp(scaleRatioBasedOnDistance - 0.3f, offsetLimits.x, offsetLimits.y), 0));


//            if (float.IsNaN(currentEnvironment.localScale.y)) return;

//            //we multiply our ratio with our initial scale
//            pivotRootTransform.localScale = doublGrabinitialScale * scaleRatioBasedOnDistance;

//            //place our grabbed object and second pivot away from the influeces of scale and rotation at first
//            currentEnvironment.SetParent(handParentForContainerPlacement, true);

//            // pivotChildTransform.rotation = player.transform.GetChild(0).rotation;

//            pivotChildTransform.SetParent(handParentForContainerPlacement, true);

//            if (xrPlayer == null)
//                xrPlayer = GameObject.FindGameObjectWithTag("XRCamera").transform;


//            if (currentEnvironment == null)
//                currentEnvironment = GameObject.Find("Environment").transform;//GameObject.Find("Environment").transform;


//            //SET PIVOT Location through our parents
//            pivotRootTransform.position = xrPlayer.position;//handReferences[1].transform.position;// secondControllerInteraction.thisTransform.position;
//            pivotChildTransform.position = xrPlayer.position; //handReferences[0].transform.position;// firstControllerInteraction.thisTransform.position;

//            //place position of rotations to be in the center of both hands to rotate according to center point of hands not object center
//            doubleGrabRotationTransform.position = xrPlayer.position;//currentEnvironment.transform.position;// player.transform.position;//((handReferences[1].transform.position + handReferences[0].transform.position) / 2);


//            //set our second pivot as a child of first to have a pivot for each hands
//            pivotChildTransform.SetParent(pivotRootTransform, true);

//            //set it to parent to modify rotation
//            //  currentEnvironment.SetParent(doubleGrabRotationTransform, true);

//            //doubleGrabRotateCoordinate.rotation = teleportPlayer.transform.rotation;


//            doubleGrabRotateCoordinate.rotation = player.transform.GetChild(0).rotation;

//            // provides how an object should behave when double grabbing, object looks at one hand point of hand
//            doubleGrabRotateCoordinate.LookAt((handReferences[1].transform.position - handReferences[0].transform.position), Vector3.up);

//            //offset our current rotation from our initial difference to set
//            var lookRot = doubleGrabInitialRotation * doubleGrabRotateCoordinate.rotation;

//            animalRuler.position = ((handReferences[1].transform.position + handReferences[0].transform.position) / 2);
//            lineRenderer.SetPosition(0, handReferences[0].transform.position);
//            lineRenderer.SetPosition(1, handReferences[1].transform.position);

//            //rotate y -> Yaw bring/push objects by pulling or pushing hand towards 
//            var quat3 = Quaternion.AngleAxis(FreeFlightController.ClampAngle(lookRot.eulerAngles.y - initialYCoord, -360, 360), Vector3.up);
//            //rotate z -> Roll shift objects right and left by lifting and lowering hands 
//            // var quat4 = Quaternion.AngleAxis(FreeFlightController.ClampAngle(initialZCoord - lookRot.eulerAngles.x, -360, 360), -doubleGrabRotateCoordinate.right);
//            player.transform.GetChild(0).rotation = quat3;

//            //   teleportPlayer.SetXRAndSpectatorRotation( quat3);


//            //add our rotatations
//            pivotRootTransform.rotation = quat3; //* quat4;// Quaternion.RotateTowards(doubleGrabRotationTransform.rotation, quat3 * quat4,60);// * handParentForContainerPlacement.rotation;

//            //check for shifting of our player rotation to adjust our offset to prevent us from accumulating offsets that separates our grabbed object from hand
//            if (player.transform.GetChild(0).rotation.eulerAngles.y != initialPlayerRotation.eulerAngles.y)
//            {
//                initialPlayerRotation = handParentForContainerPlacement.rotation;
//                initialOffsetFromHandToGrabbedObject = (currentEnvironment.position) - ((handReferences[1].transform.position + handReferences[0].transform.position) / 2);
//                initialOffsetFromHandToGrabbedObject /= scaleRatioBasedOnDistance;
//            }

//            //modify object spacing offset when scalling using ratio between initial scale and currentscale
//            // currentEnvironment.position = ((handReferences[1].transform.position + handReferences[0].transform.position) / 2) + (initialOffsetFromHandToGrabbedObject * scaleRatioBasedOnDistance);
//            //}
//            #endregion
//        }
//    }
//}

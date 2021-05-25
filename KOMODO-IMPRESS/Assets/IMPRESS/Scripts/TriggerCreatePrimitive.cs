using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using Komodo.Utilities;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class TriggerCreatePrimitive : MonoBehaviour
    {
        private Transform thisTransform;
        private int primitiveTypeCreating;
        private PrimitiveType currentPrimitiveType;

        public GameObject primitiveCreationParent;

        //reference to pick up what toggle is currently on;
        public ToggleGroup primitiveToggleGroup;
        public Toggle sphereToggle;
        public Toggle capsuleToggle;
        public Toggle CylinderToggle;
        public Toggle CubeToggle;
        public Toggle PlaneToggle;
        // public Toggle QuadToggle;
        public Toggle currentToggle;

        public Transform parentOfPrimitiveObjectsToDisplay;

        public EntityManager entityManager;
        public void Awake() { thisTransform = transform; }

        public bool isInitialized = false;
        public void Initialize()
        {
            //only initialize once
            if (isInitialized == true)
                return;

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            isInitialized = true;

        }



        // Start is called before the first frame update
        public void OnEnable()
        {
            //only create when our cursor is Off
            if (UIManager.IsAlive)
                if (UIManager.Instance.GetCursorActiveState())
                return;


            CreatePrimitiveManager.Instance.CreatePrimitive();

          
        }

       
    }
}

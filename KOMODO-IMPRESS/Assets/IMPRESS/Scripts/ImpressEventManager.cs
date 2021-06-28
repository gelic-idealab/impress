using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Komodo.Utilities;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    [RequireComponent(typeof(MainUIReferences))]
    public class ImpressEventManager : MonoBehaviour
    {
        public UnityEvent onGroupButtonEnabled;

        public UnityEvent onGroupButtonDisabled;

        public UnityEvent onUngroupButtonEnabled;

        public UnityEvent onUngroupButtonDisabled;

        public UnityEvent onSunButtonEnabled;

        public UnityEvent onSunButtonDisabled;

        public UnityEvent onSpotlightButtonEnabled;

        public UnityEvent onSpotlightButtonDisabled;

        public UnityEvent onStrokeButtonEnabled;

        public UnityEvent onStrokeButtonDisabled;

        public PlayerReferences playerRefs;

        public MainUIReferences uiRefs;

        public Alternate_Button_Function groupButton;

        public Alternate_Button_Function ungroupButton;

        public Alternate_Button_Function sunPointLightButton;

        public Alternate_Button_Function spotLightModelButton;

        public GameObject togglesContainer;

        public GameObject drawTab;

        void OnValidate ()
        {
            if (groupButton == null)
            {
                throw new System.NullReferenceException("groupButton");
            }

            if (ungroupButton == null)
            {
                throw new System.NullReferenceException("ungroupButton");
            }

            if (sunPointLightButton == null)
            {
                throw new System.NullReferenceException("sunPointLightButton");
            }

            if (spotLightModelButton == null)
            {
                throw new System.NullReferenceException("spotLightModelButton");
            }

            if (togglesContainer == null)
            {
                throw new System.NullReferenceException("togglesContainer");
            }

            if (drawTab == null)
            {
                throw new System.NullReferenceException("drawTab");
            }
        }

        public void Start()
        {
            if (playerRefs == null)
            {
                throw new System.NullReferenceException("playerRefs");
            }

            if (GameObject.FindGameObjectWithTag("Player").TryGetComponent(out ImpressPlayer player))
            {
                groupButton.onFirstClick.AddListener(() =>
                {
                    player.leftTriggerGroup.gameObject.SetActive(true);
                });

                groupButton.onSecondClick.AddListener(() =>
                {
                    player.leftTriggerGroup.gameObject.SetActive(false);
                });

                ungroupButton.onFirstClick.AddListener(() =>
                {
                    player.leftTriggerUngroup.gameObject.SetActive(true);
                });

                ungroupButton.onSecondClick.AddListener(() =>
                {
                    player.leftTriggerUngroup.gameObject.SetActive(false);
                });
            }

            sunPointLightButton.onFirstClick.AddListener(() =>
            {
                LightsManager.Instance.sunPointLightModel.SetActive(true);
            });

            sunPointLightButton.onSecondClick.AddListener(() =>
            {
                LightsManager.Instance.sunPointLightModel.SetActive(false);
            });

            spotLightModelButton.onFirstClick.AddListener(() =>
            {
                LightsManager.Instance.spotLightModel.SetActive(true);
            });

            spotLightModelButton.onSecondClick.AddListener(() =>
            {
                LightsManager.Instance.spotLightModel.SetActive(false);
            });

            TabButton tab = drawTab.GetComponent<TabButton>();

            if (tab)
            {
                tab.onTabSelected.AddListener(() =>
                {
                    uiRefs.OnDrawButtonFirstClick(playerRefs);
                });

                tab.onTabDeselected.AddListener(() =>
                {
                    uiRefs.OnDrawButtonSecondClick(playerRefs);
                });
            }
        }

        // From Unity Learn Tutorial: 
        // Create a Simple Messaging System with Events 
        // Tutorial Last Updated June 3rd, 2019
        // "As an introduction to UnityActions and UnityEvents, 
        // we will create a simple messaging system which will 
        // allow items in our projects to subscribe to events, 
        // and have events trigger actions in our games. This 
        // will reduce dependencies and allow easier maintenance 
        // of our projects."

        private Dictionary <string, UnityEvent> eventDictionary;

        private static ImpressEventManager eventManager;

        public static ImpressEventManager Instance
        {
            get
            {
                if (!eventManager)
                {
                    eventManager = FindObjectOfType(typeof (ImpressEventManager)) as ImpressEventManager;

                    if (!eventManager)
                    {
                        Debug.LogError("There needs to be one active EventManager script in your scene.");
                    }
                    else
                    {
                        eventManager.Init();
                    }
                }

                return eventManager;
            }
        }

        void Init ()
        {
            if (eventDictionary == null)
            {
                eventDictionary = new Dictionary<string, UnityEvent>();
            }
        }

        public static void StartListening (string eventName, UnityAction listener)
        {
            if (Instance.eventDictionary.TryGetValue(eventName, out UnityEvent existingEvent))
            {
                existingEvent.AddListener(listener);
            }
            else
            {
                UnityEvent newEvent = new UnityEvent();

                newEvent.AddListener(listener);

                Instance.eventDictionary.Add(eventName, newEvent);
            }
        }

        public static void StopListening (string eventName, UnityAction listener)
        {
            if (eventManager == null)
            {
                return;
            }

            if (Instance.eventDictionary.TryGetValue(eventName, out UnityEvent existingEvent))
            {
                existingEvent.RemoveListener(listener);
            }
        }

        public static void TriggerEvent (string eventName)
        {
            if (Instance.eventDictionary.TryGetValue(eventName, out UnityEvent existingEvent))
            {
                existingEvent.Invoke();
            }
        }
    }
}

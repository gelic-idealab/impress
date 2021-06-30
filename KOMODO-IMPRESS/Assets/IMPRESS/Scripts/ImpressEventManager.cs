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
        public void Start()
        {
            // TODO (Brandon): Restore ability to add spot light and sun light.
            //  Examples: 
            //  public Alternate_Button_Function sunPointLightButton;
            //  public Alternate_Button_Function spotLightModelButton;
            //  sunPointLightButton.onFirstClick.AddListener(() =>
            //  {
            //         LightsManager.Instance.sunPointLightModel.SetActive(true);
            //  });

            //  LightsManager.Instance.spotLightModel.SetActive(true);
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

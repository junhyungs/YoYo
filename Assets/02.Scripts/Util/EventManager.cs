using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EventLibrary
{
    // 제네릭 UnityEvent 클래스 정의
    [Serializable]
    public class GenericEvent<T> : UnityEvent<T> { }

    public class EventManager<E> where E : Enum
    {
        // 이벤트 이름과 해당 UnityEvent를 저장하는 딕셔너리
        private static readonly Dictionary<E, UnityEventBase> EventDictionary = new Dictionary<E, UnityEventBase>();
        // 스레드 안전성을 위한 객체
        private static readonly object LockObj = new object();

        // 매개변수가 없는 UnityAction 리스너를 추가하는 메서드
        public static void StartListening(E eventName, UnityAction listener)
        {
            AddListener(eventName, listener);
        }

        // 제네릭 매개변수를 갖는 UnityAction 리스너를 추가하는 메서드
        public static void StartListening<T>(E eventName, UnityAction<T> listener)
        {
            AddListener(eventName, listener);
        }

        // 매개변수가 없는 UnityAction 리스너를 제거하는 메서드
        public static void StopListening(E eventName, UnityAction listener)
        {
            RemoveListener(eventName, listener);
        }

        // 제네릭 매개변수를 갖는 UnityAction 리스너를 제거하는 메서드
        public static void StopListening<T>(E eventName, UnityAction<T> listener)
        {
            RemoveListener(eventName, listener);
        }

        // 매개변수가 없는 이벤트를 트리거하는 메서드
        public static void TriggerEvent(E eventName)
        {
            InvokeEvent(eventName);
        }

        // 제네릭 매개변수를 갖는 이벤트를 트리거하는 메서드
        public static void TriggerEvent<T>(E eventName, T parameter)
        {
            InvokeEvent(eventName, parameter);
        }

        // 이벤트가 존재하지 않으면 생성하여 반환하는 메서드
        private static TEvent GetOrCreateEvent<TEvent>(E eventName) where TEvent : UnityEventBase, new()
        {
            if (!EventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent = new TEvent();
                EventDictionary.Add(eventName, thisEvent);
            }
            return thisEvent as TEvent;
        }

        // 이벤트가 비어 있으면 딕셔너리에서 제거하는 메서드
        private static void RemoveEventIfEmpty(E eventName, UnityEventBase thisEvent)
        {
            if (thisEvent.GetPersistentEventCount() == 0)
            {
                EventDictionary.Remove(eventName);
            }
        }

        // 리스너를 추가하는 메서드
        private static void AddListener<T>(E eventName, UnityAction<T> listener)
        {
            lock (LockObj)
            {
                GenericEvent<T> genericEvent = GetOrCreateEvent<GenericEvent<T>>(eventName);
                genericEvent.AddListener(listener);
            }
        }

        private static void AddListener(E eventName, UnityAction listener)
        {
            lock (LockObj)
            {
                UnityEvent unityEvent = GetOrCreateEvent<UnityEvent>(eventName);
                unityEvent.AddListener(listener);
            }
        }

        // 리스너를 제거하는 메서드
        private static void RemoveListener<T>(E eventName, UnityAction<T> listener)
        {
            lock (LockObj)
            {
                if (EventDictionary.TryGetValue(eventName, out var thisEvent) && thisEvent is GenericEvent<T> genericEvent)
                {
                    genericEvent.RemoveListener(listener);
                    RemoveEventIfEmpty(eventName, genericEvent);
                }
            }
        }

        private static void RemoveListener(E eventName, UnityAction listener)
        {
            lock (LockObj)
            {
                if (EventDictionary.TryGetValue(eventName, out var thisEvent) && thisEvent is UnityEvent unityEvent)
                {
                    unityEvent.RemoveListener(listener);
                    RemoveEventIfEmpty(eventName, unityEvent);
                }
            }
        }

        // 이벤트를 트리거하는 메서드
        private static void InvokeEvent<T>(E eventName, T parameter)
        {
            lock (LockObj)
            {
                try
                {
                    if (EventDictionary.TryGetValue(eventName, out var thisEvent) && thisEvent is GenericEvent<T> genericEvent)
                    {
                        genericEvent.Invoke(parameter);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error triggering event {eventName} with parameter {parameter}: {e.Message}");
                }
            }
        }

        private static void InvokeEvent(E eventName)
        {
            lock (LockObj)
            {
                try
                {
                    if (EventDictionary.TryGetValue(eventName, out var thisEvent) && thisEvent is UnityEvent unityEvent)
                    {
                        unityEvent.Invoke();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error triggering event {eventName}: {e.Message}");
                }
            }
        }
    }
}

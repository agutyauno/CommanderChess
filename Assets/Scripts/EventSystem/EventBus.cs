using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommanderChess.EventSystem
{
    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> handlers = new();
        private readonly Dictionary<Type, List<Delegate>> onceHandlers = new();
        private readonly Queue<IGameEvent> eventQueue = new();
        private bool isProcessing = false;

        #region Subscribe/Unsubscribe

        /// <summary>
        /// Subscribe to an event type
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null)
            {
                Debug.LogWarning("Attempting to subscribe null handler");
                return;
            }

            var eventType = typeof(T);
            if (!handlers.ContainsKey(eventType))
            {
                handlers[eventType] = new List<Delegate>();
            }

            if (!handlers[eventType].Contains(handler))
            {
                handlers[eventType].Add(handler);
            }
        }

        /// <summary>
        /// Subscribe to an event type (one-time only)
        /// Handler will be automatically unsubscribed after first invocation
        /// </summary>
        public void SubscribeOnce<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null)
            {
                Debug.LogWarning("Attempting to subscribe null handler");
                return;
            }

            var eventType = typeof(T);
            if (!onceHandlers.ContainsKey(eventType))
            {
                onceHandlers[eventType] = new List<Delegate>();
            }

            if (!onceHandlers[eventType].Contains(handler))
            {
                onceHandlers[eventType].Add(handler);
            }
        }

        /// <summary>
        /// Unsubscribe from an event type
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null) return;

            var eventType = typeof(T);

            // Remove from regular handlers
            if (handlers.ContainsKey(eventType))
            {
                handlers[eventType].Remove(handler);
                if (handlers[eventType].Count == 0)
                {
                    handlers.Remove(eventType);
                }
            }

            // Remove from once handlers
            if (onceHandlers.ContainsKey(eventType))
            {
                onceHandlers[eventType].Remove(handler);
                if (onceHandlers[eventType].Count == 0)
                {
                    onceHandlers.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Unsubscribe all handlers for an event type
        /// </summary>
        public void UnsubscribeAll<T>() where T : IGameEvent
        {
            var eventType = typeof(T);
            handlers.Remove(eventType);
            onceHandlers.Remove(eventType);
        }

        /// <summary>
        /// Clear all event subscriptions
        /// </summary>
        public void Clear()
        {
            handlers.Clear();
            onceHandlers.Clear();
            eventQueue.Clear();
        }

        #endregion

        #region Publish

        /// <summary>
        /// Publish an event immediately (synchronous)
        /// </summary>
        public void Publish<T>(T evt) where T : IGameEvent
        {
            var eventType = typeof(T);

            // Handle regular subscribers
            if (handlers.ContainsKey(eventType))
            {
                // Create a copy to allow safe unsubscribe during callback
                var handlersCopy = new List<Delegate>(handlers[eventType]);

                foreach (var handler in handlersCopy)
                {
                    try
                    {
                        ((Action<T>)handler)?.Invoke(evt);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error invoking event handler for {eventType.Name}: {ex}");
                    }
                }
            }

            // Handle one-time subscribers
            if (onceHandlers.ContainsKey(eventType))
            {
                var onceHandlersCopy = new List<Delegate>(onceHandlers[eventType]);
                onceHandlers[eventType].Clear();

                foreach (var handler in onceHandlersCopy)
                {
                    try
                    {
                        ((Action<T>)handler)?.Invoke(evt);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error invoking once event handler for {eventType.Name}: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// Queue an event to be processed later
        /// Useful for avoiding recursive event calls
        /// </summary>
        public void PublishQueued<T>(T evt) where T : IGameEvent
        {
            eventQueue.Enqueue(evt);
        }

        /// <summary>
        /// Process all queued events
        /// Call this from your game loop or update
        /// </summary>
        public void ProcessQueue()
        {
            if (isProcessing) return;

            isProcessing = true;

            while (eventQueue.Count > 0)
            {
                var evt = eventQueue.Dequeue();
                var eventType = evt.GetType();

                // Use reflection to call Publish<T>
                var publishMethod = typeof(EventBus).GetMethod(nameof(Publish));
                var genericPublish = publishMethod.MakeGenericMethod(eventType);

                try
                {
                    genericPublish.Invoke(this, new object[] { evt });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing queued event {eventType.Name}: {ex}");
                }
            }

            isProcessing = false;
        }

        #endregion

        #region Debug Utilities

        /// <summary>
        /// Get count of subscribers for an event type
        /// </summary>
        public int GetSubscriberCount<T>() where T : IGameEvent
        {
            var eventType = typeof(T);
            int count = 0;

            if (handlers.ContainsKey(eventType))
                count += handlers[eventType].Count;

            if (onceHandlers.ContainsKey(eventType))
                count += onceHandlers[eventType].Count;

            return count;
        }

        /// <summary>
        /// Check if an event type has any subscribers
        /// </summary>
        public bool HasSubscribers<T>() where T : IGameEvent
        {
            var eventType = typeof(T);
            return (handlers.ContainsKey(eventType) && handlers[eventType].Count > 0) ||
                   (onceHandlers.ContainsKey(eventType) && onceHandlers[eventType].Count > 0);
        }

        /// <summary>
        /// Get debug info about current subscriptions
        /// </summary>
        public string GetDebugInfo()
        {
            var info = "=== EventBus Debug Info ===\n";
            info += $"Total Event Types: {handlers.Count + onceHandlers.Count}\n";
            info += $"Queued Events: {eventQueue.Count}\n\n";

            info += "Regular Handlers:\n";
            foreach (var kvp in handlers)
            {
                info += $"  {kvp.Key.Name}: {kvp.Value.Count} subscribers\n";
            }

            info += "\nOnce Handlers:\n";
            foreach (var kvp in onceHandlers)
            {
                info += $"  {kvp.Key.Name}: {kvp.Value.Count} subscribers\n";
            }

            return info;
        }

        #endregion   
    }
}
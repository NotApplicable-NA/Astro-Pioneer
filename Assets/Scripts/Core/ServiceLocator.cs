using System;
using System.Collections.Generic;

namespace AstroPioneer.Core
{
    /// <summary>
    /// ServiceLocator — Lightweight DI stepping-stone replacing 17 static Singletons.
    /// Interface-agnostic design allows future migration to VContainer / Reflex
    /// without modifying consumer code (just swap the resolver).
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        /// <summary>
        /// Register a service. Typically called in MonoBehaviour.Awake().
        /// </summary>
        public static void Register<T>(T service)
        {
            var type = typeof(T);
#if UNITY_EDITOR
            if (services.ContainsKey(type))
                UnityEngine.Debug.LogWarning($"[ServiceLocator] Overwriting existing service: {type.Name}");
#endif
            services[type] = service;
        }

        /// <summary>
        /// Unregister a service. Typically called in MonoBehaviour.OnDestroy().
        /// </summary>
        public static void Unregister<T>()
        {
            services.Remove(typeof(T));
        }

        /// <summary>
        /// Retrieve a registered service. Throws if not found.
        /// </summary>
        public static T Get<T>()
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var service))
                return (T)service;

            throw new InvalidOperationException(
                $"[ServiceLocator] Service '{type.Name}' not registered. " +
                $"Ensure it calls ServiceLocator.Register<{type.Name}>(this) in Awake().");
        }

        /// <summary>
        /// Safe retrieval — returns false instead of throwing when service isn't ready.
        /// Use this during initialization order ambiguity.
        /// </summary>
        public static bool TryGet<T>(out T service)
        {
            if (services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = default;
            return false;
        }

        /// <summary>
        /// Wipe all services. Called on domain reload / scene teardown.
        /// </summary>
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ClearAll() => services.Clear();
    }
}

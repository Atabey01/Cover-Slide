using System;
using System.Collections.Generic;
using DEV.Scripts.Config;
using DG.Tweening;
using Lean.Pool;
using UnityEngine;

namespace DEV.Scripts.Managers
{
    /// <summary>
    /// Centralized GameObject factory system.
    /// Uses object pooling (LeanPool) for efficient object management.
    /// 
    /// Usage:
    /// 1. Initialize in GameManager: Factory.Initialize(gameConfig, parentTransform);
    /// 2. Create objects: var obj = Factory.Create<MyComponent>(prefab);
    /// 3. Destroy all: Factory.DestroyAll();
    /// </summary>
    public static class Factory
    {
        private static GameConfig _gameConfig;
        private static Transform _defaultParent;
        private static bool _isInitialized;

        // Track spawned objects by type
        private static readonly Dictionary<Type, List<GameObject>> _spawnedObjectsByType = new();
        
        // Track objects that should be destroyed (not pooled)
        private static readonly Dictionary<Type, List<GameObject>> _destroyObjectsByType = new();

        /// <summary>
        /// Initialize Factory with GameConfig and optional parent transform.
        /// Should be called from GameManager.Initialize().
        /// </summary>
        public static void Initialize(GameConfig gameConfig, Transform defaultParent = null)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("Factory: Already initialized!");
                return;
            }

            _gameConfig = gameConfig;
            _defaultParent = defaultParent;
            _isInitialized = true;

            Debug.Log("Factory: Initialized");
        }

        /// <summary>
        /// Dispose Factory. Clears all references and destroys tracked objects.
        /// Should be called from GameManager.Dispose().
        /// </summary>
        public static void Dispose()
        {
            if (!_isInitialized) return;

            DestroyAll();

            _gameConfig = null;
            _defaultParent = null;
            _isInitialized = false;

            Debug.Log("Factory: Disposed");
        }

        #region Generic Factory Methods

        /// <summary>
        /// Creates a GameObject from prefab using object pooling.
        /// </summary>
        /// <typeparam name="T">Component type to get from spawned object</typeparam>
        /// <param name="prefab">Prefab to spawn</param>
        /// <param name="parent">Parent transform (uses default parent if null)</param>
        /// <param name="usePooling">Whether to use object pooling (default: true)</param>
        /// <returns>Component of type T, or null if prefab is null or component not found</returns>
        public static T Create<T>(GameObject prefab, Transform parent = null, bool usePooling = true) where T : Component
        {
            EnsureInitialized();

            if (prefab == null)
            {
                Debug.LogWarning($"Factory: Prefab is null for type {typeof(T).Name}");
                return null;
            }

            GameObject instance;
            Transform targetParent = parent ?? _defaultParent;

            if (usePooling)
            {
                instance = LeanPool.Spawn(prefab, targetParent);
            }
            else
            {
                instance = UnityEngine.Object.Instantiate(prefab, targetParent);
            }

            if (instance == null)
            {
                Debug.LogWarning($"Factory: Failed to spawn prefab for type {typeof(T).Name}");
                return null;
            }

            T component = instance.GetComponent<T>();
            if (component == null)
            {
                Debug.LogWarning($"Factory: Component {typeof(T).Name} not found on prefab {prefab.name}");
                if (usePooling)
                {
                    LeanPool.Despawn(instance);
                }
                else
                {
                    UnityEngine.Object.Destroy(instance);
                }
                return null;
            }

            TrackObject<T>(instance, usePooling);
            return component;
        }

        /// <summary>
        /// Creates a GameObject from prefab using object pooling.
        /// </summary>
        /// <param name="prefab">Prefab to spawn</param>
        /// <param name="parent">Parent transform (uses default parent if null)</param>
        /// <param name="usePooling">Whether to use object pooling (default: true)</param>
        /// <returns>Created GameObject, or null if prefab is null</returns>
        public static GameObject Create(GameObject prefab, Transform parent = null, bool usePooling = true)
        {
            EnsureInitialized();

            if (prefab == null)
            {
                Debug.LogWarning("Factory: Prefab is null");
                return null;
            }

            GameObject instance;
            Transform targetParent = parent ?? _defaultParent;

            if (usePooling)
            {
                instance = LeanPool.Spawn(prefab, targetParent);
            }
            else
            {
                instance = UnityEngine.Object.Instantiate(prefab, targetParent);
            }

            if (instance != null)
            {
                TrackObject<GameObject>(instance, usePooling);
            }

            return instance;
        }

        #endregion

        #region Tracking Methods

        /// <summary>
        /// Track a spawned object by type.
        /// </summary>
        private static void TrackObject<T>(GameObject obj, bool usePooling)
        {
            if (obj == null) return;

            Type type = typeof(T);
            Dictionary<Type, List<GameObject>> targetDictionary = usePooling ? _spawnedObjectsByType : _destroyObjectsByType;

            if (!targetDictionary.ContainsKey(type))
            {
                targetDictionary[type] = new List<GameObject>();
            }

            targetDictionary[type].Add(obj);
        }

        /// <summary>
        /// Remove an object from tracking.
        /// </summary>
        public static void RemoveTrackedObject<T>(GameObject obj, bool usePooling = true)
        {
            if (obj == null) return;

            Type type = typeof(T);
            Dictionary<Type, List<GameObject>> targetDictionary = usePooling ? _spawnedObjectsByType : _destroyObjectsByType;

            if (targetDictionary.ContainsKey(type))
            {
                targetDictionary[type].Remove(obj);
            }
        }

        /// <summary>
        /// Get all tracked objects of a specific type.
        /// </summary>
        public static List<GameObject> GetTrackedObjects<T>(bool usePooling = true)
        {
            Type type = typeof(T);
            Dictionary<Type, List<GameObject>> targetDictionary = usePooling ? _spawnedObjectsByType : _destroyObjectsByType;

            if (targetDictionary.ContainsKey(type))
            {
                return new List<GameObject>(targetDictionary[type]);
            }

            return new List<GameObject>();
        }

        #endregion

        #region Destroy Methods

        /// <summary>
        /// Destroy all tracked objects.
        /// </summary>
        public static void DestroyAll()
        {
            // Destroy pooled objects
            foreach (var kvp in _spawnedObjectsByType)
            {
                foreach (var obj in kvp.Value)
                {
                    if (obj != null)
                    {
                        // Kill any active DOTween animations
                        obj.transform.DOKill();
                        LeanPool.Despawn(obj);
                    }
                }
                kvp.Value.Clear();
            }

            // Destroy non-pooled objects
            foreach (var kvp in _destroyObjectsByType)
            {
                foreach (var obj in kvp.Value)
                {
                    if (obj != null)
                    {
                        // Kill any active DOTween animations
                        obj.transform.DOKill();
                        UnityEngine.Object.Destroy(obj);
                    }
                }
                kvp.Value.Clear();
            }

            _spawnedObjectsByType.Clear();
            _destroyObjectsByType.Clear();
        }

        /// <summary>
        /// Destroy all tracked objects of a specific type.
        /// </summary>
        public static void DestroyAll<T>(bool usePooling = true)
        {
            Type type = typeof(T);
            Dictionary<Type, List<GameObject>> targetDictionary = usePooling ? _spawnedObjectsByType : _destroyObjectsByType;

            if (targetDictionary.ContainsKey(type))
            {
                foreach (var obj in targetDictionary[type])
                {
                    if (obj != null)
                    {
                        obj.transform.DOKill();
                        
                        if (usePooling)
                        {
                            LeanPool.Despawn(obj);
                        }
                        else
                        {
                            UnityEngine.Object.Destroy(obj);
                        }
                    }
                }
                targetDictionary[type].Clear();
            }
        }

        /// <summary>
        /// Destroy a specific object.
        /// </summary>
        public static void Destroy(GameObject obj, bool usePooling = true)
        {
            if (obj == null) return;

            obj.transform.DOKill();

            if (usePooling)
            {
                LeanPool.Despawn(obj);
            }
            else
            {
                UnityEngine.Object.Destroy(obj);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get GameConfig (read-only access).
        /// </summary>
        public static GameConfig GetGameConfig()
        {
            EnsureInitialized();
            return _gameConfig;
        }

        /// <summary>
        /// Get default parent transform.
        /// </summary>
        public static Transform GetDefaultParent()
        {
            EnsureInitialized();
            return _defaultParent;
        }

        /// <summary>
        /// Set default parent transform.
        /// </summary>
        public static void SetDefaultParent(Transform parent)
        {
            EnsureInitialized();
            _defaultParent = parent;
        }

        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Debug.LogError("Factory: Not initialized! Call Factory.Initialize() first in GameManager.");
                throw new InvalidOperationException("Factory is not initialized. Call Initialize() first.");
            }
        }

        #endregion
    }
}


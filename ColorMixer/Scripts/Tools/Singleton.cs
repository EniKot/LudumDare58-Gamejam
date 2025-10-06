using UnityEngine;

namespace Tools
{
    /// <summary>极简 Mono 单例。把它作为基类即可。</summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this as T)
            {
                // 场景里已存在同类单例，新的自毁
                Destroy(gameObject);
                return;
            }

            Instance = this as T;
            DontDestroyOnLoad(gameObject); // 全局常驻
        }
    }
}

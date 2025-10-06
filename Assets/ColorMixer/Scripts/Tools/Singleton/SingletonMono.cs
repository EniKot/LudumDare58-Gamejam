using UnityEngine;
public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                
                instance = FindObjectOfType<T>();
                
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name + "_Singleton");
                    instance = singletonObject.AddComponent<T>();
                    
                }
            }
            return instance;
        }
    }
  
    protected virtual void Awake()
    {
        
       
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(instance);
        }
        else

        {   
            Debug.LogWarning("There is already an instance of " + instance + ". Destroying the new one.");
            Debug.LogWarning("destroy duplicate singleton:" + typeof(T).Name);
            Destroy(instance);
        }
    }
}
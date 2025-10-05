// 可选：Assets/Scripts/Gameplay/BootLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("Level_Intro");
    }
}

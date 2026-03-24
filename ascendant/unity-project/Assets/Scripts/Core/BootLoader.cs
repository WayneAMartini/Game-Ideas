using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ascendant.Core
{
    public class BootLoader : MonoBehaviour
    {
        void Start()
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}

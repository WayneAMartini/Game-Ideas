using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Ascendant.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] Button _playButton;

        void Start()
        {
            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlayClicked);
        }

        void OnPlayClicked()
        {
            SceneManager.LoadScene("GameScene");
        }

        void OnDestroy()
        {
            if (_playButton != null)
                _playButton.onClick.RemoveListener(OnPlayClicked);
        }
    }
}

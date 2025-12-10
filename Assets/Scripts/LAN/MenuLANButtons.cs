using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLANButtons : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button hostButton;
    [SerializeField] private UnityEngine.UI.Button joinButton;

    // This flag tells Game_LAN what to do AFTER loading
    public static bool ShouldHost = false;

    private void Start()
    {
        hostButton.onClick.RemoveAllListeners();
        joinButton.onClick.RemoveAllListeners();

        hostButton.onClick.AddListener(() =>
        {
            ShouldHost = true;
            SceneManager.LoadScene("Game_LAN");
        });

        joinButton.onClick.AddListener(() =>
        {
            ShouldHost = false;
            SceneManager.LoadScene("Game_LAN");
        });
    }

    private void OnEnable() => ShouldHost = false;
}
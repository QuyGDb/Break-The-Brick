using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LevelButton : MonoBehaviour
{
    public Image lockImage;
    Button button;
    private void Awake()
    {
        button = GetComponent<Button>();

    }
    public void OnClick()
    {
        int level = int.Parse(GetComponentInChildren<TextMeshProUGUI>().text);
        // Settings.selectedLevel = level;
        SceneManager.LoadScene(1);
    }
    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }
}

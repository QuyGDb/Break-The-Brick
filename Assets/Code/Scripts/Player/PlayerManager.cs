using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public PlayerControls playerControls;
    [HideInInspector] public PlayerAtributes playerAtributes;
    [HideInInspector] public PlayerAnimation playerAnimation;
    [HideInInspector] public Atributes atributes;
    private int brickCount;
    [SerializeField] private int maxChopCount = 10;
    private int chopCount;

    private void Awake()
    {
        playerControls = GetComponent<PlayerControls>();
        playerAtributes = GetComponent<PlayerAtributes>();
        playerAnimation = GetComponent<PlayerAnimation>();
        atributes = SaveLoadManager.LoadDataFromPlayerPrefs<Atributes>("atributes");
    }
    private void OnEnable()
    {
        StaticEventHandler.OnBrickCount += StaticEventHandler_OnBrickCount;
    }
    private void OnDisable()
    {
        StaticEventHandler.OnBrickCount -= StaticEventHandler_OnBrickCount;
    }

    private void StaticEventHandler_OnBrickCount(int count, int maxBrick)
    {
        brickCount = count;

    }
    private void Start()
    {
        StaticEventHandler.CallOnPlayerManager(this);
    }
    public void TrackChopCount()
    {
        chopCount++;
        StaticEventHandler.CallOnChopCount(chopCount, maxChopCount);
        if (chopCount == maxChopCount)
        {
            if (brickCount > 3)
            {
                GameManager.Instance.HandleGameState(GameState.Win);
            }
            else
            {
                GameManager.Instance.HandleGameState(GameState.Lose);
            }
        }
    }
}


using UnityEngine;


public class PlayerAtributes : MonoBehaviour
{

    [SerializeField] private PlayerBaseAtribubesSO atribubesSO;
    private float speed = 5;
    [HideInInspector] public float atk = 0;
    private float income = 0;
    private float money = 0;

    private int speedLevel = 1;
    private int atkLevel = 1;
    private int incomeLevel = 1;

    [SerializeField] private int maxChopCount = 10;
    private int currentChopCount;

    public void TrackChopCount()
    {
        currentChopCount++;
    }

    private void Start()
    {
        currentChopCount = 0;
        LoadAtributeFromPlayerPrefs();
    }

    private void LoadAtributeFromPlayerPrefs()
    {

        if (PlayerPrefs.HasKey("atkLevel"))
        {
            atkLevel = PlayerPrefs.GetInt("atkLevel");
        }
        else
        {
            atkLevel = 1;
        }
        if (PlayerPrefs.HasKey("speedLevel"))
        {
            speedLevel = PlayerPrefs.GetInt("speedLevel");
        }
        else
        {
            speedLevel = 1;
        }
        if (PlayerPrefs.HasKey("atk"))
        {
            atk = PlayerPrefs.GetFloat("atk");
        }
        else
        {
            atk = atribubesSO.baseAttack;
        }
        if (PlayerPrefs.HasKey("income"))
        {
            income = PlayerPrefs.GetFloat("income");
        }
        else
        {
            income = atribubesSO.baseIncome;
        }
        if (PlayerPrefs.HasKey("money"))
        {
            money = PlayerPrefs.GetFloat("money");
        }
        else
        {
            money = 0;
        }
        if (PlayerPrefs.HasKey("speed"))
        {
            speed = PlayerPrefs.GetFloat("speed");
        }
        else
        {
            speed = atribubesSO.baseSpeed;
        }
    }

    private void AttackUpgrade()
    {
        atk = atribubesSO.baseAttack * Mathf.Pow((1 * atribubesSO.atkGrowthRate), (atkLevel - 1));
        PlayerPrefs.SetFloat("atk", atk);
        PlayerPrefs.Save();
        AttackLevelUpgrade();
    }
    private void AttackLevelUpgrade()
    {
        atkLevel += 1;
        PlayerPrefs.SetInt("atkLevel", atkLevel);
        PlayerPrefs.Save();
    }
    private void AttackUpgradeCost()
    {
    }
    public float CalculateRatio(int lv, float x)
    {
        float exponent = Mathf.Floor((lv - 1) / 5f);
        float ratio = x * Mathf.Pow(1 + 0.25f, exponent);
        return ratio;
    }
    private void SpeedUpgrade()
    {
        speed = atribubesSO.baseSpeed * Mathf.Pow((1 * atribubesSO.speedGrowthRate), (speedLevel - 1));
        PlayerPrefs.SetFloat("speed", speed);
        PlayerPrefs.Save();
    }
    private void SpeedLevelUpgrade()
    {
        speedLevel += 1;
        PlayerPrefs.SetInt("speedLevel", speedLevel);
        PlayerPrefs.Save();
    }
    private void IncomeUpgrade()
    {
        income = atribubesSO.baseIncome * Mathf.Pow((1 * atribubesSO.incomeGrowthRate), (incomeLevel - 1));
        PlayerPrefs.SetFloat("income", income);
        PlayerPrefs.Save();
    }
    private void IncomeLevelUpgrade()
    {
        incomeLevel += 1;
        PlayerPrefs.SetInt("incomeLevel", incomeLevel);
        PlayerPrefs.Save();
    }

    private void AddMoney()
    {
        money += income;
        PlayerPrefs.SetFloat("money", money);
        PlayerPrefs.Save();
    }

    private void SubtractMoney(float amount)
    {
        money -= amount;
        PlayerPrefs.SetFloat("money", money);
        PlayerPrefs.Save();
    }
}

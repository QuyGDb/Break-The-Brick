
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;


public class PlayerAtributes : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerPrefs.DeleteAll();
        }
    }
    [SerializeField] private PlayerBaseAtribubesSO atribubesSO;
    private float speed = 5;
    [HideInInspector] public float atk = 0;

    public static float income = 0;
    private float money;
    private int speedLevel = 1;
    private int atkLevel = 1;
    private int incomeLevel = 1;

    [SerializeField] private Button attackButton;
    [SerializeField] private Button speedButton;
    [SerializeField] private Button incomeButton;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI incomeText;
    [SerializeField] private TextMeshProUGUI moneyAtkText;
    [SerializeField] private TextMeshProUGUI moneySpeedText;
    [SerializeField] private TextMeshProUGUI moneyIncomeText;
    [SerializeField] private TextMeshProUGUI moneyText;
    private float speedUpgradeCost;
    private float attackUpgradeCost;
    private float incomeUpgradeCost;



    private void Start()
    {
        LoadAtributes();
        attackButton.onClick.AddListener(() => AttackUpgrade());
        speedButton.onClick.AddListener(() => SpeedUpgrade());
        incomeButton.onClick.AddListener(() => IncomeUpgrade());
    }

    #region Load Atributes
    private void LoadAtributes()
    {
        LoadAtributeLevelFromPlayerPrefs();
        LoadAtributesFromPlayerPrefs();
        LoadCostToUpgradeAtribute();
    }

    private void LoadAtributeLevelFromPlayerPrefs()
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
        if (PlayerPrefs.HasKey("incomeLevel"))
        {
            incomeLevel = PlayerPrefs.GetInt("incomeLevel");
        }
        else
        {
            incomeLevel = 1;
        }
    }

    private void LoadAtributesFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("atk"))
        {
            atk = PlayerPrefs.GetFloat("atk");
            atkText.text = atk.ToString();
        }
        else
        {
            atk = atribubesSO.baseAttack;
            atkText.text = atk.ToString();
        }
        if (PlayerPrefs.HasKey("income"))
        {
            income = PlayerPrefs.GetFloat("income");
            incomeText.text = income.ToString();
        }
        else
        {
            income = atribubesSO.baseIncome;
            incomeText.text = income.ToString();
        }
        if (PlayerPrefs.HasKey("money"))
        {
            money = PlayerPrefs.GetFloat("money");
            moneyText.text = money.ToString();
        }
        else
        {
            money = 0;
            moneyText.text = money.ToString();
        }
        if (PlayerPrefs.HasKey("speed"))
        {
            speed = PlayerPrefs.GetFloat("speed");
            speedText.text = speed.ToString();
        }
        else
        {
            speed = atribubesSO.baseSpeed;
            speedText.text = speed.ToString();
        }
    }

    private void LoadCostToUpgradeAtribute()
    {
        if (PlayerPrefs.HasKey("attackUpgradeCost"))
        {
            attackUpgradeCost = PlayerPrefs.GetFloat("attackUpgradeCost");
            moneyAtkText.text = attackUpgradeCost.ToString();
        }
        else
        {
            attackUpgradeCost = atribubesSO.baseMoneyForAtributes;
            moneyAtkText.text = attackUpgradeCost.ToString();
        }
        if (PlayerPrefs.HasKey("speedUpgradeCost"))
        {
            speedUpgradeCost = PlayerPrefs.GetFloat("speedUpgradeCost");
            moneySpeedText.text = speedUpgradeCost.ToString();
        }
        else
        {
            speedUpgradeCost = atribubesSO.baseMoneyForAtributes;
            moneySpeedText.text = speedUpgradeCost.ToString();
        }
        if (PlayerPrefs.HasKey("incomeUpgradeCost"))
        {
            incomeUpgradeCost = PlayerPrefs.GetFloat("incomeUpgradeCost");
            moneyIncomeText.text = incomeUpgradeCost.ToString();
        }
        else
        {
            incomeUpgradeCost = atribubesSO.baseMoneyForAtributes;
            moneyIncomeText.text = incomeUpgradeCost.ToString();
        }
    }

    #endregion
    private bool UpgradeSpeedCost()
    {
        float ratio = CalculateRatioUpgrade(speedLevel, atribubesSO.progressionRateForUpgradeCost, atribubesSO.levelThresholdForUpgradeCost);
        bool isSubtract = SubtractMoney(speedUpgradeCost * ratio);
        if (isSubtract)
        {
            speedUpgradeCost = speedUpgradeCost * ratio;
            PlayerPrefs.SetFloat("speedUpgradeCost", speedUpgradeCost);
            PlayerPrefs.Save();
            moneySpeedText.text = speedUpgradeCost.ToString();
            return true;
        }
        else
        {
            return false;
        }

    }

    private bool UpgradeAttackCost()
    {
        float ratio = CalculateRatioUpgrade(speedLevel, atribubesSO.progressionRateForUpgradeCost, atribubesSO.levelThresholdForUpgradeCost);
        bool isSubtract = SubtractMoney(attackUpgradeCost * ratio);
        if (isSubtract)
        {
            attackUpgradeCost = attackUpgradeCost * ratio;
            PlayerPrefs.SetFloat("attackUpgradeCost", attackUpgradeCost);
            PlayerPrefs.Save();
            moneyAtkText.text = attackUpgradeCost.ToString();
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool UpgradeIncomeCost()
    {
        float ratio = CalculateRatioUpgrade(speedLevel, atribubesSO.progressionRateForUpgradeCost, atribubesSO.levelThresholdForUpgradeCost);
        bool isSubtract = SubtractMoney(income * ratio);
        if (isSubtract)
        {
            incomeUpgradeCost = incomeUpgradeCost * ratio;
            PlayerPrefs.SetFloat("incomeUpgradeCost", incomeUpgradeCost);
            PlayerPrefs.Save();
            moneyIncomeText.text = incomeUpgradeCost.ToString();
            return true;
        }
        else
        {
            return false;
        }
    }

    public float CalculateRatioUpgrade(int lv, float progressionRate, float levelThreshold)
    {
        float exponent = Mathf.Floor((lv - 1) / levelThreshold);
        float ratio = atribubesSO.startRatio * Mathf.Pow(1 + progressionRate, exponent);
        // return ratio;
        return ratio = 1 + progressionRate;
    }


    #region Attack
    private void AttackUpgrade()
    {
        if (UpgradeAttackCost())
        {
            AttackAtributesUpdate();
            AttackLevelUpgrade();
        }
    }

    private void AttackAtributesUpdate()
    {
        float ratio = CalculateRatioUpgrade(atkLevel, atribubesSO.progressionRateForAttack, atribubesSO.levelThresholdForAttack);
        atk = atk * ratio;
        PlayerPrefs.SetFloat("atk", atk);
        PlayerPrefs.Save();
        atkText.text = atk.ToString();
    }
    private void AttackLevelUpgrade()
    {
        atkLevel += 1;
        PlayerPrefs.SetInt("atkLevel", atkLevel);
        PlayerPrefs.Save();
    }
    #endregion

    #region Speed
    private void SpeedUpgrade()
    {
        if (UpgradeSpeedCost())
        {
            SpeedAtributesUpdate();
            SpeedLevelUpgrade();
        }
    }
    private void SpeedAtributesUpdate()
    {
        float ratio = CalculateRatioUpgrade(speedLevel, atribubesSO.progressionRateForSpeed, atribubesSO.levelThresholdForSpeed);
        speed = speed * ratio;
        PlayerPrefs.SetFloat("speed", speed);
        PlayerPrefs.Save();
        speedText.text = speed.ToString();
    }
    private void SpeedLevelUpgrade()
    {
        speedLevel += 1;
        PlayerPrefs.SetInt("speedLevel", speedLevel);
        PlayerPrefs.Save();
    }
    #endregion

    #region Income
    private void IncomeUpgrade()
    {
        if (UpgradeIncomeCost())
        {
            IncomeAtributesUpgrade();
            IncomeLevelUpgrade();
        }
    }

    private void IncomeAtributesUpgrade()
    {

        float ratio = CalculateRatioUpgrade(incomeLevel, atribubesSO.progressionRateForIncome, atribubesSO.levelThresholdForIncome);
        income = income * ratio;
        PlayerPrefs.SetFloat("income", income);
        PlayerPrefs.Save();
        incomeText.text = income.ToString();
    }
    private void IncomeLevelUpgrade()
    {
        incomeLevel += 1;
        PlayerPrefs.SetInt("incomeLevel", incomeLevel);
        PlayerPrefs.Save();
    }
    #endregion

    public void AddMoney()
    {
        money += income;
        PlayerPrefs.SetFloat("money", money);
        PlayerPrefs.Save();
        moneyText.text = money.ToString();
    }

    private bool SubtractMoney(float amount)
    {
        if (money - amount < 0)
            return false;
        money -= amount;
        PlayerPrefs.SetFloat("money", money);
        PlayerPrefs.Save();
        moneyText.text = money.ToString();
        return true;
    }
}

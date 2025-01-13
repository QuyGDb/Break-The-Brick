
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

    [SerializeField] private int maxChopCount = 10;
    private int currentChopCount;
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

    public void TrackChopCount()
    {
        currentChopCount++;
    }

    private void Start()
    {
        currentChopCount = 0;
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
        float ratio = CalculateRatio(speedLevel, atribubesSO.startRatio);
        speedUpgradeCost = speedUpgradeCost * ratio;
        speedUpgradeCost = CalculateRatio(speedLevel, atribubesSO.baseMoneyForAtributes);
        moneySpeedText.text = speedUpgradeCost.ToString();
        return SubtractMoney(attackUpgradeCost);
    }

    private bool UpgradeAttackCost()
    {
        float ratio = CalculateRatio(atkLevel, atribubesSO.startRatio);
        attackUpgradeCost = attackUpgradeCost * ratio;
        attackUpgradeCost = CalculateRatio(atkLevel, atribubesSO.baseMoneyForAtributes);
        moneySpeedText.text = attackUpgradeCost.ToString();
        return SubtractMoney(attackUpgradeCost);
    }

    private bool UpgradeIncomeCost()
    {
        float ratio = CalculateRatio(incomeLevel, atribubesSO.startRatio);
        incomeUpgradeCost = incomeUpgradeCost * ratio;
        moneySpeedText.text = attackUpgradeCost.ToString();
        return SubtractMoney(attackUpgradeCost);
    }

    public float CalculateRatio(int lv, float x)
    {
        float exponent = Mathf.Floor((lv - 1) / 5f);
        float ratio = x * Mathf.Pow(1 + 0.25f, exponent);
        return ratio;
    }
    #region Attack
    private void AttackUpgrade()
    {

        if (UpgradeAttackCost())
        {

            moneyAtkText.text = attackUpgradeCost.ToString();
            AttackAtributesUpdate();
            AttackLevelUpgrade();
        }
    }

    private void AttackAtributesUpdate()
    {
        atk = atribubesSO.baseAttack * Mathf.Pow((1 * atribubesSO.atkGrowthRate), (atkLevel - 1));
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
            moneySpeedText.text = speedUpgradeCost.ToString();
            SpeedAtributesUpdate();
            SpeedLevelUpgrade();
        }
    }
    private void SpeedAtributesUpdate()
    {
        speed = atribubesSO.baseSpeed * Mathf.Pow((1 * atribubesSO.speedGrowthRate), (speedLevel - 1));
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
            moneyIncomeText.text = incomeUpgradeCost.ToString();
            IncomeAtributesUpgrade();
            IncomeLevelUpgrade();
        }
    }

    private void IncomeAtributesUpgrade()
    {

        income = atribubesSO.baseIncome * Mathf.Pow((1 * atribubesSO.incomeGrowthRate), (incomeLevel - 1));
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

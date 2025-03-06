
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
    public Atributes atributes;

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
        atributes = SaveLoadManager.LoadDataFromPlayerPrefs<Atributes>("atributes");
        if (atributes == null)
        {
            atributes = new Atributes();
        }
    }
    #endregion

    private bool UpgradeSpeedCost()
    {
        float ratio = CalculateRatioUpgrade(atributes.speedLevel, atribubesSO.progressionRateForUpgradeCost, atribubesSO.levelThresholdForUpgradeCost);
        bool isSubtract = SubtractMoney(atributes.speedUpgradeCost * ratio);
        if (isSubtract)
        {
            atributes.speedUpgradeCost = atributes.speedUpgradeCost * ratio;
            moneySpeedText.text = HelperUtilities.ToShortString(atributes.speedUpgradeCost);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool UpgradeAttackCost()
    {
        float ratio = CalculateRatioUpgrade(atributes.speedLevel, atribubesSO.progressionRateForUpgradeCost, atribubesSO.levelThresholdForUpgradeCost);
        bool isSubtract = SubtractMoney(atributes.attackUpgradeCost * ratio);
        if (isSubtract)
        {
            atributes.attackUpgradeCost = atributes.attackUpgradeCost * ratio;
            PlayerPrefs.Save();
            moneyAtkText.text = HelperUtilities.ToShortString(atributes.attackUpgradeCost);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool UpgradeIncomeCost()
    {
        float ratio = CalculateRatioUpgrade(atributes.speedLevel, atribubesSO.progressionRateForUpgradeCost, atribubesSO.levelThresholdForUpgradeCost);
        bool isSubtract = SubtractMoney(atributes.income * ratio);
        if (isSubtract)
        {
            atributes.incomeUpgradeCost = atributes.incomeUpgradeCost * ratio;
            PlayerPrefs.Save();
            moneyIncomeText.text = HelperUtilities.ToShortString(atributes.incomeUpgradeCost);
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
        return ratio = 1 + progressionRate;
    }

    #region Attack
    private void AttackUpgrade()
    {
        if (UpgradeAttackCost())
        {
            AttackAtributesUpdate();
            AttackLevelUpgrade();
            SaveLoadManager.SaveDataToPlayerPrefs("atributes", atributes);
        }
    }

    private void AttackAtributesUpdate()
    {
        float ratio = CalculateRatioUpgrade(atributes.atkLevel, atribubesSO.progressionRateForAttack, atribubesSO.levelThresholdForAttack);
        atributes.atk = atributes.atk * ratio;
        atkText.text = HelperUtilities.ToShortString(atributes.atk);
    }
    private void AttackLevelUpgrade()
    {
        atributes.atkLevel += 1;
    }
    #endregion

    #region Speed
    private void SpeedUpgrade()
    {
        if (UpgradeSpeedCost())
        {
            SpeedAtributesUpdate();
            SpeedLevelUpgrade();
            SaveLoadManager.SaveDataToPlayerPrefs("atributes", atributes);
        }
    }
    private void SpeedAtributesUpdate()
    {
        atributes.speed = atributes.speed * atribubesSO.speedCurve.Evaluate(atributes.speedLevel);
        speedText.text = HelperUtilities.ToShortString(atributes.speed);
    }
    private void SpeedLevelUpgrade()
    {
        atributes.speedLevel += 1;
    }
    #endregion

    #region Income
    private void IncomeUpgrade()
    {
        if (UpgradeIncomeCost())
        {
            IncomeAtributesUpgrade();
            IncomeLevelUpgrade();
            SaveLoadManager.SaveDataToPlayerPrefs("atributes", atributes);
        }
    }
    private void IncomeAtributesUpgrade()
    {

        float ratio = CalculateRatioUpgrade(atributes.incomeLevel, atribubesSO.progressionRateForIncome, atribubesSO.levelThresholdForIncome);
        atributes.income = atributes.income * ratio;
        incomeText.text = HelperUtilities.ToShortString(atributes.income);
    }
    private void IncomeLevelUpgrade()
    {
        atributes.incomeLevel += 1;
    }
    #endregion
    public void AddMoney()
    {
        atributes.money += atributes.income;
        moneyText.text = HelperUtilities.ToShortString(atributes.money);
    }

    private bool SubtractMoney(float amount)
    {
        if (atributes.money - amount < 0)
            return false;
        atributes.money -= amount;
        moneyText.text = HelperUtilities.ToShortString(atributes.money);
        return true;
    }
}

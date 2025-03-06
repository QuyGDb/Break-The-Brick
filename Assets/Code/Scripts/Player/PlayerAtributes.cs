
using TMPro;
using UnityEngine;
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
        ShowAtributesText();
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
            SaveLoadManager.SaveDataToPlayerPrefs("atributes", atributes);
        }
    }
    private void ShowAtributesText()
    {
        atkText.text = HelperUtilities.ToShortString(atributes.atk);
        speedText.text = HelperUtilities.ToShortString(atributes.speed);
        incomeText.text = HelperUtilities.ToShortString(atributes.income);
        moneyAtkText.text = HelperUtilities.ToShortString(atributes.attackUpgradeCost);
        moneySpeedText.text = HelperUtilities.ToShortString(atributes.speedUpgradeCost);
        moneyIncomeText.text = HelperUtilities.ToShortString(atributes.incomeUpgradeCost);
        moneyText.text = HelperUtilities.ToShortString(atributes.money);
    }
    #endregion

    private bool UpgradeSpeedCost()
    {
        bool isSubtract = SubtractMoney(atributes.speedUpgradeCost);
        if (isSubtract)
        {
            atributes.speedUpgradeCost = atributes.speedUpgradeCost * atribubesSO.multiplierForUpgradeCost;
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
        bool isSubtract = SubtractMoney(atributes.attackUpgradeCost);
        if (isSubtract)
        {
            atributes.attackUpgradeCost = atributes.attackUpgradeCost * atribubesSO.multiplierForUpgradeCost;
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
        bool isSubtract = SubtractMoney(atributes.income);
        if (isSubtract)
        {
            atributes.incomeUpgradeCost = atributes.incomeUpgradeCost * atribubesSO.multiplierForUpgradeCost;
            PlayerPrefs.Save();
            moneyIncomeText.text = HelperUtilities.ToShortString(atributes.incomeUpgradeCost);
            return true;
        }
        else
        {
            return false;
        }
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
        atributes.atk = atributes.atk * atribubesSO.multiplierForAttack;
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
        if (UpgradeSpeedCost() && atributes.speedLevel <= 20)
        {
            SpeedAtributesUpdate();
            SpeedLevelUpgrade();
            SaveLoadManager.SaveDataToPlayerPrefs("atributes", atributes);
        }
    }
    private void SpeedAtributesUpdate()
    {
        atributes.speed = atribubesSO.speedCurve.Evaluate(atributes.speedLevel);
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
        atributes.income = atributes.income * atribubesSO.multiplierForIncome;
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

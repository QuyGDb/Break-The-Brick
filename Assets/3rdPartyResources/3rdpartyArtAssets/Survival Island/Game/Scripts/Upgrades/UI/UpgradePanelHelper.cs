using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    public class UpgradePanelHelper
    {
        private List<UpgradeUIPanel> upgradeUIPanels;
        public List<UpgradeUIPanel> UpgradeUIPanels => upgradeUIPanels;

        private Pool upgradesUIPool;
        public Pool UpgradesUIPool => upgradesUIPool;

        private List<IUpgrade> upgrades;
        public List<IUpgrade> Upgrades => upgrades;

        private IUpgradePanel panel;

        public UpgradePanelHelper(IUpgradePanel panel)
        {
            this.panel = panel;

            upgradesUIPool = new Pool(new PoolSettings(panel.UpgradeUIPrefab.name, panel.UpgradeUIPrefab, 1, true, panel.ContentTransform));

            upgrades = new List<IUpgrade>();
            upgradeUIPanels = new List<UpgradeUIPanel>();
        }

        public void AddUpgrades(List<IUpgrade> upgrades)
        {
            for (int i = 0; i < upgrades.Count; i++)
            {
                AddUpgrade(upgrades[i]);
            }
        }

        public void AddUpgrade(IUpgrade upgrade)
        {
            GameObject upgradeUIObject = UpgradesUIPool.GetPooledObject();
            upgradeUIObject.transform.ResetLocal();
            upgradeUIObject.SetActive(true);

            UpgradeUIPanel upgradeUIPanel = upgradeUIObject.GetComponent<UpgradeUIPanel>();
            upgradeUIPanel.Initialise(upgrade);

            UpgradeUIPanels.Add(upgradeUIPanel);
            Upgrades.Add(upgrade);
        }

        public void Show()
        {
            for (int i = 0; i < UpgradeUIPanels.Count; i++)
            {
                UpgradeUIPanels[i].gameObject.SetActive(true);

                IUpgrade upgrade = UpgradeUIPanels[i].Upgrade;
                   
                if (upgrade.IsHighlighted)
                {
                    UpgradeUIPanels[i].BackgroundImage.color = panel.HighlightedColor;
                    UpgradeUIPanels[i].transform.SetAsFirstSibling();

                    var highlightedPanel = UpgradeUIPanels[i];
                    UpgradeUIPanels.RemoveAt(i);
                    UpgradeUIPanels.Insert(0, highlightedPanel);
                }
                else
                {
                    UpgradeUIPanels[i].BackgroundImage.color = panel.DefaultColor;
                }
            }
        }

        public void Redraw(bool animation)
        {
            for (int i = 0; i < UpgradeUIPanels.Count; i++)
            {
                UpgradeUIPanels[i].Redraw();

                if (animation)
                {
                    int index = i;

                    UpgradeUIPanels[i].CanvasGroup.alpha = 0.0f;

                    Tween.DelayedCall(0.2f + i * 0.09f, delegate
                    {
                        UpgradeUIPanels[index].CanvasGroup.DOFade(1.0f, 0.6f).SetEasing(Ease.Type.SineOut);
                    });
                }
                else
                {
                    UpgradeUIPanels[i].CanvasGroup.alpha = 1.0f;
                }
            }


        }

        public void Reset()
        {
            upgradesUIPool.ReturnToPoolEverything();
            UpgradeUIPanels.Clear();
            Upgrades.Clear();
        }

        public void OnUpgraded(GlobalUpgradeType upgradeType, AbstactGlobalUpgrade upgrade)
        {
            if (panel.ShowAllAfterUpgrade)
            {
                for (int i = 0; i < Upgrades.Count; i++)
                {
                    UpgradeUIPanels[i].BackgroundImage.color = panel.DefaultColor;
                    UpgradeUIPanels[i].gameObject.SetActive(true);
                    UpgradeUIPanels[i].Redraw();
                }

                panel.ShowAllAfterUpgrade = false;
            }
            else
            {
                for (int i = 0; i < Upgrades.Count; i++)
                {
                    UpgradeUIPanels[i].Redraw();
                }
            }
        }
    }
}
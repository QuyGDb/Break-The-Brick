using UnityEngine;

namespace Watermelon
{
    public sealed class CollectMission : Mission
    {
        [BoxGroup("Collect Mission Special", "Collect Mission Special")]
        [SerializeField] CurrencyType currencyType;
        public CurrencyType CurrencyType => currencyType;

        [BoxGroup("Collect Mission Special")]
        [SerializeField] int amount;

        private Save save;

        public override void Initialise()
        {
            base.Initialise();

            save = SaveController.GetSaveObject<Save>(GetSaveString());
            save.LinkMission(this);

            // Load mission stage
            missionStage = save.MissionStage;
        }

        public override void Activate()
        {
            base.Activate();

            isDirty = true;

            StartMission();

            CheckStage();

            CurrenciesController.SubscribeGlobalCallback(OnCurrencyPicked);
        }

        public override void Deactivate()
        {
            base.Deactivate();

            CurrenciesController.UnsubscribeGlobalCallback(OnCurrencyPicked);
        }

        private void OnCurrencyPicked(Currency currency, int amountDifference)
        {
            if (currencyType == currency.CurrencyType)
            {
                if (amountDifference > 0)
                {
                    isDirty = true;

                    save.CollectedItems += amountDifference;

                    CheckStage();
                }
            }
        }

        private void CheckStage()
        {
            if (missionStage == Stage.Active)
            {
                if (save.CollectedItems >= amount)
                {
                    FinishMission();
                }
            }
        }

        public override string GetFormattedProgress()
        {
            return string.Format("{0}/{1} <sprite name={2}>", save.CollectedItems, amount, currencyType);
        }

        public override float GetProgress()
        {
            switch (missionStage)
            {
                case Stage.Uninitialised:
                    return 0.0f;
                case Stage.Active:
                    return Mathf.InverseLerp(0, amount, save.CollectedItems);
                case Stage.Finished:
                    return 1.0f;
                case Stage.Collected:
                    return 1.0f;
            }

            return 0.0f;
        }

        [System.Serializable]
        public class Save : MissionSave
        {
            public int CollectedItems;
        }
    }
}
using UnityEngine;
using UnityEngine.Events;

namespace Watermelon
{
    public abstract class RewardsHolder : MonoBehaviour
    {
        [Group("Events")]
        [SerializeField] UnityEvent rewardReceived;
        public UnityEvent RewardReceived => rewardReceived;

        protected Reward[] rewards;

        protected void InitialiseComponents()
        {
            // Cache all assigned Reward components
            // Note: Custom editor hides these components in Inspector, but they are still there (you can see them by turing Debug mode in Inspector window)
            rewards = GetComponents<Reward>();

            // Initialise rewards
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].Initialise();
            }
        }

        protected void ApplyRewards()
        {
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].ApplyReward();
            }

            // Invoke purhase event
            rewardReceived?.Invoke();
        }
    }
}

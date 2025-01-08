using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Watermelon
{
    public class HealthBehavior : MonoBehaviour, IHealth
    {
        [SerializeField] HealthbarBehaviour healthbar;

        public float CurrentHealth { get; private set; }
        public float MaxHealth { get; private set; }

        public bool IsDepleted => CurrentHealth <= 0;
        public bool IsFull => CurrentHealth >= MaxHealth;

        public bool ShowOnChange { get; set; }
        public bool HideOnFull { get; set; }

        public void Initialise(float maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;

            healthbar.Initialise(transform, this, false);
        }

        private void Update()
        {
            healthbar.FollowUpdate();
        }

        public void Add(float value)
        {
            CurrentHealth += value;
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
                if (HideOnFull) healthbar.DisableBar();
            }

            healthbar.RedrawHealth();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPercent(float value)
        {
            Add(MaxHealth / 100f *  value);
        }

        public void Subtract(float value)
        {
            CurrentHealth -= value;

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                healthbar.DisableBar();
            } else
            {
                if (ShowOnChange && healthbar.IsDisabled) healthbar.EnableBar();
                healthbar.RedrawHealth();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubtractPercent(float value)
        {
            Subtract(MaxHealth / 100f * value);
        }

        public void Restore()
        {
            CurrentHealth = MaxHealth;
            healthbar.RedrawHealth();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Show()
        {
            healthbar.RedrawHealth();
            healthbar.EnableBar();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Hide()
        {
            healthbar.DisableBar();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForceHide()
        {
            healthbar.ForceDisable();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class DamageSource
    {
        private float damage;
        public float Damage => damage;

        private ICharacter characterSource;
        public ICharacter CharacterSource => characterSource;

        public bool IsPlayerASource => (characterSource != null) && characterSource.IsPlayer;

        public DamageSource(float damage, ICharacter characterSource)
        {
            this.damage = damage;
            this.characterSource = characterSource;
        }
    }
}
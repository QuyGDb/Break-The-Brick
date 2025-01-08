using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public interface ICharacter
    {
        Transform Transform { get; }
        bool IsPlayer { get; }
        bool IsDead { get; }

        public Collider CharacterCollider { get; }

        void TakeDamage(DamageSource source, Vector3 position, bool shouldFlash = false);
    }
}
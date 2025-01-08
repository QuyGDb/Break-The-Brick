using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public interface IUnlockable
    {
        void SpawnUnlocked();
        void SpanwNotUnlocked();

        void FullyUnlock();

        public void SetID(string id);
    }
}
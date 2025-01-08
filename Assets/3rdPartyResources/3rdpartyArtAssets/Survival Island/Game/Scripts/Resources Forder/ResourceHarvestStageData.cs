using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Resource Harvest Stage Data", menuName = "Content/Resource Harvest Stage Data")]
    public class ResourceHarvestStageData : ScriptableObject
    {
        [SerializeField] float weightMultiplier = 1;
        public float WeightMultiplier => weightMultiplier;

        [Space]
        [SerializeField] float drag = 10f;
        [SerializeField] float angularDrag = 0.5f;
        public float Drag => drag;
        public float AngularDrag => angularDrag;

        [Space]
        [SerializeField] bool useBildInGravity = true;
        [SerializeField, Tooltip("Additional to the bild in gravity, does not override it")] float customGravity = -40;
        public bool UseBildInGravity => useBildInGravity;
        public float CustomGravity => customGravity;

        [Space]
        [SerializeField] DuoFloat explosionForce = new DuoFloat(20 * 0.8f, 20 * 1.2f);
        [SerializeField] float explosionRadius;
        [SerializeField] float upwardForceModifier = 0.01f;
        public float UpwardForceModifier => upwardForceModifier;
        public float ExplosionRadius => explosionRadius;
        public DuoFloat ExplosionForce => explosionForce;

        [Space]
        [SerializeField] DuoVector3 angularForce = new DuoVector3(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));
        public DuoVector3 AngularForce => angularForce;

        [Space]
        [SerializeField] float scaleDelay = 0.3f;
        [SerializeField] float scaleDuration = 0.7f;
        public float ScaleDuration => scaleDuration;
        public float ScaleDelay => scaleDelay;

        
    }
}
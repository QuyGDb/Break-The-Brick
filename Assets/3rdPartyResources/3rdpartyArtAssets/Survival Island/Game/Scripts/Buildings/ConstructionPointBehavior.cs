using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [CustomOverlayElement("Construction UI", "OnToggleValueChanged")]
    public class ConstructionPointBehavior : AbstractHitableBehavior, IDistanceToggle, IWorldElement
    {
        public int InitialisationOrder => 0;

        public override int HittableID => 10000000;
        public override bool IsMutlipleObjectsHitRestricted => true;

        [Space]
        [SerializeField] bool isHelperTaskActive = true;
        public bool IsHelperTaskActive => isHelperTaskActive;

        [Space]
        [SerializeField] Canvas canvas;
        [SerializeField] Image constructionIconImage;
        [SerializeField] TMP_Text progressText;
        [SerializeField] protected string textFormat = "{0} / {1}";
        [SerializeField] float canvasHideDistance = 8f;

        [Space]
        [SerializeField] AudioClip constructionAudioClip;

        private ConstructingPointSave save;

        public int HitsMade
        {
            get => save.Value;
            set => save.Value = value;
        }

        public event SimpleCallback OnConstructed;
        public event SimpleCallback OnGotHit;

        public bool DistanceToggleActivated { get; private set; }
        public bool IsDistanceToggleInCloseMode { get; private set; }

        public float ActivationDistanceOfDT => canvasHideDistance;

        public bool IsBuilt => save.IsBought || HitsMade >= UnlocableComplex.ConstructionHitsRequired;

        public Vector3 OriginPositionOfDT => canvas.transform.position;

        private IUnlockableComplex UnlocableComplex { get; set; }

        private TweenCase canvasAppearCase;
        private Vector3 canvasDefaultScale;

        private ConstructionTask constructionTask;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        private UnlockableTool unlockableTool;

        private BoxCollider boxCollider;
        public BoxCollider BoxCollider => boxCollider;

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        public bool Init(IUnlockableComplex unlockableComplex)
        {
            UnlocableComplex = unlockableComplex;

            if (save == null)
            {
                save = SaveController.GetSaveObject<ConstructingPointSave>(unlockableComplex.ID + "_building_point");
            }

            if (save.IsBought || HitsMade >= unlockableComplex.ConstructionHitsRequired)
            {
                Destroy();
                return false;
            }

            InteractionAnimationData interactionData = PlayerBehavior.GetBehavior().PlayerGraphics.GetInteractionData(interactionAnimationType);
            if(interactionData.AnimationType == interactionAnimationType && interactionData.InteractionIcon != null)
            {
                constructionIconImage.sprite = interactionData.InteractionIcon;
            }
            progressText.text = string.Format(textFormat, HitsMade, unlockableComplex.ConstructionHitsRequired);
            canvasDefaultScale = canvas.transform.localScale;

            // Add object to distance toggle
            DistanceToggle.AddObject(this);

            constructionTask = new ConstructionTask(this);
            constructionTask.Activate();
            constructionTask.Register(LinkedWorldBehavior.TaskHandler);

            unlockableTool = UnlockableToolsController.GetUnlockableTool(interactionAnimationType);

            return true;
        }

        public bool LookUpConstructed(IUnlockableComplex unlockableComplex)
        {
            if (save == null)
            {
                save = SaveController.GetSaveObject<ConstructingPointSave>(unlockableComplex.ID + "_building_point");
            }

            return save.IsBought || HitsMade >= unlockableComplex.ConstructionHitsRequired;
        }

        public void OnWorldLoaded()
        {

        }

        public void OnWorldUnloaded()
        {

        }

        public override void GetHit(Vector3 hitSourcePosition, bool drop, IHitter resourcePicked = null)
        {
            if (IsBuilt)
                return;

            HitsMade += 1;

            // instantly completing if Actions -> Instant Construction is enabled in editor
            if (InstantConstructionActionMenu.IsConstructionInstant())
                HitsMade = UnlocableComplex.ConstructionHitsRequired;

            if (HitsMade >= UnlocableComplex.ConstructionHitsRequired)
            {
                if (constructionTask != null)
                    constructionTask.Disable();

                save.IsBought = true;

                PlayerBehavior.GetBehavior().OnHittableOutsideRangeOrCompleted(this);
                UnlocableComplex.Construct();
                OnConstructed?.Invoke();
            }
            else
            {
                progressText.text = string.Format(textFormat, HitsMade, UnlocableComplex.ConstructionHitsRequired);
            }

            if(constructionAudioClip != null)
                AudioController.PlaySound(constructionAudioClip, transform.position);

            EnergyController.OnConstructionHit();

            OnGotHit?.Invoke();
        }

        public override bool IsHittable()
        {
            if (unlockableTool != null)
                return unlockableTool.IsUnlocked;

            return true;
        }

        public void Enable()
        {
            IsActive = true;

            // the next 2 fields responcible for canvas hiding and showing depending on player's position
            DistanceToggleActivated = true;
            IsDistanceToggleInCloseMode = false;

            canvas.gameObject.SetActive(false);
        }

        public void Disable()
        {
            IsActive = false;

            // the next 2 fields responcible for canvas hiding and showing depending on player's position
            DistanceToggleActivated = false;
            IsDistanceToggleInCloseMode = false;

            canvas.gameObject.SetActive(false);
        }

        public void Destroy()
        {
            // remove object from distance toggle system
            DistanceToggle.RemoveObject(this);

            if (constructionTask != null)
            {
                constructionTask.Disable();
                constructionTask.Destroy();
            }

            Destroy(gameObject);
        }

        public void PlayerEnteredZone()
        {
            IsDistanceToggleInCloseMode = true;
            canvas.gameObject.SetActive(true);

            canvasAppearCase.KillActive();

            canvas.transform.localScale = canvasDefaultScale;
            canvasAppearCase = DistanceToggle.RunShowAnimation(canvas.transform);
        }

        public void PlayerLeavedZone()
        {
            IsDistanceToggleInCloseMode = false;

            canvasAppearCase.KillActive();
            canvasAppearCase = DistanceToggle.RunHideAnimation(canvas.transform, () =>
            {
                canvas.gameObject.SetActive(false);
            });
        }

        public Sprite GetConstructionIcon()
        {
            return constructionIconImage.sprite;
        }

        #region Development

        public void OnToggleValueChanged(bool enabled)
        {
            if (canvas == null)
                return;

            canvas.gameObject.SetActive(enabled);
        }

        public void UpdateCostInEditor(int hitsRequired)
        {
            progressText.text = string.Format(textFormat, 0, hitsRequired);
        }



        #endregion
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Watermelon
{
    public class Tween : MonoBehaviour
    {
        private static Tween instance;

        #region Update Tween
        private static int updateFramesCount;
        public static int UpdateFramesCount { get { return updateFramesCount; } }

        private static TweenCase[] updateTweens;
        public TweenCase[] UpdateTweens
        {
            get { return updateTweens; }
        }

        private static int updateTweensCount;

        private static bool hasActiveUpdateTweens = false;

        private static bool updateRequiresActiveReorganization = false;
        private static int updateReorganizeFromID = -1;
        private static int updateMaxActiveLookupID = -1;

        private static List<TweenCase> updateKillingTweens = new List<TweenCase>();

#if UNITY_EDITOR
        private static int maxUpdateTweensAmount = 0;
#endif
        #endregion

        #region Fixed Tween
        private static int fixedUpdateFramesCount;
        public static int FixedUpdateFramesCount { get { return fixedUpdateFramesCount; } }

        private static TweenCase[] fixedTweens;
        public TweenCase[] FixedTweens
        {
            get { return fixedTweens; }
        }

        private static int fixedTweensCount;

        private static bool hasActiveFixedTweens = false;

        private static bool fixedRequiresActiveReorganization = false;
        private static int fixedReorganizeFromID = -1;
        private static int fixedMaxActiveLookupID = -1;

        private static List<TweenCase> fixedKillingTweens = new List<TweenCase>();

#if UNITY_EDITOR
        private static int maxFixedUpdateTweensAmount = 0;
#endif
        #endregion

        #region Late Tween
        private static int lateUpdateFramesCount;
        public static int LateUpdateFramesCount { get { return lateUpdateFramesCount; } }

        private static TweenCase[] lateTweens;
        public TweenCase[] LateTweens
        {
            get { return lateTweens; }
        }

        private static int lateTweensCount;

        private static bool hasActiveLateTweens = false;

        private static bool lateRequiresActiveReorganization = false;
        private static int lateReorganizeFromID = -1;
        private static int lateMaxActiveLookupID = -1;

        private static List<TweenCase> lateKillingTweens = new List<TweenCase>();

#if UNITY_EDITOR
        private static int maxLateUpdateTweensAmount = 0;
#endif
        #endregion

        private bool systemLogs = false;

        private static TweenCaseCollection activeTweenCaseCollection;
        private static bool isActiveTweenCaseCollectionEnabled;

        /// <summary>
        /// Create tween instance.
        /// </summary>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;

                DontDestroyOnLoad(gameObject);
            }
#if UNITY_EDITOR
            else
            {
                if(systemLogs)
                    Debug.LogError("[Tween]: Tween already exists!");
            }
#endif
        }

        public void Initialise(int tweensUpdateCount, int tweensFixedUpdateCount, int tweensLateUpdateCount, bool systemLogs)
        {
            updateTweens = new TweenCase[tweensUpdateCount];
            fixedTweens = new TweenCase[tweensFixedUpdateCount];
            lateTweens = new TweenCase[tweensLateUpdateCount];

            this.systemLogs = systemLogs;
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (systemLogs)
                Debug.Log("[Tween]: Max amount of used tweens at the same time (Update - " + maxUpdateTweensAmount + "; Fixed - " + maxFixedUpdateTweensAmount + "; Late - " + maxLateUpdateTweensAmount + ")");
#endif
        }

        public static void AddTween(TweenCase tween, UpdateMethod tweenType)
        {
            switch (tweenType)
            {
                case UpdateMethod.Update:
                    if(updateTweensCount >= updateTweens.Length)
                    {
                        Array.Resize(ref updateTweens, updateTweens.Length + 50);

                        Debug.LogWarning("[Tween]: The amount of the tweens (update) was adjusted. Current size - " + updateTweens.Length + ". Change the default amount to prevent performance leak!");
                    }

                    if (updateRequiresActiveReorganization)
                        ReorganizeUpdateActiveTweens();

                    tween.IsActive = true;
                    tween.ActiveID = (updateMaxActiveLookupID = updateTweensCount);

                    updateTweens[updateTweensCount] = tween;
                    updateTweensCount++;

                    hasActiveUpdateTweens = true;

#if UNITY_EDITOR
                    if (maxUpdateTweensAmount < updateTweensCount)
                        maxUpdateTweensAmount = updateTweensCount;
#endif
                    break;
                case UpdateMethod.FixedUpdate:
                    if (fixedTweensCount >= fixedTweens.Length)
                    {
                        Array.Resize(ref fixedTweens, fixedTweens.Length + 50);

                        Debug.LogWarning("[Tween]: The amount of the tweens (fixed) was adjusted. Current size - " + fixedTweens.Length + ". Change the default amount to prevent performance leak!");
                    }

                    if (fixedRequiresActiveReorganization)
                        ReorganizeFixedActiveTweens();

                    tween.IsActive = true;
                    tween.ActiveID = (fixedMaxActiveLookupID = fixedTweensCount);

                    fixedTweens[fixedTweensCount] = tween;
                    fixedTweensCount++;

                    hasActiveFixedTweens = true;

#if UNITY_EDITOR
                    if (maxFixedUpdateTweensAmount < fixedTweensCount)
                        maxFixedUpdateTweensAmount = fixedTweensCount;
#endif
                    break;
                case UpdateMethod.LateUpdate:
                    if (lateTweensCount >= lateTweens.Length)
                    {
                        Array.Resize(ref lateTweens, lateTweens.Length + 50);

                        Debug.LogWarning("[Tween]: The amount of the tweens (late) was adjusted. Current size - " + lateTweens.Length + ". Change the default amount to prevent performance leak!");
                    }

                    if (lateRequiresActiveReorganization)
                        ReorganizeLateActiveTweens();

                    tween.IsActive = true;
                    tween.ActiveID = (lateMaxActiveLookupID = lateTweensCount);

                    lateTweens[lateTweensCount] = tween;
                    lateTweensCount++;

                    hasActiveLateTweens = true;

#if UNITY_EDITOR
                    if (maxLateUpdateTweensAmount < lateTweensCount)
                        maxLateUpdateTweensAmount = lateTweensCount;
#endif
                    break;
            }

            if (isActiveTweenCaseCollectionEnabled)
                activeTweenCaseCollection.AddTween(tween);
        }

        public static void Pause(UpdateMethod tweenType)
        {
            switch(tweenType)
            {
                case UpdateMethod.Update:
                    for (int i = 0; i < updateTweensCount; i++)
                    {
                        TweenCase tween = updateTweens[i];
                        if (tween != null)
                        {
                            tween.Pause();
                        }
                    }
                    break;
                case UpdateMethod.FixedUpdate:
                    for (int i = 0; i < fixedTweensCount; i++)
                    {
                        TweenCase tween = fixedTweens[i];
                        if (tween != null)
                        {
                            tween.Pause();
                        }
                    }
                    break;
                case UpdateMethod.LateUpdate:
                    for (int i = 0; i < lateTweensCount; i++)
                    {
                        TweenCase tween = lateTweens[i];
                        if (tween != null)
                        {
                            tween.Pause();
                        }
                    }
                    break;
            }
        }

        public static void PauseAll()
        {
            for (int i = 0; i < updateTweensCount; i++)
            {
                TweenCase tween = updateTweens[i];
                if (tween != null)
                {
                    tween.Pause();
                }
            }

            for (int i = 0; i < fixedTweensCount; i++)
            {
                TweenCase tween = fixedTweens[i];
                if (tween != null)
                {
                    tween.Pause();
                }
            }

            for (int i = 0; i < lateTweensCount; i++)
            {
                TweenCase tween = lateTweens[i];
                if (tween != null)
                {
                    tween.Pause();
                }
            }
        }

        public static void Resume(UpdateMethod tweenType)
        {
            switch (tweenType)
            {
                case UpdateMethod.Update:
                    for (int i = 0; i < updateTweensCount; i++)
                    {
                        TweenCase tween = updateTweens[i];
                        if (tween != null)
                        {
                            tween.Resume();
                        }
                    }
                    break;
                case UpdateMethod.FixedUpdate:
                    for (int i = 0; i < fixedTweensCount; i++)
                    {
                        TweenCase tween = fixedTweens[i];
                        if (tween != null)
                        {
                            tween.Resume();
                        }
                    }
                    break;
                case UpdateMethod.LateUpdate:
                    for (int i = 0; i < lateTweensCount; i++)
                    {
                        TweenCase tween = lateTweens[i];
                        if (tween != null)
                        {
                            tween.Resume();
                        }
                    }
                    break;
            }
        }

        public static void ResumeAll()
        {
            for (int i = 0; i < updateTweensCount; i++)
            {
                TweenCase tween = updateTweens[i];
                if (tween != null)
                {
                    tween.Resume();
                }
            }

            for (int i = 0; i < fixedTweensCount; i++)
            {
                TweenCase tween = fixedTweens[i];
                if (tween != null)
                {
                    tween.Resume();
                }
            }

            for (int i = 0; i < lateTweensCount; i++)
            {
                TweenCase tween = lateTweens[i];
                if (tween != null)
                {
                    tween.Resume();
                }
            }
        }

        public static void Remove(UpdateMethod tweenType)
        {
            switch (tweenType)
            {
                case UpdateMethod.Update:
                    for (int i = 0; i < updateTweensCount; i++)
                    {
                        TweenCase tween = updateTweens[i];
                        if (tween != null)
                        {
                            tween.Kill();
                        }
                    }
                    break;
                case UpdateMethod.FixedUpdate:
                    for (int i = 0; i < fixedTweensCount; i++)
                    {
                        TweenCase tween = fixedTweens[i];
                        if (tween != null)
                        {
                            tween.Kill();
                        }
                    }
                    break;
                case UpdateMethod.LateUpdate:
                    for (int i = 0; i < lateTweensCount; i++)
                    {
                        TweenCase tween = lateTweens[i];
                        if (tween != null)
                        {
                            tween.Kill();
                        }
                    }
                    break;
            }
        }

        public static void RemoveAll()
        {
            for (int i = 0; i < updateTweensCount; i++)
            {
                TweenCase tween = updateTweens[i];
                if (tween != null)
                {
                    tween.Kill();
                }
            }

            for (int i = 0; i < fixedTweensCount; i++)
            {
                TweenCase tween = fixedTweens[i];
                if (tween != null)
                {
                    tween.Kill();
                }
            }

            for (int i = 0; i < lateTweensCount; i++)
            {
                TweenCase tween = lateTweens[i];
                if (tween != null)
                {
                    tween.Kill();
                }
            }
        }

        private void Update()
        {
            updateFramesCount++;

            if (!hasActiveUpdateTweens)
                return;

            if (updateRequiresActiveReorganization)
                ReorganizeUpdateActiveTweens();

            float deltaTime = Time.deltaTime;
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            for (int i = 0; i < updateTweensCount; i++)
            {
                TweenCase tween = updateTweens[i];
                if (tween != null)
                {
                    if(!tween.Validate())
                    {
                        tween.Kill();
                    }
                    else
                    {
                        if (tween.IsActive && !tween.IsPaused)
                        {
                            if (!tween.IsUnscaled)
                            {
                                if (Time.timeScale == 0)
                                    continue;

                                if(tween.Delay > 0 && tween.Delay > tween.CurrentDelay)
                                {
                                    tween.UpdateDelay(deltaTime);
                                }
                                else
                                {
                                    tween.UpdateState(deltaTime);

                                    tween.Invoke(deltaTime);
                                }
                            }
                            else
                            {
                                if (tween.Delay > 0 && tween.Delay > tween.CurrentDelay)
                                {
                                    tween.UpdateDelay(unscaledDeltaTime);
                                }
                                else
                                {
                                    tween.UpdateState(unscaledDeltaTime);

                                    tween.Invoke(unscaledDeltaTime);
                                }
                            }

                            if (tween.IsCompleted)
                            {
                                tween.DefaultComplete();

                                tween.InvokeCompleteEvent();

                                tween.Kill();
                            }
                        }
                    }
                }
            }

            int killingTweensCount = updateKillingTweens.Count - 1;
            for (int i = killingTweensCount; i > -1; i--)
            {
                RemoveActiveTween(updateKillingTweens[i]);
            }
            updateKillingTweens.Clear();
        }

        private void FixedUpdate()
        {
            fixedUpdateFramesCount++;

            if (!hasActiveFixedTweens)
                return;

            if (fixedRequiresActiveReorganization)
                ReorganizeFixedActiveTweens();

            float deltaTime = Time.fixedDeltaTime;
            float unscaledDeltaTime = Time.fixedUnscaledDeltaTime;

            for (int i = 0; i < fixedTweensCount; i++)
            {
                TweenCase tween = fixedTweens[i];
                if (tween != null)
                {
                    if (!tween.Validate())
                    {
                        tween.Kill();
                    }
                    else
                    {
                        if (tween.IsActive && !tween.IsPaused)
                        {
                            if (!tween.IsUnscaled)
                            {
                                if (Time.timeScale == 0)
                                    continue;

                                tween.UpdateState(deltaTime);

                                tween.Invoke(deltaTime);
                            }
                            else
                            {
                                tween.UpdateState(unscaledDeltaTime);

                                tween.Invoke(unscaledDeltaTime);
                            }

                            if (tween.IsCompleted)
                            {
                                tween.DefaultComplete();

                                tween.InvokeCompleteEvent();

                                tween.Kill();
                            }
                        }
                    }
                }
            }

            int killingTweensCount = fixedKillingTweens.Count - 1;
            for (int i = killingTweensCount; i > -1; i--)
            {
                RemoveActiveTween(fixedKillingTweens[i]);
            }
            fixedKillingTweens.Clear();
        }

        private void LateUpdate()
        {
            lateUpdateFramesCount++;

            if (!hasActiveLateTweens)
                return;

            if (lateRequiresActiveReorganization)
                ReorganizeLateActiveTweens();

            float deltaTime = Time.deltaTime;
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            for (int i = 0; i < lateTweensCount; i++)
            {
                TweenCase tween = lateTweens[i];
                if (tween != null)
                {
                    if (!tween.Validate())
                    {
                        tween.Kill();
                    }
                    else
                    {
                        if (tween.IsActive && !tween.IsPaused)
                        {
                            if (!tween.IsUnscaled)
                            {
                                if (Time.timeScale == 0)
                                    continue;

                                tween.UpdateState(deltaTime);

                                tween.Invoke(deltaTime);
                            }
                            else
                            {
                                tween.UpdateState(unscaledDeltaTime);

                                tween.Invoke(unscaledDeltaTime);
                            }

                            if (tween.IsCompleted)
                            {
                                tween.DefaultComplete();

                                tween.InvokeCompleteEvent();

                                tween.Kill();
                            }
                        }
                    }
                }
            }

            int killingTweensCount = lateKillingTweens.Count - 1;
            for (int i = killingTweensCount; i > -1; i--)
            {
                RemoveActiveTween(lateKillingTweens[i]);
            }
            lateKillingTweens.Clear();
        }

        private static void ReorganizeUpdateActiveTweens()
        {
            if (updateTweensCount <= 0)
            {
                updateMaxActiveLookupID = -1;
                updateReorganizeFromID = -1;
                updateRequiresActiveReorganization = false;

                return;
            }

            if (updateReorganizeFromID == updateMaxActiveLookupID)
            {
                updateMaxActiveLookupID--;
                updateReorganizeFromID = -1;
                updateRequiresActiveReorganization = false;

                return;
            }

            int defaultOffset = 1;
            int tweensTempCount = updateMaxActiveLookupID + 1;

            updateMaxActiveLookupID = updateReorganizeFromID - 1;

            for (int i = updateReorganizeFromID + 1; i < tweensTempCount; i++)
            {
                TweenCase tween = updateTweens[i];
                if (tween != null)
                {
                    tween.ActiveID = (updateMaxActiveLookupID = i - defaultOffset);

                    updateTweens[i - defaultOffset] = tween;
                    updateTweens[i] = null;
                }
                else
                {
                    defaultOffset++;
                }
            }

            updateRequiresActiveReorganization = false;
            updateReorganizeFromID = -1;
        }

        private static void ReorganizeFixedActiveTweens()
        {
            if (fixedTweensCount <= 0)
            {
                fixedMaxActiveLookupID = -1;
                fixedReorganizeFromID = -1;
                fixedRequiresActiveReorganization = false;

                return;
            }

            if (fixedReorganizeFromID == fixedMaxActiveLookupID)
            {
                fixedMaxActiveLookupID--;
                fixedReorganizeFromID = -1;
                fixedRequiresActiveReorganization = false;

                return;
            }

            int defaultOffset = 1;
            int tweensTempCount = fixedMaxActiveLookupID + 1;

            fixedMaxActiveLookupID = fixedReorganizeFromID - 1;

            for (int i = fixedReorganizeFromID + 1; i < tweensTempCount; i++)
            {
                TweenCase tween = fixedTweens[i];
                if (tween != null)
                {
                    tween.ActiveID = (fixedMaxActiveLookupID = i - defaultOffset);

                    fixedTweens[i - defaultOffset] = tween;
                    fixedTweens[i] = null;
                }
                else
                {
                    defaultOffset++;
                }
            }

            fixedRequiresActiveReorganization = false;
            fixedReorganizeFromID = -1;
        }

        private static void ReorganizeLateActiveTweens()
        {
            if (lateTweensCount <= 0)
            {
                lateMaxActiveLookupID = -1;
                lateReorganizeFromID = -1;
                lateRequiresActiveReorganization = false;

                return;
            }

            if (lateReorganizeFromID == lateMaxActiveLookupID)
            {
                lateMaxActiveLookupID--;
                lateReorganizeFromID = -1;
                lateRequiresActiveReorganization = false;

                return;
            }

            int defaultOffset = 1;
            int tweensTempCount = lateMaxActiveLookupID + 1;

            lateMaxActiveLookupID = lateReorganizeFromID - 1;

            for (int i = lateReorganizeFromID + 1; i < tweensTempCount; i++)
            {
                TweenCase tween = lateTweens[i];
                if (tween != null)
                {
                    tween.ActiveID = (lateMaxActiveLookupID = i - defaultOffset);

                    lateTweens[i - defaultOffset] = tween;
                    lateTweens[i] = null;
                }
                else
                {
                    defaultOffset++;
                }
           }

            lateRequiresActiveReorganization = false;
            lateReorganizeFromID = -1;
        }

        public static void MarkForKilling(TweenCase tween)
        {
            switch(tween.UpdateMethod)
            {
                case UpdateMethod.Update:
                    updateKillingTweens.Add(tween);
                    break;
                case UpdateMethod.FixedUpdate:
                    fixedKillingTweens.Add(tween);
                    break;
                case UpdateMethod.LateUpdate:
                    lateKillingTweens.Add(tween);
                    break;
            }
        }

        private void RemoveActiveTween(TweenCase tween)
        {
            int activeId = tween.ActiveID;
            tween.ActiveID = -1;

            switch (tween.UpdateMethod)
            {
                case UpdateMethod.Update:
                    updateRequiresActiveReorganization = true;

                    if (updateReorganizeFromID == -1 || updateReorganizeFromID > activeId)
                    {
                        updateReorganizeFromID = activeId;
                    }

                    updateTweens[activeId] = null;

                    updateTweensCount--;
                    hasActiveUpdateTweens = (updateTweensCount > 0);
                    break;
                case UpdateMethod.FixedUpdate:
                    fixedRequiresActiveReorganization = true;

                    if (fixedReorganizeFromID == -1 || fixedReorganizeFromID > activeId)
                    {
                        fixedReorganizeFromID = activeId;
                    }

                    fixedTweens[activeId] = null;

                    fixedTweensCount--;
                    hasActiveFixedTweens = (fixedTweensCount > 0);
                    break;
                case UpdateMethod.LateUpdate:
                    lateRequiresActiveReorganization = true;

                    if (lateReorganizeFromID == -1 || lateReorganizeFromID > activeId)
                    {
                        lateReorganizeFromID = activeId;
                    }

                    lateTweens[activeId] = null;

                    lateTweensCount--;
                    hasActiveLateTweens = (lateTweensCount > 0);
                    break;
            }
        }

        public static TweenCaseCollection BeginTweenCaseCollection()
        {
            isActiveTweenCaseCollectionEnabled = true;

            activeTweenCaseCollection = new TweenCaseCollection();

            return activeTweenCaseCollection;
        }

        public static void EndTweenCaseCollection()
        {
            isActiveTweenCaseCollectionEnabled = false;
            activeTweenCaseCollection = null;
        }

        #region Custom Tweens
        /// <summary>
        /// Delayed call of delegate.
        /// </summary>
        /// <param name="callback">Callback to call.</param>
        /// <param name="delay">Delay in seconds.</param>
        public static TweenCase DelayedCall(float delay, SimpleCallback callback, bool unscaledTime = false, UpdateMethod tweenType = UpdateMethod.Update)
        {
            if (delay <= 0)
            {
                callback?.Invoke();
                return null;
            }
            else
            {
                return new SystemTweenCases.Default().SetDuration(delay).SetUnscaledMode(unscaledTime).OnComplete(callback).SetUpdateMethod(tweenType).StartTween();
            }
        }

        /// <summary>
        /// Interpolate float value
        /// </summary>
        public static TweenCase DoColor(Color startValue, Color resultValue, float time, SystemTweenCases.ColorCase.TweenColorCallback callback, bool unscaledTime = false, UpdateMethod tweenType = UpdateMethod.Update)
        {
            return new SystemTweenCases.ColorCase(startValue, resultValue, callback).SetDuration(time).SetUnscaledMode(unscaledTime).SetUpdateMethod(tweenType).StartTween();
        }

        /// <summary>
        /// Interpolate float value
        /// </summary>
        public static TweenCase DoFloat(float startValue, float resultValue, float time, SystemTweenCases.Float.TweenFloatCallback callback, float delay = 0.0f, bool unscaledTime = false, UpdateMethod tweenType = UpdateMethod.Update)
        {
            return new SystemTweenCases.Float(startValue, resultValue, callback).SetDelay(delay).SetDuration(time).SetUnscaledMode(unscaledTime).SetUpdateMethod(tweenType).StartTween();
        }

        /// <summary>
        /// Wait for condition
        /// </summary>
        public static TweenCase DoWaitForCondition(SystemTweenCases.Condition.TweenConditionCallback callback, bool unscaledTime = false, UpdateMethod tweenType = UpdateMethod.Update)
        {
            return new SystemTweenCases.Condition(callback).SetDuration(float.MaxValue).SetUnscaledMode(unscaledTime).SetUpdateMethod(tweenType).StartTween();
        }

        /// <summary>
        /// Call function in next frame
        /// </summary>
        public static TweenCase NextFrame(SimpleCallback callback, int framesOffset = 1, bool unscaledTime = false, UpdateMethod updateMethod = UpdateMethod.Update)
        {
            switch (updateMethod)
            {
                case UpdateMethod.Update:
                    return new SystemTweenCases.UpdateNextFrame(callback, framesOffset).SetDuration(float.MaxValue).SetUnscaledMode(unscaledTime).SetUpdateMethod(updateMethod).StartTween();
                case UpdateMethod.FixedUpdate:
                    return new SystemTweenCases.FixedUpdateNextFrame(callback, framesOffset).SetDuration(float.MaxValue).SetUnscaledMode(unscaledTime).SetUpdateMethod(updateMethod).StartTween();
                case UpdateMethod.LateUpdate:
                    return new SystemTweenCases.LateUpdateNextFrame(callback, framesOffset).SetDuration(float.MaxValue).SetUnscaledMode(unscaledTime).SetUpdateMethod(updateMethod).StartTween();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Invoke coroutine from non-monobehavior script
        /// </summary>
        public static Coroutine InvokeCoroutine(IEnumerator enumerator)
        {
            return instance.StartCoroutine(enumerator);
        }

        /// <summary>
        /// Stop custom coroutine
        /// </summary>
        public static void StopCustomCoroutine(Coroutine coroutine)
        {
            instance.StopCoroutine(coroutine);
        }
        #endregion

        public static void DestroyObject()
        {
            // Stop all coroutines
            if(instance != null)
                instance.StopAllCoroutines();

            // Reset all tweens
            for (int i = 0; i < updateTweensCount; i++)
            {
                updateTweens[i] = null;
            }
            updateTweensCount = 0;

            for (int i = 0; i < fixedTweensCount; i++)
            {
                fixedTweens[i] = null;
            }
            fixedTweensCount = 0;

            for (int i = 0; i < lateTweensCount; i++)
            {
                lateTweens[i] = null;
            }
            lateTweensCount = 0;
        }
    }

    public enum UpdateMethod
    {
        Update,
        FixedUpdate,
        LateUpdate
    }
}


// -----------------
// Tween v 1.4
// -----------------

// Changelog
// v 1.4
// • Restructured tween cases and extensions
// • Added KillActive, CompleteActive extensions for arrays
// v 1.3.3
// • Added null check for custom easings
// • Added DOMove(Transform) method
// v 1.3.2
// • Added Trasform.DoRotateConstant
// v 1.3.1
// • Added TextMeshPro define
// v 1.3
// • Added camera fov tween
// • Added transform follow tween
// • Added transform ping-pong scale tween
// • Added TextMeshPro tweens
// v 1.2
// • Added float deltaTime to Invoke method
// • Added regions for Tween Cases by the type
// • Added Shake, AnchoredPosition Shake, 
// v 1.1f3
// • Added Material float tween
// v 1.1f2
// • Added TweenCaseCollection
// v 1.1
// • Check out of range
// v 1.0
// • Added basic version
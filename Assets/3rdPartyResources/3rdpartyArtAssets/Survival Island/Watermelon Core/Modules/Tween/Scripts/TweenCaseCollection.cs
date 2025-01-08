using System.Collections.Generic;

namespace Watermelon
{
    public class TweenCaseCollection
    {
        private List<TweenCase> tweenCases = new List<TweenCase>();
        public List<TweenCase> TweenCases => tweenCases;

        private SimpleCallback tweensCompleted;

        public void AddTween(TweenCase tweenCase)
        {
            tweenCase.OnComplete(OnTweenCaseComplete);

            tweenCases.Add(tweenCase);
        }

        public bool IsComplete()
        {
            for(int i = 0; i < tweenCases.Count; i++)
            {
                if (!tweenCases[i].IsCompleted)
                    return false;
            }

            return true;
        }

        public void Complete()
        {
            for (int i = 0; i < tweenCases.Count; i++)
            {
                tweenCases[i].Complete();
            }
        }

        public void Kill()
        {
            for (int i = 0; i < tweenCases.Count; i++)
            {
                tweenCases[i].Kill();
            }
        }

        public void OnComplete(SimpleCallback callback)
        {
            tweensCompleted += callback;
        }

        private void OnTweenCaseComplete()
        {
            for (int i = 0; i < tweenCases.Count; i++)
            {
                if (!tweenCases[i].IsCompleted)
                    return;
            }

            if (tweensCompleted != null)
                tweensCompleted.Invoke();
        }

        public static TweenCaseCollection operator +(TweenCaseCollection caseCollection, TweenCase tweenCase)
        {
            caseCollection.AddTween(tweenCase);

            return caseCollection;
        }
    }
}

using UnityEngine;

namespace Watermelon
{
    public class CharacterGraphicsHolder<T> where T : MonoBehaviour
    {
        protected GameObject activePrefab;
        private GameObject playerPrefabGraphics;

        private T characterGraphics;
        public T CharacterGraphics => characterGraphics;

        private ICharacterGraphics<T> holder;

        private TweenCase scaleTweenCase;

        public void Initialise(ICharacterGraphics<T> holder, GameObject defaultGraphics = null)
        {
            this.holder = holder;

            if (defaultGraphics != null)
                SetGraphics(defaultGraphics);
        }

        public void SetGraphics(GameObject graphics)
        {
            // Check if graphics isn't exist already
            if (playerPrefabGraphics != graphics)
            {
                // Store prefab link
                playerPrefabGraphics = graphics;

                if (characterGraphics != null)
                {
                    // Unload graphics based data
                    holder.OnGraphicsUnloaded(characterGraphics);

                    Object.Destroy(characterGraphics.gameObject);
                }

                GameObject graphicObject = Object.Instantiate(graphics);
                graphicObject.transform.SetParent(holder.Transform);
                graphicObject.transform.localPosition = Vector3.zero;
                graphicObject.transform.localRotation = Quaternion.identity;
                graphicObject.SetActive(true);

                characterGraphics = graphicObject.GetComponent<T>();

                holder.OnGraphicsUpdated(characterGraphics);
            }
        }

        public void LinkGraphics(T graphics)
        {
            playerPrefabGraphics = null;

            if (characterGraphics != null)
            {
                // Unload graphics based data
                holder.OnGraphicsUnloaded(characterGraphics);

                Object.Destroy(characterGraphics.gameObject);
            }

            characterGraphics = graphics;

            holder.OnGraphicsUpdated(characterGraphics);
        }

        public void SetGraphicsState(bool state)
        {
            characterGraphics.gameObject.SetActive(state);
        }

        public void PlaySpawnAnimation()
        {
            if (characterGraphics == null)
                return;

            scaleTweenCase.KillActive();

            characterGraphics.transform.localScale = Vector3.zero;
            characterGraphics.transform.DOScale(1.0f, 0.3f).SetEasing(Ease.Type.BackOut);
        }
    }
}
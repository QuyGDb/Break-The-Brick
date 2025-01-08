using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Save Controller", Core = true)]
    public class SaveInitModule : InitModule
    {
        public override string ModuleName => "Save Controller";

        [SerializeField] bool useAutoSave = false;

        public override void CreateComponent(GameObject holderObject)
        {
            SaveController.Initialise(useAutoSave);
        }
    }
}

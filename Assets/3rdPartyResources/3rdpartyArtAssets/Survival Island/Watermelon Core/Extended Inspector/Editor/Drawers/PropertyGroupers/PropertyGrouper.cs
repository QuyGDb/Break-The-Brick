using System;

namespace Watermelon
{
    public abstract class PropertyGrouper
    {
        public abstract void BeginGroup(WatermelonEditor editor, string groupID, string label);
        public abstract void EndGroup();

        public virtual bool DrawRenderers(WatermelonEditor editor, string groupID) { return true; }
    }
}

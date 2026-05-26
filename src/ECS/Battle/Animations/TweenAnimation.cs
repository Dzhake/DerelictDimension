using Friflo.Engine.ECS;
using MLEM.Maths;

namespace DerelictDimension.ECS.Battle.Animations;

public class TweenAnimation<TComponent, TField, TLerper> where TComponent : struct, IComponent where TLerper : ILerper<TField>, new()
{
    public Tween<TComponent, TField, TLerper> tween;

    public TweenAnimation(ref TComponent component, ref TField field, Entity entity, TField to, float totalTime, Easings.Easing? easingFunc = null, TLerper? lerper = default)
    {
        tween = new(ref component, ref field, entity, to, totalTime, easingFunc, lerper);
    }

    public bool Update()
    {
        tween.Update();
        return tween.Finished;
    }
}

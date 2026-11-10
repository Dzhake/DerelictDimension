using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Drawing;

public class YSortSystem : SystemBase
{
    public override void Update(SystemCtx ctx)
    {
        foreach (var entity in ctx.GetQuery(Queries.YSortSelfTag).Entities)
        {
            var data = entity.Data;
            if (data.Has<RenderDepth>())
            {
                ref var renderDepth = ref data.Get<RenderDepth>();
                ref var transform = ref data.Ensure<Transform2D>(ctx.cb);
                renderDepth.YSortOffset = -transform.PosY;
            }
        }

        //TODO (dev qol: mid) make data.Ensure() return already added commands if that component was already added.
        ctx.cb.Playback();

        foreach (var entity in ctx.GetQuery(Queries.YSortParentTag).Entities)
        {
            var data = entity.Data;
            ref var renderDepth = ref data.Ensure<RenderDepth>(ctx.cb);
            var yAnchorEnt = entity.GetFirstEntityWithTag<YSortSelfTag>();
            if (yAnchorEnt.IsNull) continue;
            renderDepth.YSortOffset = yAnchorEnt.Data.Ensure<RenderDepth>(ctx.cb).YSortOffset;
        }
    }
}

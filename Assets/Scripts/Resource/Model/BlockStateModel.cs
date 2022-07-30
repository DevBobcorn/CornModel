using System.Collections.Generic;
using MinecraftClient.Rendering;

namespace MinecraftClient.Resource
{
    public class BlockStateModel
    {
        public readonly List<BlockGeometry> Geometries = new List<BlockGeometry>();
        private RenderType renderType = RenderType.SOLID;
        public RenderType RenderType { get { return renderType; } }

        public BlockStateModel(List<BlockGeometry> geometries)
        {
            Geometries = geometries;
        }

        public void SetRenderType(RenderType renderType)
        {
            this.renderType = renderType;
        }

    }

}
using UnityEngine;

namespace MinecraftClient.Resource
{
    public struct TextureInfo
    {
        public Rect bounds;
        public int index;

        // More than 1 if the texture is animated
        public int frameCount;
        // Duration of each frame
        public float frameInterval;

        public TextureInfo(Rect bounds, int index, int fc = 1, float fi = 1)
        {
            this.bounds = bounds;
            this.index = index;
            this.frameCount = fc;
            this.frameInterval = fi;
        }
    }
}
using Unity.Mathematics;

namespace MinecraftClient.Resource
{
    public class ItemGeometry
    {
        public readonly bool isGenerated = false;
        private readonly float3[] vertexArr;
        private float3[] uvArr;
        private int[] tintIndexArr;

        public ItemGeometry(float3[] vArr, float3[] uvArr, int[] tArr, bool isGenerated)
        {
            this.vertexArr = vArr;
            this.uvArr = uvArr;
            this.tintIndexArr = tArr;
            this.isGenerated = isGenerated;
        }

        public void Build(ref (float3[] vert, float3[] txuv, float3[] tint) buffer, float3 posOffset, float3[] itemTints)
        {
            int vertexCount = buffer.vert.Length + vertexArr.Length;

            var verts = new float3[vertexCount];
            var txuvs = new float3[vertexCount];
            var tints = new float3[vertexCount];

            buffer.vert.CopyTo(verts, 0);
            buffer.txuv.CopyTo(txuvs, 0);
            buffer.tint.CopyTo(tints, 0);

            uint i, vertOffset = (uint)buffer.vert.Length;

            if (vertexArr.Length > 0)
            {
                for (i = 0U;i < vertexArr.Length;i++)
                {
                    verts[i + vertOffset] = vertexArr[i] + posOffset;
                    tints[i + vertOffset] = tintIndexArr[i] >= 0 && tintIndexArr[i] < itemTints.Length ? itemTints[tintIndexArr[i]] : BlockGeometry.DEFAULT_COLOR;
                }
                uvArr.CopyTo(txuvs, vertOffset);
                vertOffset += (uint)vertexArr.Length;
            }

            buffer.vert = verts;
            buffer.txuv = txuvs;
            buffer.tint = tints;

        }
    }
}
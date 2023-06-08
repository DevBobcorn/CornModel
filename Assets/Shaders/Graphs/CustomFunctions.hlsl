#ifndef CORN_FUNCTIONS_INCLUDED
#define CORN_FUNCTIONS_INCLUDED

void GetTexUV_float(float AnimTime, float4 AnimInfo, float4 BaseUV, out float2 TexUV, out float TexIndex)
{
    float frameCount = AnimInfo.x;

    if (frameCount > 1) {
        float frameInterval = AnimInfo.y;

        float cycleTime = fmod(AnimTime, frameInterval * frameCount);
        uint curFrame = floor(cycleTime / frameInterval);
        uint framePerRow = round(AnimInfo.w);
        
        TexUV = float2(BaseUV.x + (curFrame % framePerRow) * AnimInfo.z,
                BaseUV.y - (curFrame / framePerRow) * AnimInfo.z);
    } else {
        TexUV = BaseUV.xy;
    }

    TexIndex = BaseUV.z;
}

#endif
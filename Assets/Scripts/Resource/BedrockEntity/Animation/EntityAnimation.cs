using System.Collections.Generic;

namespace CraftSharp.Resource
{
    public enum LoopType
    {
        Loop,
        Stop,
        HoldOnLastFrame
    }

    public class EntityAnimation
    {
        public readonly LoopType Loop;
        public readonly float AnimationLength;

        public readonly Dictionary<string, EntityBoneAnimation> BoneAnimations;

        private EntityAnimation(LoopType loop, float length, Dictionary<string, EntityBoneAnimation> anims)
        {
            Loop = loop;
            AnimationLength = length;
            BoneAnimations = anims;
        }

        public static EntityAnimation FromJson(Json.JSONData data)
        {
            LoopType loop;
            if (data.Properties.ContainsKey("loop"))
            {
                loop = data.Properties["loop"].StringValue.ToLower() switch
                {
                    "true" => LoopType.Loop,
                    "false" => LoopType.Stop,
                    "hold_on_last_frame" => LoopType.HoldOnLastFrame,

                    _ => LoopType.HoldOnLastFrame
                };
            }
            else
            {
                loop = LoopType.HoldOnLastFrame;
            }

            float length = 0F;
            if (data.Properties.ContainsKey("animation_length"))
            {
                length = float.Parse(data.Properties["animation_length"].StringValue);
            }

            Dictionary<string, EntityBoneAnimation> anims = new();
            if (data.Properties.ContainsKey("bones"))
            {
                foreach (var pair in data.Properties["bones"].Properties)
                {
                    anims.Add(pair.Key, EntityBoneAnimation.FromJson(pair.Value));
                }
            }

            return new EntityAnimation(loop, length, anims);
        }
    }
}
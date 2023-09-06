#nullable enable
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

using CraftSharp.Molang.Runtime;

namespace CraftSharp.Resource
{
    public class EntityBoneAnimation
    {
        public readonly (float time, MolangVector3 pre, MolangVector3 post)[]? translationKeyframes;
        public readonly (float time, MolangVector3 pre, MolangVector3 post)[]? scaleKeyframes;
        public readonly (float time, MolangVector3 pre, MolangVector3 post)[]? rotationKeyframes;

        private EntityBoneAnimation((float time, MolangVector3 pre, MolangVector3 post)[]? t,
                (float time, MolangVector3 pre, MolangVector3 post)[]? s,
                (float time, MolangVector3 pre, MolangVector3 post)[]? r)
        {
            translationKeyframes = t;
            scaleKeyframes = s;
            rotationKeyframes = r;
        }

        public static EntityBoneAnimation FromJson(Json.JSONData data)
        {
            (float time, MolangVector3 pre, MolangVector3 post)[]? t = null, s = null, r = null;

            if (data.Properties.ContainsKey("translation"))
            {
                t = ReadKeyframes(data.Properties["translation"]);
            }

            if (data.Properties.ContainsKey("scale"))
            {
                s = ReadKeyframes(data.Properties["scale"]);
            }

            if (data.Properties.ContainsKey("rotation"))
            {
                r = ReadKeyframes(data.Properties["rotation"]);
            }

            return new EntityBoneAnimation(t, s, r);
        }

        public (float3? trans, float3? scale, float3? rot) Evaluate(float time, MoScope scope, MoLangEnvironment env)
        {
            float3? t = translationKeyframes is null ? null : EvaluateTable(translationKeyframes, time, scope, env);
            float3? s = scaleKeyframes       is null ? null : EvaluateTable(scaleKeyframes,       time, scope, env);
            float3? r = rotationKeyframes    is null ? null : EvaluateTable(rotationKeyframes,    time, scope, env);

            return (t, s, r);
        }

        private static float3? EvaluateTable((float time, MolangVector3 pre, MolangVector3 post)[] keyframes, float time,
                MoScope scope, MoLangEnvironment env)
        {
            // Time is earlier than first keyframe
            if (time < keyframes[0].time)
            {
                //Debug.Log($"Early: [{time}]");
                return keyframes[0].pre.Evaluate(scope, env);
            }

            int prevFrame = -1;
            
            while (prevFrame < keyframes.Length - 1 && keyframes[prevFrame + 1].time < time)
            {
                prevFrame++;
            }

            if (keyframes[prevFrame].time == time) // Sample right on the keyframe
            {
                //Debug.Log($"On: [{time}] (UwU) [{prevFrame}]");
                return keyframes[prevFrame].pre.Evaluate(scope, env);
            }
            else
            {
                if (prevFrame == keyframes.Length - 1) // Time is later than the last keyframe
                {
                    //Debug.Log($"Late: [{time}]");
                    return keyframes[prevFrame].post.Evaluate(scope, env);
                }
                else // Interpolate between 2 keyframes
                {
                    var prev = keyframes[prevFrame];
                    var next = keyframes[prevFrame + 1];

                    // Simple lerp
                    float proc = (time - prev.time) / (next.time - prev.time);

                    var prevVal = prev.post.Evaluate(scope, env);
                    var nextVal = next.pre.Evaluate(scope, env);

                    //Debug.Log($"Interpolate: [{time}] ({proc}) [{prevFrame}] {prevVal} => [{prevFrame + 1}] {nextVal}");
                    return prevVal * (1F - proc) + nextVal * proc;
                }
            }
        }

        private static (float time, MolangVector3 pre, MolangVector3 post)[] ReadKeyframes(Json.JSONData data)
        {
            List<(float time, MolangVector3 pre, MolangVector3 post)> result = new();

            if (data.Type == Json.JSONData.DataType.Array
                    || data.Type == Json.JSONData.DataType.String) // Fixed expression, no keyframes
            {
                // There's only one keyframe at time 0.0
                var frame = MolangVector3.FromJson(data);
                result.Add((0F, frame, frame));
            }
            else if (data.Type == Json.JSONData.DataType.Object) // Specified per-keyframe
            {
                foreach (var pair in data.Properties) // Foreach keyframe
                {
                    float time = float.Parse(pair.Key);
                    var frameData = pair.Value;

                    if (frameData.Type == Json.JSONData.DataType.Array
                            || frameData.Type == Json.JSONData.DataType.String) // A value for the keyframe
                    {
                        var frame = MolangVector3.FromJson(frameData);
                        result.Add((time, frame, frame));
                    }
                    else if (frameData.Type == Json.JSONData.DataType.Object) // Specified pre and post respectively
                    {
                        MolangVector3 pre;
                        if (frameData.Properties.ContainsKey("pre")) // Specified pre value
                        {
                            pre = MolangVector3.FromJson(frameData.Properties["pre"]);
                        }
                        else
                        {
                            if (result.Count > 0) // Take last keyframe's post value
                            {
                                pre = result.Last().post;
                            }
                            else // Not specified, use post value
                            {
                                pre = MolangVector3.FromJson(frameData.Properties["post"]);
                            }
                        }

                        MolangVector3 post;
                        if (frameData.Properties.ContainsKey("post")) // Specified post value
                        {
                            post = MolangVector3.FromJson(frameData.Properties["post"]);
                        }
                        else // Use pre value
                        {
                            post = pre;
                        }

                        result.Add((time, pre, post));
                    }
                }
            }

            return result.ToArray();
        }
    }
}
#nullable enable
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace CraftSharp.Resource
{
    public class EntityModelBone
    {
        public string? ParentName = null;
        public string Name = string.Empty;
        public bool MirrorUV;

        public float3 Pivot;
        public float3 Rotation;

        public EntityModelCube[] Cubes = { };

        public static EntityModelBone FromJson(Json.JSONData data)
        {
            var boneName = data.Properties["name"].StringValue;

            var bonePivot = float3.zero;
            if (data.Properties.ContainsKey("pivot"))
            {
                bonePivot = VectorUtil.Json2SwappedFloat3(data.Properties["pivot"]);
                // Get opposite z
                bonePivot.z = -bonePivot.z;
            }

            string? parentName = null; // null means root bone
            if (data.Properties.ContainsKey("parent"))
            {
                parentName = data.Properties["parent"].StringValue;
            }

            var boneMirrorUV = false;
            if (data.Properties.ContainsKey("mirror"))
            {
                boneMirrorUV = data.Properties["mirror"].StringValue == "true";
            }

            var boneRotation = float3.zero;
            if (data.Properties.ContainsKey("rotation"))
            {
                boneRotation = VectorUtil.Json2SwappedFloat3(data.Properties["rotation"]);
                // Get opposite x
                boneRotation.x = -boneRotation.x;
            }

            // Backwards compatibility for 'bind_pose_rotation' in 1.8 format
            // This got replaced by per-cube pivot and rotation definitions.
            // See https://github.com/JannisX11/blockbench/issues/990
            float3 bindPoseRot = float3.zero;
            bool useBindPoseRot = data.Properties.ContainsKey("bind_pose_rotation");
            if (useBindPoseRot)
            {
                bindPoseRot = VectorUtil.Json2SwappedFloat3(data.Properties["bind_pose_rotation"]);
                // Get opposite x
                bindPoseRot.x = -bindPoseRot.x;
            }

            EntityModelCube[] boneCubes;

            if (!data.Properties.ContainsKey("cubes"))
            {
                boneCubes = new EntityModelCube[] { };
            }
            else
            {
                boneCubes = data.Properties["cubes"].DataArray.Select(cubeData =>
                {
                    var origin = VectorUtil.Json2SwappedFloat3(cubeData.Properties["origin"]);
                    var size = VectorUtil.Json2SwappedFloat3(cubeData.Properties["size"]);
                    // Get opposite z
                    origin.z = -origin.z - size.z;

                    var uv = float2.zero;
                    Dictionary<FaceDir, float4>? perFaceUV = null;
                    if (cubeData.Properties.ContainsKey("uv"))
                    {
                        if (cubeData.Properties["uv"].Type == Json.JSONData.DataType.Array) // Use whole box uv mapping
                        {
                            uv = VectorUtil.Json2Float2(cubeData.Properties["uv"]);
                        }
                        else // Use per-face uv mapping
                        {
                            perFaceUV = new();
                            foreach (var pair in cubeData.Properties["uv"].Properties)
                            {
                                var faceDir = Directions.FaceDirFromName(pair.Key);
                                var faceData = pair.Value.Properties;

                                float2 faceUV = float2.zero, faceUVSize = float2.zero;
                                if (faceData.ContainsKey("uv"))
                                    faceUV = VectorUtil.Json2Float2(faceData["uv"]);
                                if (faceData.ContainsKey("uv_size"))
                                    faceUVSize = VectorUtil.Json2Float2(faceData["uv_size"]);
                                
                                perFaceUV.Add(faceDir, new float4(faceUV, faceUVSize));
                            }
                        }
                    }

                    bool? cubeMirrorUV = null;
                    if (cubeData.Properties.ContainsKey("mirror"))
                    {
                        cubeMirrorUV = cubeData.Properties["mirror"].StringValue == "true";
                    }

                    var rotation = float3.zero;
                    var pivot = float3.zero;

                    if (useBindPoseRot) // Legacy 1.8 format, where cube pivot and rotation is not yet supported, but 'bind_pose_rotation' is available instead
                    {
                        rotation = bindPoseRot;
                        pivot = bonePivot;
                    }
                    else // Probably new format, see if cube rotation and pivot is specified
                    {
                        if (cubeData.Properties.ContainsKey("rotation"))
                        {
                            rotation = VectorUtil.Json2SwappedFloat3(cubeData.Properties["rotation"]);
                            // Get opposite x
                            rotation.x = -rotation.x;
                        }

                        if (cubeData.Properties.ContainsKey("pivot"))
                        {
                            pivot = VectorUtil.Json2SwappedFloat3(cubeData.Properties["pivot"]);
                            // Get opposite z
                            pivot.z = -pivot.z;
                        }
                        else //  Not specified
                        {
                            // In this case, the geometry center of the cube is used as the pivot (took me a really long time to figure out)
                            pivot = origin + size / 2F;
                        }
                    }

                    var inflate = 0F;
                    if (cubeData.Properties.ContainsKey("inflate"))
                    {
                        inflate = float.Parse(cubeData.Properties["inflate"].StringValue);
                    }

                    return new EntityModelCube
                    {
                        Origin = origin,
                        Size = size,
                        MirrorUV = cubeMirrorUV,
                        UV = uv,
                        PerFaceUV = perFaceUV,
                        Inflate = inflate,
                        Pivot = pivot,
                        Rotation = rotation
                    };
                } ).ToArray();
            }

            return new EntityModelBone
            {
                // Bone names are NOT case sensitive, so we convert all bone names
                // to lower case to make sure they'll be referenced correctly
                ParentName = parentName?.ToLower(),
                Name = boneName.ToLower(),
                MirrorUV = boneMirrorUV,
                Pivot = bonePivot,
                Rotation = boneRotation,

                Cubes = boneCubes
            };
        }
    }
}
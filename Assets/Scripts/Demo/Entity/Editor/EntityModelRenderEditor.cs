#nullable enable
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;

using CraftSharp.Resource;
using CraftSharp.Molang.Utils;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Demo
{
    [CustomEditor(typeof (EntityModelRender))]
    public class EntityModelRenderEditor : Editor
    {
        private int selectedAnimation = -1;
        private EntityAnimation? animation = null;

        private Dictionary<MoPath, float> variableTable = new();
        private string animationDescription = string.Empty;
        private float animationTime = 0F;

        private readonly static GUILayoutOption[] ANIMATION_DESCRIPTION_LAYOUT_OPTIONS = { GUILayout.Height(270F) };

        public override void OnInspectorGUI()
        {
            var render = (EntityModelRender) target;

            var sel = EditorGUILayout.Popup("Animation", selectedAnimation, render.animationNames);

            if (sel != selectedAnimation) // Selection changed
            {
                selectedAnimation = sel;
                animation = render.SetAnimation(sel, animationTime);

                if (animation is not null)
                {
                    StringBuilder animationDesc;

                    animationDesc = new($"===========\n* Loop: {animation.Loop}\n* Length: {animation.Length}\n===========\n");
                    animationDesc.AppendLine("* Bones:");

                    HashSet<MoPath> allVariables = new();

                    foreach (var pair in animation.BoneAnimations)
                    {
                        allVariables.AddRange(pair.Value.Variables);

                        var variables = string.Join(", ", pair.Value.Variables);
                        animationDesc.AppendLine($"   - {pair.Key} ({variables})");
                    }

                    // Update variable values
                    var newTable = new Dictionary<MoPath, float>();
                    foreach (var varName in allVariables)
                    {
                        // Register in inspector
                        newTable.Add(varName, variableTable.ContainsKey(varName) ? variableTable[varName] : 0F);
                        // Register in entity render's Molang environment
                        render.UpdateMolangValue(varName, MoValue.FromObject(0F));
                    }
                    variableTable = newTable;

                    animationDescription = animationDesc.ToString();
                }
                else
                {
                    animationDescription = "* Selected animation is empty";
                }
            }

            if (selectedAnimation != -1)
            {
                EditorGUILayout.TextArea(
                        $"Selected animation: [{selectedAnimation}] {render.animationNames[sel]}\n\n{animationDescription}",
                        ANIMATION_DESCRIPTION_LAYOUT_OPTIONS);
                
                bool variableUpdated = false;

                var newAnimTime = EditorGUILayout.FloatField("Animation time: ", animationTime);
                if (newAnimTime != animationTime)
                {
                    animationTime = newAnimTime;
                    variableUpdated = true;
                }

                EditorGUILayout.LabelField("Variables:");
                foreach (var varName in variableTable.Keys.ToArray())
                {
                    //var newValue = EditorGUILayout.FloatField($"[{varName}]", variableTable[varName]);
                    var newValue = EditorGUILayout.Slider($"[{varName}]", variableTable[varName], -100F, 100F);

                    if (newValue != variableTable[varName])
                    {
                        variableTable[varName] = newValue;
                        render.UpdateMolangValue(varName, MoValue.FromObject(newValue));

                        variableUpdated = true;
                    }
                }

                if (variableUpdated)
                {
                    render.UpdateAnimation(animationTime);
                }
            }
        }
    }
}
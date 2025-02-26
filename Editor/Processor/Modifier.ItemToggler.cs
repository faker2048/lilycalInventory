using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class Processor
    {
        private partial class Modifier
        {
            internal static void ApplyItemToggler(AnimatorController controller, bool hasWriteDefaultsState, ItemToggler[] togglers, BlendTree root, List<InternalParameter> parameters)
            {
                foreach(var toggler in togglers)
                {
                    if(toggler.parameter.objects.Length + toggler.parameter.blendShapeModifiers.Length + toggler.parameter.materialReplacers.Length + toggler.parameter.materialPropertyModifiers.Length + toggler.parameter.clips.Length + toggler.parameter.vrcParameterSetters.Length > 0)
                    {
                        // コンポーネントの設定値とprefab初期値を取得したAnimationClipを作成
                        var clips = toggler.parameter.CreateClip(ctx.AvatarRootObject, toggler.menuName);
                        var (clipDefault, clipChanged) = (clips.clipDefault.ToClip(), clips.clipChanged.ToClip());
                        AssetDatabase.AddObjectToAsset(clipDefault, ctx.AssetContainer);
                        AssetDatabase.AddObjectToAsset(clipChanged, ctx.AssetContainer);

                        // AnimatorControllerに追加
                        if(root) AnimationHelper.AddItemTogglerTree(controller, clipDefault, clipChanged, toggler.menuName, toggler.parameterName, toggler.defaultValue, root);
                        else AnimationHelper.AddItemTogglerLayer(controller, hasWriteDefaultsState, clipDefault, clipChanged, toggler.menuName, toggler.parameterName, toggler.defaultValue);

                        #if LIL_VRCSDK3A
                        // 为 ItemToggler 的 State 添加 VRCParameterDriver 组件，以处理 vrcParameterSetters
                        if (toggler.parameter.vrcParameterSetters.Length > 0)
                        {
                            foreach (var layer in controller.layers)
                            {
                                if (layer.name == toggler.menuName)
                                {
                                    // 查找对应的 State（Changed 状态，即激活状态）
                                    foreach (var state in layer.stateMachine.states)
                                    {
                                        if (state.state.name == $"{toggler.menuName}_Changed")
                                        {
                                            // 使用辅助方法添加 VRCParameterDriver
                                            AnimationHelper.AddVRCParameterDriverToState(state.state, toggler.parameter.vrcParameterSetters);
                                            
                                            // 为其他状态添加退出时的参数驱动器
                                            AnimationHelper.AddExitVRCParameterDriverToOtherStates(layer.stateMachine, state.state, toggler.parameter.vrcParameterSetters);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        #endif
                    }
                    else
                    {
                        controller.TryAddParameter(toggler.parameterName, toggler.defaultValue);
                    }

                    parameters.Add(new InternalParameter(toggler.parameterName, toggler.defaultValue ? 1 : 0, toggler.isLocalOnly, toggler.isSave, InternalParameterType.Bool));
                }
            }
        }
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if LIL_VRCSDK3A
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class Processor
    {
        private partial class Modifier
        {
            internal static void ApplySmoothChanger(AnimatorController controller, bool hasWriteDefaultsState, SmoothChanger[] changers, BlendTree root, List<InternalParameter> parameters)
            {
                foreach(var changer in changers)
                {
                    if(changer.frames.Length != 0)
                    {
                        var clipDefaults = new InternalClip[changer.frames.Length];
                        var clipChangeds = new InternalClip[changer.frames.Length];
                        var frames = new float[changer.frames.Length];

                        // 各フレームの設定値とprefab初期値を取得したAnimationClipを作成
                        for(int i = 0; i < changer.frames.Length; i++)
                        {
                            var frame = changer.frames[i];
                            var frameValue = Mathf.Clamp01(frame.frameValue);
                            var clip2 = frame.parametersPerMenu.CreateClip(ctx.AvatarRootObject, $"{changer.menuName}_{i}");
                            clipDefaults[i] = clip2.Item1;
                            clipChangeds[i] = clip2.Item2;
                            frames[i] = frameValue;
                        }

                        // prefab初期値AnimationClipをマージ
                        var clipDefault = InternalClip.MergeAndCreate(clipDefaults);

                        // 各フレームの未設定値をprefab初期値で埋める
                        var clips = new AnimationClip[clipChangeds.Length];
                        for(int i = 0; i < clipChangeds.Length; i++)
                        {
                            clipChangeds[i] = InternalClip.MergeAndCreate(clipChangeds[i], clipDefault);
                            clipChangeds[i].name = $"{changer.menuName}_{i}_Merged";
                            clips[i] = clipChangeds[i].ToClip();
                            AssetDatabase.AddObjectToAsset(clips[i], ctx.AssetContainer);
                        }

                        // AnimatorControllerに追加
                        if(root) AnimationHelper.AddSmoothChangerTree(controller, clips, frames, changer.menuName, changer.parameterName, changer.defaultFrameValue, root);
                        else AnimationHelper.AddSmoothChangerLayer(controller, hasWriteDefaultsState, clips, frames, changer.menuName, changer.parameterName, changer.defaultFrameValue);

                        #if LIL_VRCSDK3A
                        // 检查是否有任何帧包含 VRCParameterSetter
                        bool hasVRCParameterSetters = false;
                        for (int i = 0; i < changer.frames.Length; i++)
                        {
                            if (changer.frames[i].parametersPerMenu.vrcParameterSetters.Length > 0)
                            {
                                hasVRCParameterSetters = true;
                                break;
                            }
                        }

                        // 如果有 VRCParameterSetter，我们需要为每个帧创建一个单独的层
                        if (hasVRCParameterSetters)
                        {
                            // 创建一个空的 AnimationClip 用于 VRCParameterDriver 状态
                            var emptyClip = new AnimationClip() { name = "Empty" };
                            AssetDatabase.AddObjectToAsset(emptyClip, ctx.AssetContainer);

                            for (int i = 0; i < changer.frames.Length; i++)
                            {
                                var frame = changer.frames[i];
                                if (frame.parametersPerMenu.vrcParameterSetters.Length == 0) continue;

                                // 创建一个新的状态机
                                var stateMachine = new AnimatorStateMachine();
                                
                                // 创建一个状态，当参数值接近当前帧值时激活
                                var state = new AnimatorState
                                {
                                    motion = emptyClip,
                                    name = $"{changer.menuName}_{i}_VRCParams",
                                    writeDefaultValues = hasWriteDefaultsState
                                };
                                stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(200, 0, 0));
                                stateMachine.defaultState = state;

                                // 添加条件：当参数值接近当前帧值时激活
                                float frameValue = Mathf.Clamp01(frame.frameValue);
                                float threshold = 0.05f; // 阈值，可以根据需要调整
                                
                                // 如果是第一帧，条件是 <= frameValue + threshold
                                if (i == 0)
                                {
                                    var entryTransition = stateMachine.AddEntryTransition(state);
                                    entryTransition.AddCondition(AnimatorConditionMode.Less, frameValue + threshold, changer.parameterName);
                                }
                                // 如果是最后一帧，条件是 >= frameValue - threshold
                                else if (i == changer.frames.Length - 1)
                                {
                                    var entryTransition = stateMachine.AddEntryTransition(state);
                                    entryTransition.AddCondition(AnimatorConditionMode.Greater, frameValue - threshold, changer.parameterName);
                                }
                                // 中间帧，条件是在范围内
                                else
                                {
                                    var entryTransition = stateMachine.AddEntryTransition(state);
                                    entryTransition.AddCondition(AnimatorConditionMode.Greater, frameValue - threshold, changer.parameterName);
                                    entryTransition.AddCondition(AnimatorConditionMode.Less, frameValue + threshold, changer.parameterName);
                                }

                                // 使用辅助方法添加 VRCParameterDriver
                                AnimationHelper.AddVRCParameterDriverToState(state, frame.parametersPerMenu.vrcParameterSetters);
                                
                                // 为其他状态添加退出时的参数驱动器
                                AnimationHelper.AddExitVRCParameterDriverToOtherStates(stateMachine, state, frame.parametersPerMenu.vrcParameterSetters);

                                // 添加层
                                var layer = new AnimatorControllerLayer
                                {
                                    blendingMode = AnimatorLayerBlendingMode.Override,
                                    defaultWeight = 1,
                                    name = $"{changer.menuName}_{i}_VRCParams",
                                    stateMachine = stateMachine
                                };
                                controller.AddLayer(layer);
                            }
                        }
                        #endif
                    }
                    else
                    {
                        controller.TryAddParameter(changer.parameterName, changer.defaultFrameValue);
                    }

                    parameters.Add(new InternalParameter(changer.parameterName, changer.defaultFrameValue, changer.isLocalOnly, changer.isSave, InternalParameterType.Float));
                }
            }
        }
    }
}

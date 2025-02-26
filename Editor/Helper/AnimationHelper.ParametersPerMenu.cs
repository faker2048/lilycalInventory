using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class AnimationHelper
    {
        internal static (InternalClip clipDefault, InternalClip clipChanged) CreateClip(this ParametersPerMenu parameter, GameObject gameObject, string name)
        {
            var clipDefault = new InternalClip();
            var clipChanged = new InternalClip();
            clipDefault.name = $"{name}_Default";
            clipChanged.name = $"{name}_Changed";

            foreach(var toggler in parameter.objects)
            {
                if(!toggler.obj) continue;
                toggler.ToClipDefault(clipDefault);
                toggler.ToClip(clipChanged);
            }

            foreach(var modifier in parameter.blendShapeModifiers)
            {
                if(modifier.applyToAll)
                {
                    var renderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach(var renderer in renderers)
                    {
                        if(!renderer || !renderer.sharedMesh) continue;
                        foreach(var namevalue in modifier.blendShapeNameValues)
                        {
                            if(renderer.sharedMesh.GetBlendShapeIndex(namevalue.name) == -1) continue;
                            namevalue.ToClipDefault(clipDefault, renderer);
                            namevalue.ToClip(clipChanged, renderer);
                        }
                    }
                    continue;
                }
                if(!modifier.skinnedMeshRenderer) continue;
                foreach(var namevalue in modifier.blendShapeNameValues)
                {
                    namevalue.ToClipDefault(clipDefault, modifier.skinnedMeshRenderer);
                    namevalue.ToClip(clipChanged, modifier.skinnedMeshRenderer);
                }
            }

            foreach(var replacer in parameter.materialReplacers)
            {
                if(!replacer.renderer) continue;
                replacer.ToClipDefault(clipDefault);
                replacer.ToClip(clipChanged);
            }

            foreach(var modifier in parameter.materialPropertyModifiers)
            {
                if(modifier.renderers.Length == 0)
                    modifier.renderers = gameObject.GetComponentsInChildren<Renderer>(true).Where(r => r.sharedMaterials != null && r.sharedMaterials.Length > 0 && r.sharedMaterials.Any(m => m)).ToArray();

                modifier.ToClipDefault(clipDefault);
                modifier.ToClip(clipChanged, clipDefault);
            }

            foreach(var clip in parameter.clips)
            {
                if(!clip) continue;
                clipDefault.AddDefault(clip, gameObject);
                clipChanged.Add(clip);
            }

            #if LIL_VRCSDK3A
            // 处理 VRCParameterSetter
            foreach(var setter in parameter.vrcParameterSetters)
            {
                if(string.IsNullOrEmpty(setter.parameterName)) continue;
                
                // 我们不需要在 clipDefault 中添加任何内容，因为 VRCParameterSetter 是通过 VRCAvatarParameterDriver 实现的
                // 而不是通过动画曲线
            }
            #endif
            
            return (clipDefault, clipChanged);
        }

        #if LIL_VRCSDK3A
        /// <summary>
        /// 为状态添加 VRCAvatarParameterDriver 组件，处理 VRCParameterSetter
        /// </summary>
        /// <param name="state">要添加驱动器的状态</param>
        /// <param name="setters">参数设置器数组</param>
        internal static void AddVRCParameterDriverToState(AnimatorState state, VRCParameterSetter[] setters)
        {
            if(setters == null || setters.Length == 0) return;
            
            // 过滤出进入状态时执行的参数设置器
            var enterSetters = setters.Where(s => !string.IsNullOrEmpty(s.parameterName)).ToArray();
            if(enterSetters.Length > 0)
            {
                // 查找现有的VRCAvatarParameterDriver组件，如果没有则创建一个新的
                var driver = state.behaviours.OfType<VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver>().FirstOrDefault();
                if(driver == null)
                {
                    driver = state.AddStateMachineBehaviour<VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver>();
                    driver.localOnly = false; // 全局参数
                }

                // 添加参数设置
                foreach(var setter in enterSetters)
                {
                    var parameter = new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter();
                    parameter.name = setter.parameterName;

                    // 根据参数类型设置值和操作类型
                    switch(setter.operationType)
                    {
                        case VRCParameterOperationType.Set:
                            parameter.type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set;
                            
                            // 根据参数类型设置值
                            switch(setter.parameterType)
                            {
                                case VRCParameterType.Float:
                                    parameter.value = setter.floatValue;
                                    break;
                                case VRCParameterType.Int:
                                    parameter.value = setter.intValue;
                                    break;
                                case VRCParameterType.Bool:
                                    parameter.value = setter.boolValue ? 1 : 0;
                                    break;
                            }
                            break;
                            
                        case VRCParameterOperationType.Copy:
                            parameter.type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Copy;
                            parameter.source = setter.sourceParameterName;
                            break;
                    }

                    driver.parameters.Add(parameter);
                }
            }
        }

        /// <summary>
        /// 为状态机中除指定状态外的所有状态添加退出时的参数驱动器
        /// </summary>
        /// <param name="stateMachine">状态机</param>
        /// <param name="excludeState">要排除的状态（当前状态）</param>
        /// <param name="setters">参数设置器数组</param>
        internal static void AddExitVRCParameterDriverToOtherStates(AnimatorStateMachine stateMachine, AnimatorState excludeState, VRCParameterSetter[] setters)
        {
            if(setters == null || setters.Length == 0) return;
            
            // 过滤出退出状态时执行的参数设置器
            var exitSetters = setters.Where(s => s.executeOnExit && !string.IsNullOrEmpty(s.parameterName)).ToArray();
            if(exitSetters.Length == 0) return;
            
            // 为状态机中除了excludeState外的所有状态添加退出时的参数驱动器
            foreach(var stateInfo in stateMachine.states)
            {
                if(stateInfo.state == excludeState) continue;
                
                // 查找现有的VRCAvatarParameterDriver组件，如果没有则创建一个新的
                var driver = stateInfo.state.behaviours.OfType<VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver>().FirstOrDefault();
                if(driver == null)
                {
                    driver = stateInfo.state.AddStateMachineBehaviour<VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver>();
                    driver.localOnly = false; // 全局参数
                }
                
                // 添加参数设置
                foreach(var setter in exitSetters)
                {
                    var parameter = new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter();
                    parameter.name = setter.parameterName;
                    
                    // 根据参数类型设置值和操作类型
                    switch(setter.exitOperationType)
                    {
                        case VRCParameterOperationType.Set:
                            parameter.type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set;
                            
                            // 根据参数类型设置值
                            switch(setter.parameterType)
                            {
                                case VRCParameterType.Float:
                                    parameter.value = setter.exitFloatValue;
                                    break;
                                case VRCParameterType.Int:
                                    parameter.value = setter.exitIntValue;
                                    break;
                                case VRCParameterType.Bool:
                                    parameter.value = setter.exitBoolValue ? 1 : 0;
                                    break;
                            }
                            break;
                            
                        case VRCParameterOperationType.Copy:
                            parameter.type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Copy;
                            parameter.source = setter.exitSourceParameterName;
                            break;
                    }
                    
                    driver.parameters.Add(parameter);
                }
            }
        }
        #endif

        // ObjectToggler
        internal static void ToClipDefault(this ObjectToggler toggler, InternalClip clip)
        {
            var binding = CreateToggleBinding(toggler.obj);
            clip.Add(binding, !toggler.value);
            toggler.obj.SetActive(!toggler.value);
        }

        internal static void ToClip(this ObjectToggler toggler, InternalClip clip)
        {
            var binding = CreateToggleBinding(toggler.obj);
            clip.Add(binding, toggler.value);
        }

        // BlendShapeModifier
        internal static void ToClipDefault(this BlendShapeNameValue namevalue, InternalClip clip, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var binding = CreateBlendShapeBinding(skinnedMeshRenderer, namevalue.name);
            var value = skinnedMeshRenderer.GetBlendShapeWeight(skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(namevalue.name));
            clip.Add(binding, value);
        }

        internal static void ToClip(this BlendShapeNameValue namevalue, InternalClip clip, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var binding = CreateBlendShapeBinding(skinnedMeshRenderer, namevalue.name);
            clip.Add(binding, namevalue.value);
        }

        // MaterialReplacer
        private static void ToClipDefault(this MaterialReplacer replacer, InternalClip clip)
        {
            for(int i = 0; i < replacer.replaceTo.Length; i++)
            {
                if(!replacer.replaceTo[i]) continue;
                var binding = CreateMaterialReplaceBinding(replacer.renderer, i);
                clip.Add(binding, replacer.renderer.sharedMaterials[i]);
            }
        }

        private static void ToClip(this MaterialReplacer replacer, InternalClip clip)
        {
            for(int i = 0; i < replacer.replaceTo.Length; i++)
            {
                if(!replacer.replaceTo[i]) continue;
                var binding = CreateMaterialReplaceBinding(replacer.renderer, i);
                clip.Add(binding, replacer.replaceTo[i]);
            }
        }

        // MaterialPropertyModifier
        private static void ToClipDefault(this MaterialPropertyModifier modifier, InternalClip clip)
        {
            foreach(var renderer in modifier.renderers)
            {
                if(!renderer) continue;
                foreach(var floatModifier in modifier.floatModifiers)
                {
                    var binding = CreateMaterialPropertyBinding(renderer, floatModifier.propertyName);
                    float value = 0;
                    foreach(var material in renderer.sharedMaterials)
                    {
                        if(Processor.TryGetFloat(material, floatModifier.propertyName, out value)) break;
                    }
                    clip.Add(binding, value);
                }
                foreach(var vectorModifier in modifier.vectorModifiers)
                {
                    var bindingX = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.x");
                    var bindingY = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.y");
                    var bindingZ = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.z");
                    var bindingW = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.w");
                    Vector4 value = Vector4.zero;
                    foreach(var material in renderer.sharedMaterials)
                    {
                        if(Processor.TryGetVector(material, vectorModifier.propertyName, out value)) break;
                    }
                    clip.Add(bindingX, value.x);
                    clip.Add(bindingY, value.y);
                    clip.Add(bindingZ, value.z);
                    clip.Add(bindingW, value.w);
                }
            }
        }

        private static void ToClip(this MaterialPropertyModifier modifier, InternalClip clip, InternalClip clipDefault)
        {
            foreach(var renderer in modifier.renderers)
            {
                if(!renderer) continue;
                foreach(var floatModifier in modifier.floatModifiers)
                {
                    var binding = CreateMaterialPropertyBinding(renderer, floatModifier.propertyName);
                    clip.Add(binding, floatModifier.value);
                }
                foreach(var vectorModifier in modifier.vectorModifiers)
                {
                    var bindingX = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.x");
                    var bindingY = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.y");
                    var bindingZ = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.z");
                    var bindingW = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.w");
                    clip.Add(bindingX, !vectorModifier.disableX ? vectorModifier.value.x : clipDefault.bindings[bindingX].Item1);
                    clip.Add(bindingY, !vectorModifier.disableY ? vectorModifier.value.y : clipDefault.bindings[bindingY].Item1);
                    clip.Add(bindingZ, !vectorModifier.disableZ ? vectorModifier.value.z : clipDefault.bindings[bindingZ].Item1);
                    clip.Add(bindingW, !vectorModifier.disableW ? vectorModifier.value.w : clipDefault.bindings[bindingW].Item1);
                }
            }
        }

        internal static ParametersPerMenu CreateDefaultParameters(this ParametersPerMenu[] parameters)
        {
            var parameter = new ParametersPerMenu();
            parameter.objects = parameters.SelectMany(p => p.objects).Select(o => o.obj).Distinct().Select(o => new ObjectToggler{obj = o, value = false}).ToArray();

            var blendShapeModifiers = parameters.SelectMany(p => p.blendShapeModifiers).Where(b => b.skinnedMeshRenderer && b.skinnedMeshRenderer.sharedMesh).Select(b => new BlendShapeModifier{skinnedMeshRenderer = b.skinnedMeshRenderer, blendShapeNameValues = b.blendShapeNameValues});
            foreach(var b in blendShapeModifiers)
            {
                b.blendShapeNameValues.Select(v => {
                    var index = b.skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(v.name);
                    if(index != -1) v.value = b.skinnedMeshRenderer.GetBlendShapeWeight(index);
                    return v;
                });
            }
            parameter.blendShapeModifiers = blendShapeModifiers.ToArray();

            parameter.materialReplacers = parameters.SelectMany(p => p.materialReplacers).Where(m => m.renderer).Select(m => new MaterialReplacer{renderer = m.renderer, replaceTo = m.renderer.sharedMaterials}).ToArray();
            var materialPropertyModifiers = (MaterialPropertyModifier[])parameters.SelectMany(p => p.materialPropertyModifiers).Select(m => {
                var mod = new MaterialPropertyModifier();
                mod.renderers = m.renderers;
                mod.floatModifiers = (FloatModifier[])m.floatModifiers.Clone();
                mod.vectorModifiers = (VectorModifier[])m.vectorModifiers.Clone();
                return mod;
            }).ToArray().Clone();
            foreach(var modifier in materialPropertyModifiers)
            foreach(var renderer in modifier.renderers)
            {
                if(!renderer) continue;
                for(int i = 0; i < modifier.floatModifiers.Length; i++)
                {
                    var floatModifier = modifier.floatModifiers[i];
                    float value = 0;
                    foreach(var material in renderer.sharedMaterials)
                    {
                        if(!material || !material.HasProperty(floatModifier.propertyName)) continue;
                        value = material.GetFloat(floatModifier.propertyName);
                        break;
                    }
                    modifier.floatModifiers[i].value = value;
                }
                for(int i = 0; i < modifier.vectorModifiers.Length; i++)
                {
                    var vectorModifier = modifier.vectorModifiers[i];
                    Vector4 value = Vector4.zero;
                    foreach(var material in renderer.sharedMaterials)
                    {
                        if(!material || !material.HasProperty(vectorModifier.propertyName)) continue;
                        value = material.GetVector(vectorModifier.propertyName);
                        break;
                    }
                    modifier.vectorModifiers[i].value = value;
                }
            }
            parameter.materialPropertyModifiers = materialPropertyModifiers;

            return parameter;
        }

        internal static ParametersPerMenu Merge(this ParametersPerMenu parameter1, ParametersPerMenu parameter2)
        {
            var parameter = new ParametersPerMenu();
            var objs = parameter1.objects.Select(o => o.obj);
            parameter.objects = parameter1.objects.Union(parameter2.objects.Where(t => !objs.Contains(t.obj))).ToArray();
            var smrs = parameter1.blendShapeModifiers.Select(m => m.skinnedMeshRenderer);
            parameter.blendShapeModifiers = parameter1.blendShapeModifiers.Union(parameter2.blendShapeModifiers.Where(m => !smrs.Contains(m.skinnedMeshRenderer))).ToArray();
            var rs = parameter1.materialReplacers.Select(m => m.renderer);
            parameter.materialReplacers = parameter1.materialReplacers.Union(parameter2.materialReplacers.Where(m => !rs.Contains(m.renderer))).ToArray();
            parameter.materialPropertyModifiers = (MaterialPropertyModifier[])parameter1.materialPropertyModifiers.Clone();
            return parameter;
        }

        internal static void GatherConditions(
            this ItemToggler[] itemTogglers,
            Dictionary<GameObject, HashSet<(string name, bool toActive, bool defaultValue)>> toggleBools,
            Dictionary<(SkinnedMeshRenderer, string), HashSet<(string name, bool toActive, bool defaultValue)>> shapeBools
        )
        {
            foreach(var itemToggler in itemTogglers)
            {
                foreach(var toggler in itemToggler.parameter.objects)
                    toggleBools.GetOrAdd(toggler.obj).Add((itemToggler.parameterName, toggler.value, itemToggler.defaultValue));

                foreach(var bsModifier in itemToggler.parameter.blendShapeModifiers)
                    foreach(var nv in bsModifier.blendShapeNameValues)
                        shapeBools.GetOrAdd((bsModifier.skinnedMeshRenderer, nv.name)).Add((itemToggler.parameterName, nv.value == 100f, itemToggler.defaultValue));
            }
        }

        internal static void GatherConditions(
            this CostumeChanger[] costumeChangers,
            Dictionary<GameObject, HashSet<(string name, bool[] toActives, int defaultValue)>> toggleInts,
            Dictionary<(SkinnedMeshRenderer, string), HashSet<(string name, bool[] toActives, int defaultValue)>> shapeInts
        )
        {
            bool Cond(CostumeChanger costumeChanger, GameObject obj, Costume c)
            {
                var first = c.parametersPerMenu.objects.FirstOrDefault(x => x.obj == obj);
                if(first != null) return first.value;
                if(System.Array.IndexOf(costumeChanger.costumes, c) == costumeChanger.defaultValue) return obj.activeSelf;
                var def = costumeChanger.costumes[costumeChanger.defaultValue].parametersPerMenu.objects.FirstOrDefault(x => x.obj == obj);
                if(def != null) return !def.value;
                return obj.activeSelf;
            }

            bool CondShape(CostumeChanger costumeChanger, SkinnedMeshRenderer obj, string name, Costume c)
            {
                var first = c.parametersPerMenu.blendShapeModifiers
                    .FirstOrDefault(x => x.skinnedMeshRenderer == obj && x.blendShapeNameValues.Any(nv => nv.name == name));
                if(first != null) return first.blendShapeNameValues.First(nv => nv.name == name).value == 100f;

                if(System.Array.IndexOf(costumeChanger.costumes, c) == costumeChanger.defaultValue) return obj.GetBlendShapeWeight(name) == 100f;

                var def = costumeChanger.costumes[costumeChanger.defaultValue].parametersPerMenu.blendShapeModifiers.FirstOrDefault(x => x.skinnedMeshRenderer == obj && x.blendShapeNameValues.Any(nv => nv.name == name));
                if(def != null) return def.blendShapeNameValues.First(nv => nv.name == name).value == 100f;
                return obj.GetBlendShapeWeight(name) == 100f;
            }

            foreach(var costumeChanger in costumeChangers)
            {
                foreach(var obj in costumeChanger.costumes.SelectMany(c => c.parametersPerMenu.objects).Select(o => o.obj).Where(o => o).Distinct())
                    toggleInts.GetOrAdd(obj).Add((costumeChanger.parameterName, costumeChanger.costumes.Select(c => Cond(costumeChanger, obj, c)).ToArray(), costumeChanger.defaultValue));

                foreach(var kv in costumeChanger.costumes.SelectMany(c => c.parametersPerMenu.blendShapeModifiers).SelectMany(o => o.blendShapeNameValues.Select(nv => (o.skinnedMeshRenderer,nv.name))).Distinct())
                    shapeInts.GetOrAdd(kv).Add((costumeChanger.parameterName, costumeChanger.costumes.Select(c => CondShape(costumeChanger, kv.skinnedMeshRenderer, kv.name, c)).ToArray(), costumeChanger.defaultValue));
            }
        }
    }
}

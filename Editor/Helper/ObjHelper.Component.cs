using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class ObjHelper
    {
        internal static string GetMenuName(this MenuBaseComponent component)
        {
            var name = component.menuName;
            if(string.IsNullOrEmpty(name)) name = component.gameObject.name;
            return name;
        }

        // 衣装のメニュー名を取得
        private static string GetMenuName(this Costume costume, CostumeChanger changer)
        {
            if(!string.IsNullOrEmpty(costume.menuName)) return costume.menuName;
            var obj = costume.parametersPerMenu.objects.Select(o => o.obj).FirstOrDefault(o => o && !string.IsNullOrEmpty(o.name));
            if(obj && !string.IsNullOrEmpty(obj.name)) return obj.name;
            var shape = costume.parametersPerMenu.blendShapeModifiers.SelectMany(m => m.blendShapeNameValues).Select(v => v.name).FirstOrDefault(n => !string.IsNullOrEmpty(n));
            if(!string.IsNullOrEmpty(shape)) return shape;
            var material = costume.parametersPerMenu.materialReplacers.SelectMany(r => r.replaceTo).FirstOrDefault(m => m && !string.IsNullOrEmpty(m.name));
            if(material && !string.IsNullOrEmpty(material.name)) return material.name;
            var nameF = costume.parametersPerMenu.materialPropertyModifiers.SelectMany(m => m.floatModifiers).Select(m => m.propertyName).FirstOrDefault(n => !string.IsNullOrEmpty(n));
            if(!string.IsNullOrEmpty(nameF)) return nameF;
            var nameV = costume.parametersPerMenu.materialPropertyModifiers.SelectMany(m => m.vectorModifiers).Select(m => m.propertyName).FirstOrDefault(n => !string.IsNullOrEmpty(n));
            if(!string.IsNullOrEmpty(nameV)) return nameF;
            return null;
        }

        // メニューの親がオフでないかチェック
        internal static MenuFolder UnenabledParent(this MenuBaseComponent component)
        {
            if(component.parentOverrideMA) return null;
            var parent = component.GetMenuParent();
            if(!parent) return null;
            if(!parent.enabled) return parent;
            return UnenabledParent(parent);
        }

        // メニュー名を解決しつう重複がないかをチェック
        internal static void ResolveMenuName(this MenuBaseComponent[] components)
        {
            foreach(var component in components)
                component.menuName = component.GetMenuName();
            var duplicates = components.Where(c => !(c is MenuFolder)).GroupBy(c => c.menuName).Where(g => g.Count() > 1).SelectMany(g => g).ToArray();
            if(duplicates.Length > 0) ErrorHelper.Report("dialog.error.menuNameDuplication", duplicates);
        }

        internal static void ResolveMenuName(this CostumeChanger[] changers)
        {
            foreach(var changer in changers)
                foreach(var costume in changer.costumes)
                    costume.menuName = costume.GetMenuName(changer);

            var objs = changers.Where(c => c.costumes.Any(d => string.IsNullOrEmpty(d.menuName)));
            if(objs.Count() > 0) ErrorHelper.Report("dialog.error.menuNameEmpty", objs.ToArray());
        }

        // 親フォルダを検索
        internal static MenuFolder GetMenuParent(this MenuBaseComponent component)
        {
            if(component.parentOverride) return component.parentOverride;
            return component.gameObject.GetComponentInParentInAvatar<MenuFolder>();
        }

        /// <summary>
        /// 递归向上查找第一个MenuFolder
        /// </summary>
        internal static MenuFolder GetMenuParentRecursiveFirst(this MenuBaseComponent component)
        {
            if(component.parentOverride) return component.parentOverride;
            return component.gameObject.GetComponentInParentRecursiveFirst<MenuFolder>(f => true);
        }

        // メッシュが空っぽのときに全メッシュをアニメーション対象にするようにする
        internal static void CheckApplyToAll(ItemToggler[] togglers, CostumeChanger[] costumeChangers, SmoothChanger[] smoothChangers)
        {
            foreach(var toggler in togglers) toggler.parameter.CheckApplyToAll();
            foreach(var changer in costumeChangers)
                foreach(var costume in changer.costumes) costume.parametersPerMenu.CheckApplyToAll();
            foreach(var changer in smoothChangers)
                foreach(var frame in changer.frames) frame.parametersPerMenu.CheckApplyToAll();
        }

        private static void CheckApplyToAll(this ParametersPerMenu parameters)
        {
            foreach(var blendShapeModifier in parameters.blendShapeModifiers) 
                blendShapeModifier.applyToAll = !blendShapeModifier.skinnedMeshRenderer;
        }

        // 元のコンポーネントの設定値を変更しないようにクローン
        private static ParametersPerMenu Clone(this ParametersPerMenu parameters)
        {
            var p = new ParametersPerMenu();
            if(parameters != null)
            {
                if(parameters.objects != null) p.objects = (ObjectToggler[])parameters.objects.Clone();
                if(parameters.blendShapeModifiers != null) p.blendShapeModifiers = (BlendShapeModifier[])parameters.blendShapeModifiers.Clone();
                if(parameters.materialReplacers != null) p.materialReplacers = (MaterialReplacer[])parameters.materialReplacers.Clone();
                if(parameters.materialPropertyModifiers != null) p.materialPropertyModifiers = (MaterialPropertyModifier[])parameters.materialPropertyModifiers.Clone();
                if(parameters.vrcParameterSetters != null) p.vrcParameterSetters = (VRCParameterSetter[])parameters.vrcParameterSetters.Clone();
                if(parameters.clips != null) p.clips = (AnimationClip[])parameters.clips.Clone();
            }
            if(p.objects == null) p.objects = new ObjectToggler[]{};
            if(p.blendShapeModifiers == null) p.blendShapeModifiers = new BlendShapeModifier[]{};
            if(p.materialReplacers == null) p.materialReplacers = new MaterialReplacer[]{};
            if(p.materialPropertyModifiers == null) p.materialPropertyModifiers = new MaterialPropertyModifier[]{};
            if(p.vrcParameterSetters == null) p.vrcParameterSetters = new VRCParameterSetter[]{};
            if(p.clips == null) p.clips = new AnimationClip[]{};
            return p;
        }

        /// <summary>
        /// 将AutoDresser数组转换为Costume数组
        /// </summary>
        /// <param name="dressers">要转换的AutoDresser数组</param>
        /// <param name="avatarRoot">输出参数：Avatar的根节点Transform</param>
        /// <param name="changer">CostumeChanger组件实例</param>
        /// <param name="presets">预设数组</param>
        /// <param name="dresserDefOverride">可选参数：强制指定默认的AutoDresser</param>
        /// <returns>转换后的Costume数组</returns>
        internal static Costume[] DresserToCostumes(this AutoDresser[] dressers, out Transform avatarRoot, CostumeChanger changer, Preset[] presets, AutoDresser dresserDefOverride = null)
        {
            avatarRoot = null;
            // 如果输入的dressers为空，直接返回null
            if(dressers == null || dressers.Length == 0) return null;
            // 用于存储默认的Costume
            Costume def = null;
            // 存储所有转换后的Costume列表
            var costumes = new List<Costume>();
            foreach(var dresser in dressers)
            {
                var obj = dresser.gameObject;
                // 只初始化一次 avatarRoot
                if(avatarRoot == null) avatarRoot = obj.GetAvatarRoot();
                // 创建新的Costume实例并复制AutoDresser的相关属性
                var cos = new Costume{
                    menuName = dresser.menuName,
                    icon = dresser.icon,
                    parentOverride = dresser.parentOverride,
                    parentOverrideMA = dresser.parentOverrideMA,
                    parametersPerMenu = dresser.parameter.Clone()
                };
                // 添加对象切换器，设置为启用状态
                cos.parametersPerMenu.objects = cos.parametersPerMenu.objects.Append(new ObjectToggler{obj = obj, value = true}).ToArray();
                
                // 处理默认Costume的逻辑
                if(dresserDefOverride && dresser == dresserDefOverride) def = cos;
                else if(!dresserDefOverride && obj.activeSelf)
                {
                    // 如果没有指定默认dresser且当前对象是激活状态
                    if(def == null)
                    {
                        def = cos;
                        // 在预设中替换组件
                        presets.ReplaceComponent(dresser, changer, 0);
                    }
                    // 如果已经存在默认Costume，则报错（不允许多个激活的衣装）
                    else ErrorHelper.Report("dialog.error.defaultDuplication", dressers);
                }
                else
                {
                    // 非默认的Costume添加到列表中
                    presets.ReplaceComponent(dresser, changer, costumes.Count);
                    costumes.Add(cos);
                }
            }
            
            // 错误检查：如果没有找到默认Costume
            if(def == null)
            {
                ErrorHelper.Report("dialog.error.allObjectOff", dressers);
                return null;
            }
            // 错误检查：如果没有找到Avatar根节点
            if(!avatarRoot) ErrorHelper.Report("dialog.error.avatarRootNofFound", dressers);
            
            // 将默认Costume插入到列表开头
            costumes.Insert(0, def);
            return costumes.ToArray();
        }

        // PropをItemTogglerに変換
        internal static ParametersPerMenu PropToTogglerParameters(this Prop prop)
        {
            var parameters = prop.parameter.Clone();
            parameters.objects = parameters.objects.Append(new ObjectToggler{obj = prop.gameObject, value = !prop.gameObject.activeSelf}).ToArray();
            return parameters;
        }

        // Preset内のComponentを置換
        internal static void ReplaceComponent(this Preset[] presets, MenuBaseComponent from, MenuBaseComponent to, float value)
        {
            foreach(var item in presets.SelectMany(p => p.presetItems).Where(i => i.obj == from))
            {
                item.obj = to;
                item.value = value;
            }
        }

        // n-bit intの計算
        internal static int ToNBitInt(int costumeCount)
        {
            var bits = 0;
            var n = costumeCount - 1;
            while(n > 0){
                bits++;
                n >>= 1;
            }
            return bits;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    /// <summary>
    /// Avatar标签组件的基础编辑器类
    /// 用于处理Inspector界面的显示和交互逻辑
    /// 支持多对象编辑
    /// </summary>
    [CustomEditor(typeof(AvatarTagComponent), true)] [CanEditMultipleObjects]
    internal class BaseEditor : Editor
    {
        // 版本相关字段
        private static string ndmfVersion = "";
        /// <summary>
        /// 获取NDMF版本号，使用延迟初始化模式
        /// </summary>
        private static string NdmfVersion => string.IsNullOrEmpty(ndmfVersion) ? ndmfVersion = AsmdefReader.Asmdef_LI.versionDefines.FirstOrDefault(v => v.define == "LIL_NDMF").expression : ndmfVersion;
        
        private static string maVersion = "";
        /// <summary>
        /// 获取ModularAvatar版本号，使用延迟初始化模式
        /// </summary>
        private static string MAVersion => string.IsNullOrEmpty(maVersion) ? maVersion = AsmdefReader.Asmdef_LI.versionDefines.FirstOrDefault(v => v.define == "LIL_MODULAR_AVATAR").expression : maVersion;
        
        /// <summary>
        /// 存储菜单文件夹及其包含的子组件的映射关系
        /// Key: 菜单文件夹组件
        /// Value: 该文件夹下的所有菜单基础组件列表
        /// </summary>
        private static readonly Dictionary<MenuFolder, List<MenuBaseComponent>> menuChildren = new();

        /// <summary>
        /// 当Inspector被禁用时调用，用于清理资源和重置状态
        /// </summary>
        void OnDisable()
        {
            OnDisableInternal();
        }

        /// <summary>
        /// 内部禁用处理函数，执行具体的清理操作：
        /// 1. 停止预览
        /// 2. 重置预览状态
        /// 3. 重置参数查看器
        /// 4. 重置Avatar扫描器
        /// 5. 清空菜单子组件缓存
        /// </summary>
        internal static void OnDisableInternal()
        {
            PreviewHelper.instance.StopPreview();
            if(PreviewHelper.doPreview == 1) PreviewHelper.doPreview = 0;
            ParameterViewer.Reset();
            AvatarScanner.Reset();
            menuChildren.Clear();
        }

        /// <summary>
        /// 重写Inspector GUI绘制方法
        /// 负责绘制编辑器界面的所有元素和处理用户交互
        /// </summary>
        public override void OnInspectorGUI()
        {
            // 绘制版本检查器和语言选择界面
            VersionChecker.DrawGUI();
            Localization.SelectLanguageGUI();

            // 检查Modular Avatar和NDMF版本兼容性
            #if !LIL_MODULAR_AVATAR && LIL_MODULAR_AVATAR_ANY
            EditorGUILayout.HelpBox(string.Format(Localization.S("inspector.maTooOld"), MAVersion), MessageType.Error);
            #endif
            #if !LIL_NDMF && LIL_NDMF_ANY
            EditorGUILayout.HelpBox(string.Format(Localization.S("inspector.ndmfTooOld"), NdmfVersion), MessageType.Error);
            #endif

            // 检查AutoDresser组件的激活状态并显示相关警告
            if(target is AutoDresser dresser)
            {
                var root = dresser.gameObject.GetAvatarRoot();
                if(root)
                {
                    // 统计当前激活的AutoDresser组件数量
                    int activeCount = 0;
                    foreach(var d in root.GetComponentsInChildren<AutoDresser>(true))
                    {
                        if(!d.enabled || d.IsEditorOnly()) continue;
                        using var so = new SerializedObject(d.gameObject);
                        using var sp = so.FindProperty("m_IsActive");
                        var a = PreviewHelper.GetFromContainer(d.gameObject, sp.propertyPath);
                        if(a is bool activeSelf && activeSelf) activeCount++;
                        if(a == null && d.gameObject.activeSelf) activeCount++;
                    }

                    // 显示错误提示：所有对象都被禁用或存在多个激活的默认对象
                    if(activeCount == 0) EditorGUILayout.HelpBox(Localization.S("dialog.error.allObjectOff"), MessageType.Error);
                    if(activeCount > 1) EditorGUILayout.HelpBox(Localization.S("dialog.error.defaultDuplication"), MessageType.Error);
                }
            }

            // 检查组件是否被禁用，显示警告信息
            if(targets.All(t => !((AvatarTagComponent)t).enabled)) 
                EditorGUILayout.HelpBox(Localization.S("inspector.componentDisabled"), MessageType.Warning);

            // 检查Prop和AutoDresserSettings组件是否存在重复
            if(target is Prop || target is AutoDresserSettings)
            {
                MenuBaseComponent dis = (target as Component).gameObject.GetComponent<MenuBaseDisallowMultipleComponent>();
                if(!dis && target is Prop) dis = (target as Component).gameObject.GetComponent<AutoDresserSettings>();
                if(!dis && target is AutoDresserSettings) dis = (target as Component).gameObject.GetComponent<Prop>();
                if(dis) EditorGUILayout.HelpBox(string.Format(Localization.S("inspector.componentDuplicate"), target.GetType().Name, dis.GetType().Name), MessageType.Error);
            }

            // 处理菜单基础组件的特殊显示逻辑
            if(target is MenuBaseComponent comp)
            {
                // 检查并显示父级对象的禁用状态
                var unenabled = ObjHelper.UnenabledParent(comp);
                if(unenabled)
                {
                    EditorGUILayout.HelpBox(Localization.S("inspector.parentDisabled"), MessageType.Warning);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(unenabled, typeof(Object), true);
                    EditorGUI.EndDisabledGroup();
                }

                // 显示参数查看器和Avatar扫描结果
                ParameterViewer.Draw(comp);
                AvatarScanner.Update(comp);
                AvatarScanner.Draw(targets);

                // 处理预览功能的显示和控制
                if(targets.Length == 1 && PreviewHelper.instance.ChechTargetHasPreview(target))
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    PreviewHelper.instance.TogglePreview(target);
                    PreviewHelper.instance.DrawIndex(target);
                    EditorGUILayout.EndVertical();
                }
            }

            // 绘制序列化属性
            var hasProperty = false;
            serializedObject.UpdateIfRequiredOrScript();
            using var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true); // 跳过m_Script属性

            // 如果是菜单组件，绘制其基础参数设置
            if(target is MenuBaseComponent)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(Localization.G("inspector.menuSettings"), EditorStyles.boldLabel);
                DrawMenuBaseParameters(target, serializedObject, iterator);
                hasProperty = true;
            }

            // 绘制剩余的序列化属性
            while(iterator.NextVisible(false))
            {
                if(iterator.name == "costumes" || iterator.name == "frames") 
                    GUIHelper.AutoField(iterator, false);
                else 
                    GUIHelper.AutoField(iterator);
                hasProperty = true;
            }

            // 应用修改并更新预览状态
            if(serializedObject.ApplyModifiedProperties()) 
                PreviewHelper.instance.StopPreview();

            // 如果没有任何属性可显示，显示提示信息
            if(!hasProperty) 
                EditorGUILayout.HelpBox(Localization.S("inspector.noProperty"), MessageType.Info);

            // 显示文件夹内容（仅当选中单个MenuFolder对象时）
            if(targets.Length == 1 && target is MenuFolder folder)
            {
                // 初始化菜单子组件缓存
                if(menuChildren.Count == 0)
                {
                    var root = folder.gameObject.GetAvatarRoot();
                    if(root)
                    {
                        var components = folder.gameObject.GetAvatarRoot().GetComponentsInChildren<MenuBaseComponent>(true).Where(c => c.enabled);
                        foreach(var c in components)
                        {
                            if(c is MenuFolder f && !menuChildren.ContainsKey(f)) menuChildren[f] = new List<MenuBaseComponent>();
                            var parent = c.GetMenuParent();
                            if(!parent) continue;
                            if(!menuChildren.ContainsKey(parent)) menuChildren[parent] = new List<MenuBaseComponent>();
                            menuChildren[parent].Add(c);
                        }
                    }
                }

                // 显示文件夹内容
                if(menuChildren.ContainsKey(folder) && menuChildren[folder].Count > 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(Localization.G("inspector.folderContents"), EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    DrawChildren(folder);
                    EditorGUILayout.EndVertical();
                }
            }

            // 添加转换和生成按钮
            if(target is Prop && GUILayout.Button(Localization.S("inspector.convertToItemToggler")))
                foreach(var prop in targets.Select(t => t as Prop).Where(t => t).ToArray())
                    Processor.PropToToggler(new[]{prop}, prop.gameObject.GetAvatarRoot().GetComponentsInChildren<Preset>(true));

            if((target is AutoDresser || target is Prop) && !(target as MenuBaseComponent).parentOverride && 
               GUILayout.Button(Localization.S("inspector.generateMenuFolder")))
                foreach(var c in targets.Select(t => t as MenuBaseComponent).Where(t => t && !t.parentOverride && (t is AutoDresser || t is Prop)).ToArray())
                {
                    Undo.RecordObject(c, "Generate Folder");
                    c.parentOverride = Undo.AddComponent<MenuFolder>(c.gameObject);
                }

            // 启动预览（仅当选中单个对象时）
            if(targets.Length == 1) 
                PreviewHelper.instance.StartPreview(target);
        }

        /// <summary>
        /// 递归绘制菜单文件夹的子组件
        /// </summary>
        /// <param name="root">根菜单文件夹</param>
        /// <param name="current">当前正在处理的文件夹，默认为root</param>
        private static void DrawChildren(MenuFolder root, MenuFolder current = null)
        {
            EditorGUILayout.BeginVertical();
            var folder = current == null ? root : current;
            var components = menuChildren[folder];
            foreach(var c in components)
            {
                if(c.GetMenuParent() != folder) continue;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                ParamsPerChildren(c);
                if(c == root)
                {
                    EditorGUILayout.HelpBox(Localization.S("inspector.folderContentsCircularReference"), MessageType.Error);
                }
                else if(c is MenuFolder f)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(6, false);
                    DrawChildren(root, f);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 显示菜单子对象及其属性
        /// </summary>
        /// <param name="obj">要显示的菜单基础组件</param>
        private static void ParamsPerChildren(MenuBaseComponent obj)
        {
            // 显示对象字段（禁用状态）
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(obj, obj.GetType(), true);
            EditorGUI.EndDisabledGroup();

            // 更新并显示序列化属性
            using var so = new SerializedObject(obj);
            so.UpdateIfRequiredOrScript();

            using var iterator = so.GetIterator();
            iterator.NextVisible(true);
            DrawMenuBaseParameters(obj, so, iterator);

            so.ApplyModifiedProperties();
        }

        // 图标样式缓存
        private static GUIStyle styleIcon => m_StyleIcon != null ? m_StyleIcon : m_StyleIcon = new GUIStyle(EditorStyles.objectFieldThumb){alignment = TextAnchor.MiddleCenter};
        private static GUIStyle m_StyleIcon;

        /// <summary>
        /// 绘制菜单的基础参数设置界面
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="so">序列化对象</param>
        /// <param name="iterator">属性迭代器</param>
        private static void DrawMenuBaseParameters(Object obj, SerializedObject so, SerializedProperty iterator)
        {
            EditorGUILayout.BeginHorizontal();
                // 在左侧绘制大型正方形图标
                var iconSize = EditorGUIUtility.singleLineHeight * 3 + GUIHelper.GetSpaceHeight(3);
                var rectIcon = EditorGUILayout.GetControlRect(GUILayout.Width(iconSize), GUILayout.Height(iconSize));

                EditorGUILayout.BeginVertical();
                #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
                bool isOverridedByMA = so.GetObjectInProperty("parentOverrideMA");
                #else
                bool isOverridedByMA = false;
                #endif

                // 绘制菜单名称
                iterator.NextVisible(false);
                GUIHelper.AutoField(iterator);

                // 如果被MA控制则禁用相关字段
                if(isOverridedByMA) EditorGUI.BeginDisabledGroup(true);
                
                // 绘制父级覆盖设置
                iterator.NextVisible(false);
                GUIHelper.AutoField(iterator);
                
                // 绘制图标设置
                iterator.NextVisible(false);
                EditorGUI.BeginChangeCheck();
                var tex = EditorGUI.ObjectField(rectIcon, iterator.objectReferenceValue, typeof(Texture2D), false);
                if(EditorGUI.EndChangeCheck()) iterator.objectReferenceValue = tex;
                
                // 显示默认图标提示
                if(!isOverridedByMA && !iterator.objectReferenceValue)
                {
                    EditorGUI.LabelField(rectIcon, Localization.G("inspector.icon"), styleIcon);
                    GUIStyle styleOverlay = EditorStyles.objectFieldThumb.name + "Overlay2";
                    EditorGUI.LabelField(rectIcon, "Select", styleOverlay);
                }
                if(isOverridedByMA) EditorGUI.EndDisabledGroup();

                // 绘制MA父级覆盖设置
                iterator.NextVisible(false);
                #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
                GUIHelper.AutoField(iterator);
                #endif
                EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ModularAvatarHelper.Inspector(obj, iterator);
        }

        /// <summary>
        /// 初始化方法，注册对象变更事件处理
        /// 用于修复Avatar外部对象的引用问题
        /// </summary>
        [InitializeOnLoadMethod] private static void Initialize() => ObjectChangeEvents.changesPublished += (ref ObjectChangeEventStream stream) =>
        {
            var components = new HashSet<AvatarTagComponent>();
            for(int i = 0; i < stream.length; i++)
            {
                switch(stream.GetEventType(i))
                {
                    case ObjectChangeKind.ChangeGameObjectParent:
                    {
                        stream.GetChangeGameObjectParentEvent(i, out var data);
                        if(EditorUtility.InstanceIDToObject(data.instanceId) is GameObject go) components.UnionWith(go.GetComponentsInChildren<AvatarTagComponent>(true));
                        break;
                    }
                    case ObjectChangeKind.ChangeGameObjectStructure:
                    {
                        stream.GetChangeGameObjectStructureEvent(i, out var data);
                        if(EditorUtility.InstanceIDToObject(data.instanceId) is GameObject go) components.UnionWith(go.GetComponents<AvatarTagComponent>());
                        break;
                    }
                    default: continue;
                }
            }

            if(components.Count == 0) return;
            foreach(var component in components) FixObjectReferences(component);
        };

        /// <summary>
        /// 修复组件中对Avatar外部对象的引用
        /// 将引用重定向到Avatar内部的对应对象
        /// </summary>
        /// <param name="component">需要修复的Avatar标签组件</param>
        private static void FixObjectReferences(AvatarTagComponent component)
        {
            // 跳过不需要处理的组件
            if(!component || !component.gameObject ||
                component is Comment ||
                component is MaterialModifier ||
                component is MaterialOptimizer
            ) return;

            var root = component.gameObject.GetAvatarRoot();
            if(!root) return;

            // 遍历所有序列化属性
            using var so = new SerializedObject(component);
            using var iter = so.GetIterator();
            var enterChildren = true;
            while(iter.Next(enterChildren))
            {
                enterChildren = iter.propertyType != SerializedPropertyType.String;
                if(iter.propertyType != SerializedPropertyType.ObjectReference) continue;

                // 修复GameObject引用
                if(iter.objectReferenceValue is GameObject gameObject && gameObject.GetAvatarRoot() != root)
                {
                    var lastPath = gameObject.GetPathInAvatar();
                    if(string.IsNullOrEmpty(lastPath)) continue;
                    iter.objectReferenceValue = root.transform.Find(lastPath).gameObject;
                }
                // 修复Component引用
                else if(iter.objectReferenceValue is Component c && c.gameObject.GetAvatarRoot() != root)
                {
                    var lastPath = c.GetPathInAvatar();
                    if(string.IsNullOrEmpty(lastPath)) continue;
                    var t = root.transform.Find(lastPath);
                    if(t) iter.objectReferenceValue = t.GetComponent(c.GetType());
                    else iter.objectReferenceValue = null;
                }
            }
            so.ApplyModifiedProperties();
        }
    }
}

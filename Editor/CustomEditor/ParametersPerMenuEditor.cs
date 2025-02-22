using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    // 通常の要素
    [CustomPropertyDrawer(typeof(LILElement), true)]
    internal class LILElementDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OnGUI(position, property, true);
        }

        internal static void OnGUI(Rect position, SerializedProperty property, bool drawFoldout)
        {
            bool isExpanded = GUIHelper.FoldoutOnly(position, property);
            using var end = property.GetEndProperty();
            property.NextVisible(true);
            bool isObjectArray = property.isArray && property.arrayElementType.StartsWith("PPtr");

            // Foldoutを閉じている場合
            if(!isExpanded)
            {
                // Object名でラベルを表示
                if(isObjectArray) EditorGUI.LabelField(position, string.Join(", ", property.GetAllObjectNames()));
                // もしくはシンプルにプロパティを表示
                else EditorGUI.PropertyField(position.SetHeight(property), property);
                return;
            }

            // Foldoutを開いている場合は後のプロパティを普通に表示
            if(drawFoldout && isObjectArray) position.Indent();
            position = GUIHelper.AutoField(position, property, drawFoldout);
            if(drawFoldout && !isObjectArray) position.Indent();

            while(property.NextVisible(false) && !SerializedProperty.EqualContents(property, end))
            {
                position = GUIHelper.AutoField(position, property, drawFoldout);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(!property.isExpanded) return GUIHelper.propertyHeight;
            using var end = property.GetEndProperty();
            using var iterator = property.Copy();
            iterator.NextVisible(true);
            float height = GUIHelper.GetAutoFieldHeight(iterator);
            while(iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, end))
            {
                height += GUIHelper.GetAutoFieldHeight(iterator);
            }
            return height;
        }
    }

    // 子の要素のFoldoutを表示しない
    [CustomPropertyDrawer(typeof(LILElementWithoutChildrenFoldout), true)]
    internal class LILElementWithoutChildrenFoldoutDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LILElementDrawer.OnGUI(position, property, false);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(!property.isExpanded) return GUIHelper.propertyHeight;
            using var end = property.GetEndProperty();
            using var iterator = property.Copy();
            iterator.NextVisible(true);
            float height = GUIHelper.GetAutoFieldHeight(iterator);
            while(iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, end))
            {
                height += GUIHelper.GetAutoFieldHeight(iterator);
            }
            return height;
        }
    }

    // Foldout完全になし
    [CustomPropertyDrawer(typeof(LILElementSimple), true)]
    internal class LILElementSimpleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using var end = property.GetEndProperty();
            property.NextVisible(true);
            position = GUIHelper.AutoField(position, property);

            while(property.NextVisible(false) && !SerializedProperty.EqualContents(property, end))
            {
                position = GUIHelper.AutoField(position, property);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            using var end = property.GetEndProperty();
            using var iterator = property.Copy();
            iterator.NextVisible(true);
            float height = GUIHelper.GetAutoFieldHeight(iterator);
            while(iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, end))
            {
                height += GUIHelper.GetAutoFieldHeight(iterator);
            }
            return height;
        }
    }

    // ParametersPerMenuはListの要素追加時に初期値を設定したいのでPropertyDrawerを作成
    [CustomPropertyDrawer(typeof(ParametersPerMenu))]
    internal class ParametersPerMenuDrawer : PropertyDrawer
    {
        private static readonly string[] menuKeys = new[]
        {
            "inspector.showObjects",
            "inspector.showBlendShapes",
            "inspector.showMaterials",
            "inspector.showProperties",
            "inspector.showAnimations"
        };

        private static readonly string[] propertyNames = new[]
        {
            "objects",
            "blendShapeModifiers",
            "materialReplacers",
            "materialPropertyModifiers",
            "clips"
        };

        private static readonly string[] editorPrefsKeys = new[]
        {
            "lilycalInventory_showObjects",
            "lilycalInventory_showBlendShapes",
            "lilycalInventory_showMaterials",
            "lilycalInventory_showProperties",
            "lilycalInventory_showAnimations"
        };

        private static bool GetShowState(int index)
        {
            return EditorPrefs.GetBool(editorPrefsKeys[index], false);
        }

        private static void SetShowState(int index, bool value)
        {
            EditorPrefs.SetBool(editorPrefsKeys[index], value);
        }

        private static bool GetShowState(int index, SerializedProperty property)
        {
            using var arrayProp = property.FPR(propertyNames[index]);
            if(arrayProp.arraySize > 0) return true;
            
            return EditorPrefs.GetBool(editorPrefsKeys[index], false);
        }

        private static GUIContent[] GetMenuItems()
        {
            return menuKeys.Select(key => new GUIContent(Localization.S(key))).ToArray();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 添加空行
            position.y += 4;

            // 绘制按钮
            var buttonRect = new Rect(position.x, position.y, position.width, 20);
            if(GUI.Button(buttonRect, Localization.S("inspector.showHideComponents")))
            {
                ShowSettingsMenu(property);
            }

            // 内容区域
            position.y = buttonRect.yMax + 4;

            using var objects = property.FPR(propertyNames[0]);
            using var blendShapeModifiers = property.FPR(propertyNames[1]);
            using var materialReplacers = property.FPR(propertyNames[2]);
            using var materialPropertyModifiers = property.FPR(propertyNames[3]);
            using var clips = property.FPR(propertyNames[4]);

            if(GetShowState(0, property))
            {
                position = GUIHelper.DragAndDropList(position, objects, true, "obj", prop =>
                {
                    prop.FPR("obj").objectReferenceValue = null;
                    prop.FPR("value").boolValue = true;
                }, actionPerObject: (sp,o) => {
                    if(o is not GameObject go) return;
                    sp.FPR("value").boolValue = !go.activeSelf;
                });
            }

            if(GetShowState(1, property))
            {
                position = GUIHelper.DragAndDropList<SkinnedMeshRenderer>(position, blendShapeModifiers, true, "skinnedMeshRenderer", prop =>
                {
                    prop.FPR("skinnedMeshRenderer").objectReferenceValue = null;
                    prop.FPR("blendShapeNameValues").arraySize = 0;
                });
            }

            if(GetShowState(2, property))
            {
                position = GUIHelper.DragAndDropList<Renderer>(position, materialReplacers, true, "renderer", prop =>
                {
                    prop.FPR("renderer").objectReferenceValue = null;
                    prop.FPR("replaceTo").arraySize = 0;
                });
            }

            if(GetShowState(3, property))
            {
                position = GUIHelper.List(position, materialPropertyModifiers);
            }

            if(GetShowState(4, property))
            {
                position = GUIHelper.List(position, clips);
            }
        }

        private void ShowSettingsMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();
            var menuItems = GetMenuItems();

            for(int i = 0; i < menuItems.Length; i++)
            {
                var index = i;
                menu.AddItem(menuItems[i], GetShowState(index, property), () => {
                    // 如果数组长度大于0，不允许隐藏
                    using var arrayProp = property.FPR(propertyNames[index]);
                    if(arrayProp.arraySize > 0) return;
                    
                    SetShowState(index, !GetShowState(index, property));
                });
            }

            menu.ShowAsContext();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 32; // 顶部空行(4) + 按钮高度(20) + 按钮下方间距(4) + 底部间距(4)

            using var objects = property.FPR(propertyNames[0]);
            using var blendShapeModifiers = property.FPR(propertyNames[1]);
            using var materialReplacers = property.FPR(propertyNames[2]);
            using var materialPropertyModifiers = property.FPR(propertyNames[3]);
            using var clips = property.FPR(propertyNames[4]);

            if(GetShowState(0, property))
                height += GUIHelper.GetListHeight(property, propertyNames[0]);
            if(GetShowState(1, property))
                height += GUIHelper.GetListHeight(property, propertyNames[1]);
            if(GetShowState(2, property))
                height += GUIHelper.GetListHeight(property, propertyNames[2]);
            if(GetShowState(3, property))
                height += GUIHelper.GetListHeight(property, propertyNames[3]);
            if(GetShowState(4, property))
                height += GUIHelper.GetListHeight(property, propertyNames[4]);

            return height;
        }
    }

    // オンオフ対象のオブジェクトとトグルを1行で表示
    [CustomPropertyDrawer(typeof(ObjectToggler))]
    internal class ObjectTogglerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var yMax = position.yMax;
            position.height = GUIHelper.propertyHeight;
            var valueRect = new Rect(position.x, position.y, 80, position.height);
            var objRect = new Rect(valueRect.xMax + 4, position.y, position.width - valueRect.width - 4, position.height);

            using var value = property.FPR("value");
            int valueInt = value.boolValue ? 1 : 0;
            EditorGUI.BeginChangeCheck();
            valueInt = EditorGUI.Popup(valueRect, valueInt, new[]{Localization.S("inspector.turnOff"), Localization.S("inspector.turnOn")});
            if(EditorGUI.EndChangeCheck()) value.boolValue = valueInt == 1 ? true : false;

            using var obj = property.FPR("obj");
            EditorGUI.BeginChangeCheck();
            GUIHelper.FieldOnly(objRect, obj);
            if(EditorGUI.EndChangeCheck() && obj.objectReferenceValue is GameObject gameObject)
            {
                value.boolValue = !gameObject.activeSelf;
            }

            position.y = position.yMax + GUIHelper.GetSpaceHeight();
            position.yMax = yMax;
            AvatarScanner.Draw(position, obj.objectReferenceValue);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GUIHelper.propertyHeight + AvatarScanner.Height(property.GetObjectInProperty("obj"));
        }
    }

    // BlendShapeを表示
    [CustomPropertyDrawer(typeof(BlendShapeModifier))]
    internal class BlendShapeModifierDrawer : PropertyDrawer
    {
        private readonly Dictionary<Mesh, string[]> blendShapes = new();
        private Mesh mesh = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // メッシュを選択
            using var smr = property.FPR("skinnedMeshRenderer");
            EditorGUI.PropertyField(position.SingleLine(), smr);
            mesh = null;
            if(smr.objectReferenceValue) mesh = ((SkinnedMeshRenderer)smr.objectReferenceValue).sharedMesh;

            // 後続のBlendShapeが正しく入力されているかどうかを判定するためにメッシュを渡す
            BlendShapeNameValueDrawer.mesh = mesh;

            // Foldoutを表示
            if(!GUIHelper.FoldoutOnly(position, property)) return;

            using var blendShapeNameValues = property.FPR("blendShapeNameValues");
            var path = blendShapeNameValues.propertyPath;
            position = GUIHelper.List(position.NewLine(), blendShapeNameValues, false, prop =>
                {
                    if(!mesh) return;
                    // 追加ボタンデフォルトで行われる追加は一旦削除
                    var bsnvs = prop.serializedObject.FindProperty(path);
                    bsnvs.arraySize--;
                    UpdateBlendShapes(mesh);

                    // BlendShapeのサジェストを表示
                    EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition, Vector2.one), GUIHelper.CreateContents(blendShapes[mesh]), -1, (userData, options, selected) =>
                    {
                        using var blendShapeNameValues2 = (((SerializedProperty,Mesh))userData).Item1;
                        var mesh2 = (((SerializedProperty,Mesh))userData).Item2;
                        blendShapeNameValues2.arraySize++;
                        using var p = blendShapeNameValues2.GetArrayElementAtIndex(blendShapeNameValues2.arraySize - 1);
                        using var name = p.FPR("name");
                        name.stringValue = blendShapes[mesh2][selected];
                        if(blendShapeNameValues2.serializedObject.ApplyModifiedProperties()) PreviewHelper.instance.StopPreview();
                    }, (bsnvs,mesh));
                }
            );
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(!property.isExpanded) return GUIHelper.propertyHeight;
            return GUIHelper.propertyHeight +
                GUIHelper.GetListHeight(property, "blendShapeNameValues") +
                GUIHelper.GetSpaceHeight(2);
        }

        // メッシュからBlendShapeを取得
        private void UpdateBlendShapes(Mesh mesh)
        {
            if(blendShapes.ContainsKey(mesh) && blendShapes[mesh].Length == mesh.blendShapeCount) return;
    
            var blendShapeList = new List<string>();
            for(int i = 0; i < mesh.blendShapeCount; i++)
                blendShapeList.Add(mesh.GetBlendShapeName(i));
            blendShapes[mesh] = blendShapeList.ToArray();
        }
    }

    // BlendShapeと値を1行で表示
    [CustomPropertyDrawer(typeof(BlendShapeNameValue))]
    internal class BlendShapeNameValueDrawer : PropertyDrawer
    {
        public static Mesh mesh = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var colors = EditorStyles.textField.GetColors();

            // BlendShape名が不正な場合は赤文字で警告
            if(mesh && mesh.GetBlendShapeIndex(property.GetStringInProperty("name")) == -1) EditorStyles.textField.SetColors(Color.red);

            var nameRect = new Rect(position.x, position.y, position.width * 0.666f, position.height);
            var valueRect = new Rect(nameRect.xMax + 2, position.y, position.width * 0.333f - 2, position.height);
            EditorGUIUtility.labelWidth = 40;
            GUIHelper.ChildField(nameRect, property, "name");
            GUIHelper.ChildField(valueRect, property, "value");
            EditorGUIUtility.labelWidth = 0;
            EditorStyles.textField.SetColors(colors);
        }
    }

    // Rendererと置き換え先マテリアル
    [CustomPropertyDrawer(typeof(MaterialReplacer))]
    internal class MaterialReplacerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using var renderer = property.FPR("renderer");
            using var replaceTo = property.FPR("replaceTo");
            GUIHelper.FieldOnly(position.SingleLine(), renderer);

            // 配列のサイズをあわせる
            var materials = new Material[0];
            if(renderer.objectReferenceValue)
            {
                materials = ((Renderer)renderer.objectReferenceValue).sharedMaterials;
            }
            replaceTo.ResizeArray(materials.Length, p => p.objectReferenceValue = null);

            position.Indent();
            EditorGUI.LabelField(position.NewLine(), Localization.G("inspector.replaceTo"));
            position = GUIHelper.SimpleList(replaceTo, position.NewLine(), materials);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            using var replaceTo = property.FPR("replaceTo");
            return GUIHelper.propertyHeight * (replaceTo.arraySize + 2) + GUIHelper.GetSpaceHeight(3);
        }
    }

    // マテリアルのベクトルプロパティの表示
    [CustomPropertyDrawer(typeof(VectorModifier))]
    internal class VectorModifierDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using var propertyName = property.FPR("propertyName");
            using var value = property.FPR("value");
            using var disableX = property.FPR("disableX");
            using var disableY = property.FPR("disableY");
            using var disableZ = property.FPR("disableZ");
            using var disableW = property.FPR("disableW");

            // プロパティ名
            GUIHelper.AutoField(position.SingleLine(), propertyName);

            // ベクトル
            var fieldPosition = EditorGUI.PrefixLabel(position.NewLine(), Localization.G(value));
            var fieldWidth = fieldPosition.width * 0.25f;
            EditorGUIUtility.labelWidth = 12;
            fieldPosition.width = fieldWidth - 2;

            EditorGUI.BeginChangeCheck();
            var vec = value.vector4Value;
            vec.x = FloatField(ref fieldPosition, "X", vec.x, disableX.boolValue, fieldWidth);
            vec.y = FloatField(ref fieldPosition, "Y", vec.y, disableY.boolValue, fieldWidth);
            vec.z = FloatField(ref fieldPosition, "Z", vec.z, disableZ.boolValue, fieldWidth);
            vec.w = FloatField(ref fieldPosition, "W", vec.w, disableW.boolValue, fieldWidth);
            if(EditorGUI.EndChangeCheck()) value.vector4Value = vec;

            EditorGUIUtility.labelWidth = 0;

            // トグルをベクトルの各要素の位置に合わせて表示
            position.NewLine();
            position = EditorGUI.PrefixLabel(position, Localization.G("inspector.disable"));
            position.width *= 0.25f;
            GUIHelper.AutoField(position, disableX);
            position.x = position.xMax;
            GUIHelper.AutoField(position, disableY);
            position.x = position.xMax;
            GUIHelper.AutoField(position, disableZ);
            position.x = position.xMax;
            GUIHelper.AutoField(position, disableW);
        }

        // 位置を更新しつつ表示する
        float FloatField(ref Rect rect, string label, float value, bool disabled, float fieldWidth)
        {
            if(disabled) EditorGUI.BeginDisabledGroup(true);
            value = EditorGUI.FloatField(rect, label, value);
            if(disabled) EditorGUI.EndDisabledGroup();
            rect.x += fieldWidth;
            return value;
        }

        // プロパティ名、ベクトル、トグルで3行
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GUIHelper.propertyHeight * 3 + GUIHelper.GetSpaceHeight(3);
        }
    }

    // プリセット用のエディタ
    [CustomPropertyDrawer(typeof(PresetItem))]
    internal class PresetItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var objRect = new Rect(position.x, position.y, position.width - 120 - 16, position.height);
            var valueRect = new Rect(objRect.xMax + 16, position.y, 120, position.height);
            using var obj = property.FPR("obj");
            using var value = property.FPR("value");

            GUIHelper.FieldOnly(objRect, obj);
            var valueLabel = Localization.G("inspector.presetItemValue");

            EditorGUIUtility.labelWidth = 60;
            void AsBool()
            {
                EditorGUI.BeginChangeCheck();
                var valueBool = EditorGUI.Popup(valueRect, valueLabel, value.floatValue > 0.5f ? 1 : 0, new[]{new GUIContent("False"), new GUIContent("True")}) > 0;
                if(EditorGUI.EndChangeCheck()) value.floatValue = valueBool ? 1 : 0;
            }

            void AsInt()
            {
                EditorGUI.BeginChangeCheck();
                var valueInt = EditorGUI.IntField(valueRect, valueLabel, (int)value.floatValue);
                if(EditorGUI.EndChangeCheck()) value.floatValue = valueInt;
            }

            void AsFloat()
            {
                EditorGUI.BeginChangeCheck();
                var valueFloat = EditorGUI.FloatField(valueRect, valueLabel, value.floatValue);
                if(EditorGUI.EndChangeCheck()) value.floatValue = valueFloat;
            }

            // コンポーネントが生成するパラメーターの型に応じてエディタを変える
            if(!obj.objectReferenceValue)
            {
                GUI.enabled = false;
                AsFloat();
                GUI.enabled = true;
            }
            else if(obj.objectReferenceValue is AutoDresser)
            {
                GUI.enabled = false;
                EditorGUI.Popup(valueRect, valueLabel, 0, new[]{new GUIContent("True")});
                GUI.enabled = true;
            }
            else if(obj.objectReferenceValue is CostumeChanger) AsInt();
            else if(obj.objectReferenceValue is ItemToggler) AsBool();
            else if(obj.objectReferenceValue is Prop) AsBool();
            else if(obj.objectReferenceValue is SmoothChanger) AsFloat();
            else obj.objectReferenceValue = null;
            EditorGUIUtility.labelWidth = 0;
        }
    }
}

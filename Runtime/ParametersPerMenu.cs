using System;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // CostumeChangerの各衣装に付けられるパラメーター
    [Serializable]
    internal class Costume : LILElement
    {
        [CostumeName] public string menuName;
        [LILLocalize] public Texture2D icon;
        [LILLocalize] public MenuFolder parentOverride;
        #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
        [LILLocalize] public nadena.dev.modular_avatar.core.ModularAvatarMenuItem parentOverrideMA;
        #else
        [HideInInspector] public UnityEngine.Object parentOverrideMA;
        #endif
        public AutoDresser autoDresser;
        public ParametersPerMenu parametersPerMenu = new ParametersPerMenu();
    }

    // SmoothChangerの各フレームに付けられるパラメーター
    [Serializable]
    internal class Frame : LILElement
    {
        [Frame] public float frameValue = 0;
        public ParametersPerMenu parametersPerMenu = new ParametersPerMenu();
    }

    // 各メニュー操作コンポーネントの設定をまとめたクラス
    [Serializable]
    internal class ParametersPerMenu
    {
        public ObjectToggler[] objects = new ObjectToggler[]{};
        public BlendShapeModifier[] blendShapeModifiers = new BlendShapeModifier[]{};
        public MaterialReplacer[] materialReplacers = new MaterialReplacer[]{};
        public MaterialPropertyModifier[] materialPropertyModifiers = new MaterialPropertyModifier[]{};
        public VRCParameterSetter[] vrcParameterSetters = new VRCParameterSetter[]{};
        public AnimationClip[] clips = new AnimationClip[]{};
    }

    [Serializable]
    internal class ObjectToggler
    {
        public GameObject obj = null;
        public bool value = true;
    }

    [Serializable]
    internal class BlendShapeModifier
    {
        [LILLocalize] public SkinnedMeshRenderer skinnedMeshRenderer;
        public BlendShapeNameValue[] blendShapeNameValues;
        [NonSerialized] internal bool applyToAll = false;
    }

    [Serializable]
    internal struct BlendShapeNameValue
    {
        public string name;
        public float value;
    }

    [Serializable]
    internal class MaterialReplacer
    {
        public Renderer renderer;
        public Material[] replaceTo;
    }

    [Serializable]
    internal class MaterialPropertyModifier : LILElementWithoutChildrenFoldout
    {
        public Renderer[] renderers;
        public FloatModifier[] floatModifiers;
        public VectorModifier[] vectorModifiers;
    }

    [Serializable]
    internal struct FloatModifier : LILElementSimple
    {
        [LILLocalize] public string propertyName;
        [LILLocalize] public float value;
    }

    [Serializable]
    internal struct VectorModifier : LILElementSimple
    {
        [LILLocalize] public string propertyName;
        [OneLineVector] public Vector4 value;
        [NoLabel] public bool disableX;
        [NoLabel] public bool disableY;
        [NoLabel] public bool disableZ;
        [NoLabel] public bool disableW;
    }

    // PropertyDrawerで制御するためのインターフェース
    interface LILElement {}
    interface LILElementWithoutChildrenFoldout {}
    interface LILElementSimple {}

    // VRCパラメータを設定するためのクラス
    [Serializable]
    internal class VRCParameterSetter : LILElement
    {
        [LILLocalize] public string parameterName;
        public VRCParameterType parameterType = VRCParameterType.Float;
        public VRCParameterOperationType operationType = VRCParameterOperationType.Set;
        [LILLocalize] public float floatValue;
        [LILLocalize] public int intValue;
        [LILLocalize] public bool boolValue;
        [LILLocalize] public string sourceParameterName; // 用于Copy模式，指定要复制的源参数名
        
        // 状态退出时的操作
        public bool executeOnExit = false; // 是否在状态退出时执行
        public VRCParameterOperationType exitOperationType = VRCParameterOperationType.Set;
        [LILLocalize] public float exitFloatValue;
        [LILLocalize] public int exitIntValue;
        [LILLocalize] public bool exitBoolValue;
        [LILLocalize] public string exitSourceParameterName; // 用于Copy模式，指定要复制的源参数名
    }

    // VRCパラメータの型
    internal enum VRCParameterType
    {
        Float,
        Int,
        Bool
    }

    // VRCパラメータの操作タイプ
    internal enum VRCParameterOperationType
    {
        Set,
        Copy
    }
}

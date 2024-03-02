using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    [DisallowMultipleComponent]
    internal abstract class MenuBaseComponent : AvatarTagComponent
    {
        [NotKeyable] [MenuName] public string menuName;
        [NotKeyable] [MenuFolderOverride] public MenuFolder parentOverride;
        [NotKeyable] [LILLocalize] public Texture2D icon;
        #if LIL_MODULAR_AVATAR
        [NotKeyable] [LILLocalize] public nadena.dev.modular_avatar.core.ModularAvatarMenuItem parentOverrideMA;
        #else
        [NotKeyable] [HideInInspector] public Object parentOverrideMA;
        #endif
    }
}

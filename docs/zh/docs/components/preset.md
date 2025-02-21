# LIICON(LI_Script_Preset.png) LI Preset

这是一个用于批量操作 lilycalInventory 的菜单类组件，可以同时切换多个对象的组件。

## 规格说明

设置内容在构建时会转换为 State 和 ParameterDriver。用于 State 切换的参数都是非 Synced 的，因此不会增加参数内存。而且无论有多少个 LI Preset，添加到 AnimatorController 的层数都只有 1 个（如果不存在 LI Preset 则为 0）。

在构建时具体会执行以下处理：

- 在 AnimatorController 和 ExpressionParameters 中添加以 `菜单・参数名称` 设置的名称作为 Bool 参数（非 Synced）
- 在 AnimatorController 中添加层，添加 DefaultState 和空的 AnimationClip
- 为每个 LI Preset 添加 Transition 和 State・空的 AnimationClip
- 在生成的 State 中添加 VRC Avatar Parameter Driver，根据 LI Preset 的设置设定参数名称和值
- 生成用于设置 Bool 值的 Toggle 菜单

## 设置项目

### 菜单设置

#include "docs/zh/docs/components/_menu_folder_settings_table.md"

### 操作项目

可以指定要操作的组件和要设置的值。对于 AutoDresser，无需输入值，会切换到设置的服装。 
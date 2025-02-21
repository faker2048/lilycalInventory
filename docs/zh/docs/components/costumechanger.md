# LIICON(LI_Script_CostumeChanger.png) LI CostumeChanger

这是一个用于切换服装的组件。

## 规格说明

使用 Int 类型进行控制。注册的服装在构建时会转换为 AnimationClip 和 AnimatorController 的 State。根据参数值的变化，State 会进行切换，从而播放注册的服装动画。

在构建时具体会执行以下处理：

- 创建包含各服装设置值和预制体初始值的 AnimationClip
- 为防止同步事故，将对象的开关状态调整为与组件设置一致
- 用预制体初始值填充各服装的未设置值
- 在 AnimatorController 和 ExpressionParameters 中添加以 `菜单・参数名称` 设置的名称作为 Int 参数
- 将 `保存启用状态` 和 `仅本地生效` 设置复制到 ExpressionParameters
- 在 AnimatorController 中添加层，注册 State、AnimationClip 和 Transition
- 生成用于设置 Int 值的 Toggle 菜单

## 设置项目

### 菜单设置

#include "docs/zh/docs/components/_menu_settings_table.md"

### 服装（可指定多个）

#include "docs/zh/docs/components/_menu_folder_settings_table.md"

#include "docs/zh/docs/components/_additional_settings_table.md"

### 详细设置

|名称|说明|
|-|-|
|默认状态的参数值|可以指定创建的菜单使用的参数的初始值（Int 值）。| 
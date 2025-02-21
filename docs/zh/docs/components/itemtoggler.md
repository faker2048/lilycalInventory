# LIICON(LI_Script_ItemToggler.png) LI ItemToggler

这是一个用于切换小物件的组件。

## 规格说明

使用 Bool 类型进行控制。设置内容在构建时会转换为 AnimationClip 和 AnimatorController 的 State。根据参数值的变化，State 会进行切换，从而播放动画。

通常，此组件不会切换其所附加的对象本身。如果需要切换组件所附加的对象本身，使用 [LI Prop](prop) 会更方便。

在构建时具体会执行以下处理：

- 分别创建包含组件设置值和预制体初始值的 AnimationClip
- 为防止同步事故，将对象的开关状态调整为与组件设置一致
- 在 AnimatorController 和 ExpressionParameters 中添加以 `菜单・参数名称` 设置的名称作为 Bool 参数
- 将 `保存启用状态` 和 `仅本地生效` 设置复制到 ExpressionParameters
- 在 AnimatorController 中添加层，注册 State、AnimationClip 和 Transition
- 生成用于设置 Bool 值的 Toggle 菜单

## 设置项目

### 菜单设置

#include "docs/zh/docs/components/_menu_settings_table.md"

### 动画设置

#include "docs/zh/docs/components/_additional_settings_table.md"

### 详细设置

|名称|说明|
|-|-|
|默认状态的参数值|可以指定创建的菜单使用的参数的初始值（Bool 值）。| 
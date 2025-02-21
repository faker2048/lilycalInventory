# LIICON(LI_Script_SmoothChanger.png) LI SmoothChanger

这是一个用于控制 BlendShape 等需要无级调节的对象的组件。

## 规格说明

使用 Float 类型进行控制。注册的帧在构建时会转换为 AnimationClip 和 AnimatorController 的 BlendTree。根据参数值的变化，注册的帧的动画会进行混合。

在构建时具体会执行以下处理：

- 创建包含各帧设置值和预制体初始值的 AnimationClip
- 为防止同步事故，将对象的开关状态调整为与组件设置一致
- 用预制体初始值填充各帧的未设置值
- 在 AnimatorController 和 ExpressionParameters 中添加以 `菜单・参数名称` 设置的名称作为 Float 参数
- 将 `保存启用状态` 和 `仅本地生效` 设置复制到 ExpressionParameters
- 在 AnimatorController 中添加层，注册 State、BlendTree 和 AnimationClip
- 生成用于通过 RadialPuppet 控制 Float 值的菜单

## 设置项目

### 菜单设置

#include "docs/zh/docs/components/_menu_settings_table.md"

### 动画设置

|名称|说明|
|-|-|
|操纵杆初始值(%)|可以指定创建的菜单使用的参数的初始值。|

#### 帧（可指定多个）

|名称|说明|
|-|-|
|操纵杆设置值(%)|指定分配给帧的参数值。|

#include "docs/zh/docs/components/_additional_settings_table.md" 
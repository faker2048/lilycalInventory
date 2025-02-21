# LIICON(LI_Script_AutoFixMeshSettings.png) LI AutoFixMeshSettings

这是一个用于统一角色内所有网格（Renderer）设置的组件。

## 规格说明

在构建时会扫描角色内的渲染器（Mesh Renderer、Skinned Mesh Renderer、Particle System Renderer），并将设置内容调整为与此组件一致。`Update When Offscreen`、`Skinned Motion Vectors`、`Root Bone`、`Bounds` 这些设置仅会更改 Skinned Mesh Renderer。

## 设置项目

|名称|说明|
|-|-|
|要排除的渲染器（可指定多个）|可以指定不包含在设置统一对象中的渲染器。|
|网格设置（面向高级用户）|可以自定义设置统一的内容。通常不需要更改此处。此项目的内容遵循 Unity 标准的 Renderer。| 
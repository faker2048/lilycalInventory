# 角色体型调整

本页介绍如何为角色添加体型调整菜单。除了体型之外，这个功能也可以用于操作其他 BlendShape 等多种用途。

## 操作方法

1. 在层级视图中右键点击，选择 `Create Empty` 在角色内创建新对象
2. 在该对象上添加 `LI SmoothChanger` 组件
3. 点击帧的 + 按钮，将帧值设为 0，在 `BlendShape 切换` 中指定要操作的网格和 BlendShape（如果未指定网格则操作所有网格）
4. 点击帧的 + 按钮，将帧值设为 1，按照步骤 3 的方法进行设置

<video controls="controls" src="/images/ja/tutorial/morph.webm" /> 
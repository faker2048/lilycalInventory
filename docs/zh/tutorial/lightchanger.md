# 角色亮度调整

本页介绍如何添加亮度调整菜单。

## 操作方法

如果使用 lilToon，只需将 Prefabs 文件夹中的 LightChanger 拖放到角色上即可完成。

1. 打开 Packages
2. 打开 lilycalInventory 文件夹

![在 Packages 目录中选择 lilycalInventory](/images/ja/tutorial/lightchanger_1.png "在 Packages 目录中选择 lilycalInventory")

3. 打开 Prefabs 文件夹

![打开 Prefabs 文件夹](/images/ja/tutorial/lightchanger_2.png "打开 Prefabs 文件夹")

4. 将 LightChanger 拖放到角色上

![将 LightChanger 拖放到角色上](/images/ja/tutorial/lightchanger_3.png "将 LightChanger 拖放到角色上")

## 手动设置的情况

对于其他着色器，需要手动设置属性。

1. 在层级视图中右键点击，选择 `Create Empty` 在角色内创建新对象
2. 在该对象上添加 `LI SmoothChanger` 组件
3. 点击帧的 + 按钮，将帧值设为 0，在 `材质属性操作` 中指定要操作的网格和属性（如果未指定网格则操作所有网格）
4. 点击帧的 + 按钮，将帧值设为 1，按照步骤 3 的方法进行设置 
# 安装方法

有几种不同的安装方法，但您只需要选择以下4种方法中的任意一种即可。我个人推荐使用 VCC 进行安装！

[[toc]]

## 【推荐！】通过 VCC 安装

虽然看起来有点复杂，但只要完成一次这个步骤，就可以像 VRCSDK 一样通过 VCC 一键更新了！

1. [点击这里](vcc://vpm/addRepo?url=https://lilxyzw.github.io/vpm-repos/vpm.json)打开 VCC，然后点击 `I Understand, Add Repository`。这将把 lilycalInventory 添加到 VCC 中。
2. 安装完成后，点击 `Projects` 返回项目选择界面。

![VCC 包安装界面](/images/ja/tutorial/vcc_packages.png "VCC 包安装界面")

3. 在项目选择界面点击 `Manage Project` 打开管理界面。

![VCC Projects 界面](/images/ja/tutorial/vcc_projects.png "VCC Projects 界面")

4. 最后点击 lilycalInventory 右侧的 `+` 按钮即可完成安装！

![VCC Manage 界面](/images/ja/tutorial/vcc_manage.png "VCC Manage 界面")

## 通过 Unitypackage 安装

从[这里](https://github.com/lilxyzw/lilycalInventory/releases)下载 unitypackage。将 unitypackage 拖放到 Unity 窗口中即可导入。

::: warning
使用此方法需要在每次更新时都从下载页面重新下载。
:::

![GitHub 下载页面](/images/ja/tutorial/github_unitypackage.png "GitHub 下载页面")

## 通过 VPMCLI 安装

如果使用 VPMCLI，可以使用以下命令进行安装：

```
vpm add repo https://lilxyzw.github.io/vpm-repos/vpm.json

cd /path/to/your-unity-project
vpm add package jp.lilxyzw.lilycalinventory
```

## 通过 vrc-get 安装

如果使用 vrc-get，可以使用以下命令进行安装：

```
vrc-get repo add https://lilxyzw.github.io/vpm-repos/vpm.json

cd /path/to/your-unity-project
vrc-get install jp.lilxyzw.lilycalinventory
``` 
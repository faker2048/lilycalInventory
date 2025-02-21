# 多套服装切换

本页介绍如何切换角色的多套服装。

## 操作方法

<u>只需要在每套服装的根对象上添加 `LI AutoDresser` 组件，并确保只有一套服装处于开启状态</u>即可！此时处于开启状态的服装将成为默认服装。

::: info
如果服装没有统一的对象结构（比如角色的标准服装），请在服装的任意部分添加 `LI AutoDresser` 组件，并在 `一起操作的参数` 的 `对象开关` 中添加服装的其他部分。
:::

<video controls="controls" src="/images/ja/tutorial/costume.webm" />

## 不想直接在服装上添加组件的情况

可以使用 `LI CostumeChanger` 代替。使用这个组件时，请手动设置需要开关的对象。 
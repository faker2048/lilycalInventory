# 与 NDMF 和其他工具的兼容性

lilycalInventory 可以独立运行，但如果项目中存在 NDMF，它将作为 NDMF 插件运行。作为 NDMF 插件运行时，由于已预设了运行顺序，因此可以与已知的其他 NDMF 插件同时使用。

## Modular Avatar

通过在 lilycalInventory 的各个组件中设置 `MA Menu Item`，可以让 Modular Avatar 负责生成菜单。

## lilToon

支持使用附带的预制体进行距离淡入淡出和统一光照设置，以及使用 LightChanger 生成亮度调整菜单。 
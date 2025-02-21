# 使用 Direct Blend Tree 进行优化

当 `Tools/lilycalInventory/Use Direct Blend Tree` 开启时，将使用 `Direct Blend Tree` 将此工具生成的 AnimatorController 层合并为一个。此功能利用了 [即使 ExpressionParameters 和 AnimatorController 中的参数类型不同也能正常工作](https://creators.vrchat.com/avatars/animator-parameters/#mismatched-parameter-type-conversion) 的特性。 
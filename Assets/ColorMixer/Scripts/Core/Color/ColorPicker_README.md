# ColorPicker 重构文档

## 概述
ColorPicker 是一个用于从场景中的 3D 物体或 UI 元素中提取颜色的系统。通过右键点击实现了 `IColorPickable<T>` 泛型接口的物体，可以获取其颜色并进行混合。

## 主要改进

### 1. 代码结构优化
- **使用 Region 分组**: 将代码按功能分组（Serialized Fields, Properties, Events, Unity Lifecycle 等）
- **方法职责单一**: 将大型方法拆分为多个小方法，每个方法只负责一个功能
- **命名规范**: 使用清晰的命名约定，提高代码可读性

### 2. 输入系统改进
- **Input System 支持**: 完全支持 Unity 新输入系统
- **空值检查**: 添加 `Mouse.current` 的空值检查，防止运行时错误
- **更清晰的输入处理**: 将输入处理逻辑独立到 `HandleInput()` 方法

### 3. 泛型接口处理
- **反射机制**: 使用反射动态查找和调用 `IColorPickable<T>` 接口的 `GetColor()` 方法
- **支持任意泛型类型**: 不再限制特定的泛型参数类型
- **错误处理**: 添加 try-catch 块处理反射调用可能出现的异常

### 4. 颜色拾取逻辑
- **分离 3D 和 UI 拾取**: 
  - `TryPickColorFrom3DObject()`: 处理 3D 物体的颜色拾取
  - `TryPickColorFromUI()`: 处理 UI 元素的颜色拾取
- **通用组件查找**: `GetColorFromPickableComponents()` 方法可处理任何 GameObject

### 5. 事件系统
添加了三个事件供外部监听：
- `OnColorPicked`: 当拾取颜色时触发 (Color color, int slotIndex)
- `OnColorsMixed`: 当颜色混合完成时触发 (Color mixedColor)
- `OnColorSlotsCleared`: 当颜色槽被清空时触发

### 6. 公共 API
提供了完整的公共方法：
- `MixCurrentColors()`: 手动触发颜色混合
- `ClearColorSlots()`: 清空所有颜色槽
- `GetColorFromSlot(int slotIndex)`: 获取指定槽的颜色
- `SetColorToSlot(int slotIndex, Color color)`: 设置指定槽的颜色

### 7. 属性访问
添加了只读属性以安全访问内部状态：
- `ColorSlots`: 获取颜色槽数组
- `MixedColor`: 获取混合后的颜色
- `PickCount`: 获取拾取次数
- `IsMixingComplete`: 检查是否已完成混合

### 8. 可配置参数
通过 Inspector 可配置：
- `mainCamera`: 主摄像机引用
- `maxColorSlots`: 颜色槽数量（默认 2）
- `autoMixOnSecondPick`: 是否在第二次拾取时自动混合
- `showDebugGUI`: 是否显示调试 GUI

### 9. 改进的 Debug GUI
- 动态显示所有颜色槽
- 显示当前状态（拾取次数、混合状态）
- 空槽使用灰色背景
- 槽位标签

## 使用方法

### 基本设置

1. **将 ColorPicker 组件添加到场景中**
```csharp
// 在 Inspector 中设置 Main Camera
// 或者自动查找 Camera.main
```

2. **实现 IColorPickable 接口**
```csharp
public class MyColorObject : MonoBehaviour, IColorPickable<MeshRenderer>
{
    public Color GetColor()
    {
        // 返回物体的颜色
        return GetComponent<MeshRenderer>().material.color;
    }

    public void OnColorPicked(Color color, MeshRenderer target)
    {
        // 颜色被拾取时的回调
        Debug.Log($"Color picked: {color}");
    }
}
```

### 监听事件

```csharp
public class MyController : MonoBehaviour
{
    [SerializeField] private ColorPicker colorPicker;

    private void OnEnable()
    {
        colorPicker.OnColorPicked += HandleColorPicked;
        colorPicker.OnColorsMixed += HandleColorsMixed;
    }

    private void OnDisable()
    {
        colorPicker.OnColorPicked -= HandleColorPicked;
        colorPicker.OnColorsMixed -= HandleColorsMixed;
    }

    private void HandleColorPicked(Color color, int slotIndex)
    {
        Debug.Log($"Color {color} picked into slot {slotIndex}");
    }

    private void HandleColorsMixed(Color mixedColor)
    {
        Debug.Log($"Mixed color: {mixedColor}");
        // 应用混合颜色到目标物体
    }
}
```

### 手动操作

```csharp
// 手动混合颜色
colorPicker.MixCurrentColors();

// 清空所有槽
colorPicker.ClearColorSlots();

// 获取特定槽的颜色
Color color = colorPicker.GetColorFromSlot(0);

// 设置颜色到特定槽
colorPicker.SetColorToSlot(1, Color.red);

// 检查状态
if (colorPicker.IsMixingComplete)
{
    Color result = colorPicker.MixedColor;
}
```

## 工作流程

1. **首次拾取**: 右键点击物体 → 颜色存入槽 0
2. **第二次拾取**: 右键点击另一物体 → 颜色存入槽 1 → 自动混合 → 结果存入槽 0
3. **后续拾取**: 混合完成后，新颜色存入槽 1

## 示例场景

查看 `ExampleColorPickerUsage.cs` 了解完整的使用示例，包括：
- 事件订阅
- 手动操作（按键 M/C/1/2）
- 颜色应用到目标物体
- 状态查询

## 注意事项

1. 确保场景中有 EventSystem（用于 UI 拾取）
2. 确保使用了 Unity Input System 包
3. 实现 `IColorPickable<T>` 的组件必须附加到可点击的物体上
4. 3D 物体需要有 Collider 组件才能被射线检测

## 性能优化

- 使用反射查找接口实现（可能有轻微性能开销）
- 建议在需要高性能的场景中缓存接口引用
- Debug GUI 仅在开发时启用

## 扩展建议

1. **添加更多混合模式**
   - 加法混合
   - 乘法混合
   - 叠加混合

2. **支持更多颜色槽**
   - 通过 `maxColorSlots` 参数配置

3. **添加撤销/重做功能**
   - 使用命令模式保存操作历史

4. **可视化改进**
   - 使用 UI Toolkit 替代 OnGUI
   - 添加动画效果

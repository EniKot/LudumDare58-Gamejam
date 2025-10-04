# ColorPicker �ع��ĵ�

## ����
ColorPicker ��һ�����ڴӳ����е� 3D ����� UI Ԫ������ȡ��ɫ��ϵͳ��ͨ���Ҽ����ʵ���� `IColorPickable<T>` ���ͽӿڵ����壬���Ի�ȡ����ɫ�����л�ϡ�

## ��Ҫ�Ľ�

### 1. ����ṹ�Ż�
- **ʹ�� Region ����**: �����밴���ܷ��飨Serialized Fields, Properties, Events, Unity Lifecycle �ȣ�
- **����ְ��һ**: �����ͷ������Ϊ���С������ÿ������ֻ����һ������
- **�����淶**: ʹ������������Լ������ߴ���ɶ���

### 2. ����ϵͳ�Ľ�
- **Input System ֧��**: ��ȫ֧�� Unity ������ϵͳ
- **��ֵ���**: ��� `Mouse.current` �Ŀ�ֵ��飬��ֹ����ʱ����
- **�����������봦��**: �����봦���߼������� `HandleInput()` ����

### 3. ���ͽӿڴ���
- **�������**: ʹ�÷��䶯̬���Һ͵��� `IColorPickable<T>` �ӿڵ� `GetColor()` ����
- **֧�����ⷺ������**: ���������ض��ķ��Ͳ�������
- **������**: ��� try-catch �鴦������ÿ��ܳ��ֵ��쳣

### 4. ��ɫʰȡ�߼�
- **���� 3D �� UI ʰȡ**: 
  - `TryPickColorFrom3DObject()`: ���� 3D �������ɫʰȡ
  - `TryPickColorFromUI()`: ���� UI Ԫ�ص���ɫʰȡ
- **ͨ���������**: `GetColorFromPickableComponents()` �����ɴ����κ� GameObject

### 5. �¼�ϵͳ
����������¼����ⲿ������
- `OnColorPicked`: ��ʰȡ��ɫʱ���� (Color color, int slotIndex)
- `OnColorsMixed`: ����ɫ������ʱ���� (Color mixedColor)
- `OnColorSlotsCleared`: ����ɫ�۱����ʱ����

### 6. ���� API
�ṩ�������Ĺ���������
- `MixCurrentColors()`: �ֶ�������ɫ���
- `ClearColorSlots()`: ���������ɫ��
- `GetColorFromSlot(int slotIndex)`: ��ȡָ���۵���ɫ
- `SetColorToSlot(int slotIndex, Color color)`: ����ָ���۵���ɫ

### 7. ���Է���
�����ֻ�������԰�ȫ�����ڲ�״̬��
- `ColorSlots`: ��ȡ��ɫ������
- `MixedColor`: ��ȡ��Ϻ����ɫ
- `PickCount`: ��ȡʰȡ����
- `IsMixingComplete`: ����Ƿ�����ɻ��

### 8. �����ò���
ͨ�� Inspector �����ã�
- `mainCamera`: �����������
- `maxColorSlots`: ��ɫ��������Ĭ�� 2��
- `autoMixOnSecondPick`: �Ƿ��ڵڶ���ʰȡʱ�Զ����
- `showDebugGUI`: �Ƿ���ʾ���� GUI

### 9. �Ľ��� Debug GUI
- ��̬��ʾ������ɫ��
- ��ʾ��ǰ״̬��ʰȡ���������״̬��
- �ղ�ʹ�û�ɫ����
- ��λ��ǩ

## ʹ�÷���

### ��������

1. **�� ColorPicker �����ӵ�������**
```csharp
// �� Inspector ������ Main Camera
// �����Զ����� Camera.main
```

2. **ʵ�� IColorPickable �ӿ�**
```csharp
public class MyColorObject : MonoBehaviour, IColorPickable<MeshRenderer>
{
    public Color GetColor()
    {
        // �����������ɫ
        return GetComponent<MeshRenderer>().material.color;
    }

    public void OnColorPicked(Color color, MeshRenderer target)
    {
        // ��ɫ��ʰȡʱ�Ļص�
        Debug.Log($"Color picked: {color}");
    }
}
```

### �����¼�

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
        // Ӧ�û����ɫ��Ŀ������
    }
}
```

### �ֶ�����

```csharp
// �ֶ������ɫ
colorPicker.MixCurrentColors();

// ������в�
colorPicker.ClearColorSlots();

// ��ȡ�ض��۵���ɫ
Color color = colorPicker.GetColorFromSlot(0);

// ������ɫ���ض���
colorPicker.SetColorToSlot(1, Color.red);

// ���״̬
if (colorPicker.IsMixingComplete)
{
    Color result = colorPicker.MixedColor;
}
```

## ��������

1. **�״�ʰȡ**: �Ҽ�������� �� ��ɫ����� 0
2. **�ڶ���ʰȡ**: �Ҽ������һ���� �� ��ɫ����� 1 �� �Զ���� �� �������� 0
3. **����ʰȡ**: �����ɺ�����ɫ����� 1

## ʾ������

�鿴 `ExampleColorPickerUsage.cs` �˽�������ʹ��ʾ����������
- �¼�����
- �ֶ����������� M/C/1/2��
- ��ɫӦ�õ�Ŀ������
- ״̬��ѯ

## ע������

1. ȷ���������� EventSystem������ UI ʰȡ��
2. ȷ��ʹ���� Unity Input System ��
3. ʵ�� `IColorPickable<T>` ��������븽�ӵ��ɵ����������
4. 3D ������Ҫ�� Collider ������ܱ����߼��

## �����Ż�

- ʹ�÷�����ҽӿ�ʵ�֣���������΢���ܿ�����
- ��������Ҫ�����ܵĳ����л���ӿ�����
- Debug GUI ���ڿ���ʱ����

## ��չ����

1. **��Ӹ�����ģʽ**
   - �ӷ����
   - �˷����
   - ���ӻ��

2. **֧�ָ�����ɫ��**
   - ͨ�� `maxColorSlots` ��������

3. **��ӳ���/��������**
   - ʹ������ģʽ���������ʷ

4. **���ӻ��Ľ�**
   - ʹ�� UI Toolkit ��� OnGUI
   - ��Ӷ���Ч��

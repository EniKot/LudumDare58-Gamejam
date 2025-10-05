# ColorPicker ϵͳ��������ָ��

## ?? ��ɵ���д����

### ? **��д�����**

1. **ColorMagazine** - ��ϻϵͳ
   - 6���ӵ�����
   - ��ɫƥ����֤
   - �������¼�ϵͳ

2. **ColorBag** - ��ɫ������
   - 4����ϻ���졢�̡��������
   - �Զ���ɫ����
   - ��ɫ�߼�����

3. **ColorPicker** - ��ɫʰȡ��
   - ������ײ��⣨����ʹ����꣩
   - ����������E����
   - �Զ���ӵ���Ӧ��ϻ

4. **ColorMixController** - ��ɫ������
   - ���ּ���ɫ����
   - ����ϻ���
   - ��ɫ�������

## ?? �������ò���

### ��1��������ϵͳ�ṹ

```
1. ���������� "ColorSystem"
2. ��� ColorBag ����� ColorSystem
3. ����4�������壺
   - RedMagazine (��� ColorMagazine ���)
   - GreenMagazine (��� ColorMagazine ���)  
   - BlueMagazine (��� ColorMagazine ���)
   - MixMagazine (��� ColorMagazine ���)
4. �� ColorBag �з�����4����ϻ������
```

### ��2�������ʰȡ��

```
1. ����һ������������� ColorPicker ���
2. ���ü��뾶 (�Ƽ� 1.5f)
3. ���ü��㼶 LayerMask
4. ���� ColorBag ����
```

### ��3������ӻ�ɫ������

```
1. �������������������������� ColorMixController ���
2. ���� ColorBag ����
3. ���û�ɫ������Ĭ�ϣ�1,2,3,4��
```

### ��4����������ʰȡ����

```
1. ����3D���壨�� Cube��
2. ȷ���� Collider ���
3. ��� ExampleColorPickableObject ���
4. ����������ɫ
5. ������ȷ�� Layer���� ColorPicker �� LayerMask ƥ�䣩
```

## ?? ʹ�÷���

### **��������**
- **E��**: ʰȡ�����������ɫ
- **1��**: ��Ϻ�ɫ+��ɫ��ϻ
- **2��**: ��Ϻ�ɫ+��ɫ��ϻ
- **3��**: �����ɫ+��ɫ��ϻ
- **4��**: ��ջ�ϵ�ϻ

### **��ɫ����**
1. ���������ɫ�ĵ�ϻ���붼������6/6��
2. ��ϵ�ϻ����Ϊ��
3. ��ɫ�����ĵ�ϻ����գ���ϵ�ϻ������

## ?? Inspector ����

### **ColorPicker ����**
```
Detection Radius: 1.5f
Pickable Layer Mask: Default (���Զ���㼶)
Pick Color Key: E
Show Debug GUI: ? (����ʱ����)
Show Detection Range: ? (����ʱ����)
```

### **ColorBag ����**
```
Base Red Color: (255, 0, 0, 255)
Base Green Color: (0, 255, 0, 255)  
Base Blue Color: (0, 0, 255, 255)
Show Debug: ? (����ʱ����)
```

### **ColorMagazine ����**
```
Bullet Capacity: 6
Magazine Color: �Զ����û��ֶ�����
Show Debug: ? (����ʱ����)
```

## ?? ����ʾ��

### **������ʰȡ����**
```csharp
GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
var pickable = obj.AddComponent<ExampleColorPickableObject>();
pickable.SetColor(Color.red);
```

### **����ϵͳ�¼�**
```csharp
// ������ɫʰȡ
colorPicker.OnColorPicked += (color, obj) => {
    Debug.Log($"Picked {color} from {obj.name}");
};

// ������ɫ���
colorBag.OnColorsMixed += (mag1, mag2, result) => {
    Debug.Log($"Mixed {mag1.MagazineColor} + {mag2.MagazineColor} = {result}");
};
```

### **���򻯲���**
```csharp
// �ֶ�ʰȡ��ɫ
colorPicker.ManualPickColor();

// ����Ƿ���Ի�ɫ
bool canMix = colorBag.CanMix(colorBag.RedMagazine, colorBag.GreenMagazine);

// �ֶ���ɫ
if (canMix) {
    colorBag.MixRedGreen();
}

// ��ȡϵͳ״̬
string status = colorBag.GetAllMagazineInfo();
```

## ?? ���Թ���

### **���ӻ�����**
- Scene ��ͼ��ʾ��ⷶΧ����
- GUI ��ʾʵʱϵͳ״̬
- Console �����ϸ������־

### **��������**
```csharp
// �������л�����ϻ�������ã�
colorBag.RedMagazine.FillMagazine();
colorBag.GreenMagazine.FillMagazine(); 
colorBag.BlueMagazine.FillMagazine();

// ������е�ϻ
colorBag.ClearAllMagazines();

// ��ʾϵͳ״̬
Debug.Log(colorBag.GetAllMagazineInfo());
```

## ?? ��Ҫ�仯�ܽ�

### ? **�Ƴ��Ĺ���**
- �����ʰȡ
- UI Ԫ�ؼ��
- ʵʱ��ɫ
- ��ɫ��ϵͳ
- ��Ļ���ض�ȡ

### ? **�����Ĺ���**
- ��ײ���ʰȡ
- �ӵ��ռ�����
- ��ϻ����ϵͳ
- ������������
- ����ϻ��ɫ����
- �������¼�ϵͳ

### ?? **���ĸĽ�**
- ���ʺ���Ϸ�Ե����
- �����Ĳ�������
- ������״̬����
- ����չ�ļܹ�
- �ḻ�ĵ��Թ���

## ?? ��һ������

1. **���ɵ���Ϸѭ��**
   - �������ϵͳ
   - ���UI��ʾ
   - ʵ����ϷĿ��

2. **����Ӿ�Ч��**
   - ʰȡ����Ч��
   - ��ɫ����
   - UI ��������

3. **��չ����**
   - ������ɫ����
   - �����ɫ����
   - ��ϻ����ϵͳ

����������һ����ȫ��д�ġ�������ײ���� ColorPicker ϵͳ��???
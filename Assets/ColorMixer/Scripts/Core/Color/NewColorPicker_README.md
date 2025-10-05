# �� ColorPicker ϵͳ�ĵ�

## ����
ȫ����д�� ColorPicker ϵͳ��������ײ�������������רע���ӵ��ռ�����ɫ��ϻ��ơ�

## ϵͳ�ܹ�

### ?? **�������**

#### **1. ColorMagazine����ϻ��**
- **����**: 6���ӵ�
- **����**: �洢ͬɫ�ӵ�
- **����**: 
  - �Զ���ɫƥ����֤
  - ��/��״̬���
  - �¼�ϵͳ֧��

#### **2. ColorBag����ɫ����**
- **������ϻ**: �졢�̡�������������ɫ��ϻ
- **��ϵ�ϻ**: һ�����ڴ洢�����ɫ�ĵ�ϻ
- **����**: 
  - �Զ���ɫ����
  - ��ɫ����
  - ��ϻ״̬���

#### **3. ColorPicker����ɫʰȡ����**
- **��ⷽʽ**: ������ײ���
- **������ʽ**: ����������E����
- **����**: 
  - ��ⷶΧ�ڵ� IColorPickable ����
  - �Զ�ѡ������Ķ���
  - ��ʰȡ����ɫ��ӵ���Ӧ��ϻ

#### **4. ColorMixController����ɫ��������**
- **��ɫ���**: 
  - ��+�� (����1)
  - ��+�� (����2)  
  - ��+�� (����3)
- **��ɫ����**: �������뵯ϻ����������
- **��ɫ���**: ��ղ��뵯ϻ��������ϵ�ϻ

## ��������

### ?? **��ɫʰȡ����**

1. **����Ŀ��**: ����ƶ�����ʰȡ���󸽽�
2. **��ⷶΧ**: ColorPicker ��ⷶΧ�ڵ� IColorPickable ����
3. **����ʰȡ**: ��E������ʰȡ
4. **��ɫ����**: ������ɫ�Զ����䵽��Ӧ��ϻ
5. **�ӵ����**: �ڶ�Ӧ��ϻ�����һ���ӵ�

### ?? **��ɫ�������**

1. **��ϻ���**: ȷ�����������ɫ�ĵ�ϻ��������6/6��
2. **��ϵ�ϻ���**: ȷ����ϵ�ϻΪ��
3. **ִ�л�ɫ**: ����Ӧ���ּ�������ɫ
4. **��ղ��뵯ϻ**: �������ɫ�ĵ�ϻ���
5. **����ϵ�ϻ**: ����Ͻ��������ϵ�ϻ

## ʹ��ָ��

### ?? **��������**

#### **1. ��������**
```csharp
// ��������㼶�ṹ
GameObject colorSystem = new GameObject("ColorSystem");
������ ColorBag (ColorBag.cs)
��   ������ RedMagazine (ColorMagazine.cs)
��   ������ GreenMagazine (ColorMagazine.cs)
��   ������ BlueMagazine (ColorMagazine.cs)
��   ������ MixMagazine (ColorMagazine.cs)
������ ColorPicker (ColorPicker.cs)
������ ColorMixController (ColorMixController.cs)
```

#### **2. ��ʰȡ��������**
```csharp
// Ϊ���������ɫʰȡ����
GameObject colorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
colorObject.AddComponent<ExampleColorPickableObject>();
// ȷ���� Collider ���ڼ��
```

### ?? **���Ʋ���**

| ���� | ���� | ���� |
|------|------|------|
| ʰȡ��ɫ | E | �Ӹ���������ʰȡ��ɫ |
| ��Ϻ��� | 1 | ��Ϻ�ɫ����ɫ��ϻ |
| ��Ϻ��� | 2 | ��Ϻ�ɫ����ɫ��ϻ |
| ������� | 3 | �����ɫ����ɫ��ϻ |
| ��ջ�ϵ�ϻ | 4 | ��ջ�ϵ�ϻ |

### ?? **��������**

#### **ColorPicker ����**
```csharp
[Header("Detection Settings")]
public LayerMask pickableLayerMask = -1;    // ���㼶
public float detectionRadius = 1.5f;        // ���뾶
public Transform detectionCenter;           // ������ĵ�

[Header("Input Settings")]
public KeyCode pickColorKey = KeyCode.E;    // ʰȡ����
```

#### **ColorMagazine ����**
```csharp
[Header("Magazine Settings")]
public int bulletCapacity = 6;              // ��ϻ����
public Color magazineColor = Color.white;   // ��ϻ��ɫ
```

## API �ο�

### ?? **ColorMagazine ��Ҫ����**

```csharp
// ����ӵ�
bool AddBullet(Color color)

// �Ƴ��ӵ�  
bool RemoveBullet()

// ��յ�ϻ
void ClearMagazine()

// ������ϻ�������ã�
void FillMagazine()

// ���Է���
bool IsFull { get; }           // �Ƿ�����
bool IsEmpty { get; }          // �Ƿ�Ϊ��
int CurrentBullets { get; }    // ��ǰ�ӵ���
Color MagazineColor { get; }   // ��ϻ��ɫ
```

### ?? **ColorBag ��Ҫ����**

```csharp
// �����ɫ�ӵ�
bool AddColorBullet(Color color)

// ��ɫ����
bool MixColors(ColorMagazine mag1, ColorMagazine mag2)
bool MixRedGreen()
bool MixRedBlue()  
bool MixGreenBlue()

// ���߷���
bool CanMix(ColorMagazine mag1, ColorMagazine mag2)
void ClearAllMagazines()
string GetAllMagazineInfo()
```

### ?? **ColorPicker ��Ҫ����**

```csharp
// �ֶ�ʰȡ
void ManualPickColor()

// ���ò���
void SetDetectionRadius(float radius)
void SetPickableLayerMask(LayerMask layerMask)
void SetColorBag(ColorBag bag)

// ״̬��ѯ
int GetPickableObjectCount()
IColorPickable<GameObject> GetNearestPickable()
```

## �¼�ϵͳ

### ?? **�¼�����ʾ��**

```csharp
// ColorMagazine �¼�
magazine.OnBulletAdded += (mag) => Debug.Log("Bullet added");
magazine.OnMagazineFull += (mag) => Debug.Log("Magazine full");
magazine.OnMagazineEmpty += (mag) => Debug.Log("Magazine empty");

// ColorBag �¼�
colorBag.OnColorsMixed += (mag1, mag2, color) => Debug.Log("Colors mixed");
colorBag.OnMagazineUpdated += (mag) => Debug.Log("Magazine updated");

// ColorPicker �¼�
colorPicker.OnColorPicked += (color, obj) => Debug.Log("Color picked");
colorPicker.OnPickableObjectFound += (pickable) => Debug.Log("Object found");
```

## ʵ���Զ�����ɫʰȡ����

### ?? **����ʵ��**

```csharp
public class MyColorObject : MonoBehaviour, IColorPickable<GameObject>
{
    [SerializeField] private Color objectColor = Color.white;
    
    public Color GetColor()
    {
        return objectColor;
    }
    
    public void OnColorPicked(Color color, GameObject target)
    {
        Debug.Log($"My color {color} was picked!");
        
        // ���ʰȡЧ��
        StartCoroutine(PickupEffect());
    }
    
    private IEnumerator PickupEffect()
    {
        // ʵ��ʰȡЧ���������š���˸��
        yield return null;
    }
}
```

### ?? **�߼�ʵ��**

```csharp
public class AdvancedColorObject : MonoBehaviour, IColorPickable<GameObject>
{
    [SerializeField] private Color[] availableColors;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private ParticleSystem pickupEffect;
    
    private int currentColorIndex = 0;
    
    public Color GetColor()
    {
        return availableColors[currentColorIndex];
    }
    
    public void OnColorPicked(Color color, GameObject target)
    {
        // ������Ч
        AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        
        // ��������Ч��
        if (pickupEffect != null)
        {
            pickupEffect.Play();
        }
        
        // �л�����һ����ɫ
        currentColorIndex = (currentColorIndex + 1) % availableColors.Length;
        
        // �����Ӿ�����
        UpdateVisual();
    }
    
    private void UpdateVisual()
    {
        GetComponent<Renderer>().material.color = GetColor();
    }
}
```

## ���ԺͿ��ӻ�

### ?? **���Թ���**

#### **1. ��ⷶΧ���ӻ�**
- Scene ��ͼ����ʾ�������
- ��ɫ��ʾ��⵽����
- ��ɫ��ʾû�м�⵽����
- �������ӵ�����Ķ���

#### **2. GUI ������Ϣ**
- ʵʱ��ʾ���״̬
- ��ϻ״̬���
- ��ʰȡ�����б�
- ������ʾ

#### **3. Console ��־**
- ��ϸ�Ĳ�����־
- ����;�����Ϣ
- �¼�������¼

### ??? **���Թ���**

```csharp
// �� Inspector �����õ���
[Header("Debug")]
public bool showDebugGUI = true;
public bool showDetectionRange = true;
public bool showDebug = true;

// ����ʱ��������
public void DebugFillAllMagazines()
{
    colorBag.RedMagazine.FillMagazine();
    colorBag.GreenMagazine.FillMagazine(); 
    colorBag.BlueMagazine.FillMagazine();
}

public void DebugClearAllMagazines()
{
    colorBag.ClearAllMagazines();
}

public void DebugShowSystemStatus()
{
    Debug.Log(colorBag.GetAllMagazineInfo());
}
```

## �����Ż�

### ? **�Ż�����**

1. **���Ƶ���Ż�**
   - ʹ�� FixedUpdate �򽵵ͼ��Ƶ��
   - ֻ������ƶ�ʱ���м��

2. **�㼶����**
   - ��ȷ���� LayerMask
   - �����ⲻ��Ҫ�Ķ���

3. **�����**
   - ��Ƶ��������Ч��ʹ�ö����
   - ��������Ч������Ч����

4. **�¼��Ż�**
   - ��ʱȡ�����ı����ڴ�й©
   - �������¼��н������Ͳ���

## ��չ����

### ?? **������չ**

1. **�����ɫ���**
   - ��ɫ���
   - ���������
   - �����Ϲ���

2. **��ϻ����ϵͳ**
   - ���ӵ�ϻ����
   - ���ⵯϻ����
   - ��ϻ����

3. **UI ϵͳ����**
   - ���ӻ���ϻ״̬
   - ʰȡ������
   - ��ɫԤ��

4. **��Ч����Ч**
   - ʰȡ��Ч
   - ��ɫ��Ч
   - UI ����

### ?? **��Ϸ������չ**

1. **���ϵͳ**
   - ʹ���ӵ��������
   - ��ͬ��ɫ���ӵ�Ч��
   - �������˺�ϵͳ

2. **��ɫ����**
   - ��Ҫ�ض���ɫ�Ļ���
   - ��ɫƥ����Ϸ
   - ������ɫ��ս

3. **��Դ����**
   - ϡ����ɫϵͳ
   - ��ɫ����
   - ��ɫ�ռ��ɾ�

## �����ų�

### ? **��������**

#### **Q1: ��ⲻ������**
**A**: �����������
- �����Ƿ��� Collider ���
- LayerMask �����Ƿ���ȷ
- ���뾶�Ƿ��㹻��
- �����Ƿ�ʵ���� IColorPickable �ӿ�

#### **Q2: �޷���ɫ��**
**A**: ȷ�ϻ�ɫ����
- ������ϻ������������6/6��
- ��ϵ�ϻ����Ϊ��
- ColorBag ����������ȷ

#### **Q3: ��ɫ�������**
**A**: ������ɫƥ����ֵ
```csharp
// �� ColorBag.IsColorSimilar() �е���
private bool IsColorSimilar(Color color1, Color color2, float threshold = 0.2f)
```

#### **Q4: �������⣿**
**A**: �Ż��������
- ��С���뾶
- ʹ�ø���ȷ�� LayerMask
- ���ͼ��Ƶ��

## �ܽ�

? **��ϵͳ�ص�**:
- ������ײ��⣬����������
- �ӵ��ռ����ƣ�ÿ����ɫ���6��
- �ĸ���ϻϵͳ���졢�̡��������
- ����ϻ��ɫ����
- �������¼�ϵͳ
- ���ӻ����Թ���

? **���ó���**:
- ��һ�˳�/�����˳������Ϸ
- ƽ̨��Ծ��Ϸ
- ������Ϸ
- ��ɫ�ռ�����Ϸ

����������һ�����������Ļ�����ײ���� ColorPicker ϵͳ��????
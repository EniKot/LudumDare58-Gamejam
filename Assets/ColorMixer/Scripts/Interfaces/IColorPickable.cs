using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IColorPickable
{
    public void OnColorPicked();
    public Color GetColor();
}

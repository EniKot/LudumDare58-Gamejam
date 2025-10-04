using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IColorPickable<Target>
{
    void OnColorPicked(Color color, Target target);
    Color GetColor();
}

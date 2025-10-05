using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IColorDyeable<Target> { 
    void OnColorDye(Target target, Color color);
}

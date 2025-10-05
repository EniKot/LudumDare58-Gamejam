using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputService : SingletonMono<InputService>
{
    public InputActionsMap inputMap;
    //public Vector2 inputDirectionHorizen;
    //public Vector2 inputDirectionVertical;
    protected override void Awake()
    {
        base.Awake();
        if (inputMap == null)
        {
            inputMap = new InputActionsMap();
        }

        inputMap.Enable();
    }
  
    public Vector2 GetMoveHorizontalValue
    {
        get
        {
            Vector2 dir = inputMap.Player.Move.ReadValue<Vector2>();
            

            if (dir != Vector2.zero)
            {
                dir.y = 0;
                return dir.normalized;
            }
            else
            {
                return Vector2.zero;
            }
        }
    }

    public Vector2 GetMoveVerticalValue
    {
        get
        {
            Vector2 dir = inputMap.Player.Move.ReadValue<Vector2>();

            if (dir != Vector2.zero)
            {
                dir.x = 0;
                return dir.normalized;
            }
            else
            {
                return Vector2.zero;
            }
        }
    }
    public bool Shoot
    {
        get
        {
            return inputMap.Player.Fire.triggered;
        }
    }
    public bool Jump
    {
        get
        {
            return inputMap.Player.Jump.triggered;
        }
    }
    public bool Interact
    {
        get
        {
            return inputMap.Player.Interact.triggered;
        }
    }



    public Vector2 Move
    {
        get
        {
            Vector2 vector2 = inputMap.Player.Move.ReadValue<Vector2>();
            if (vector2.x > 0)
            {
                vector2.x = 1;
            }
            else if (vector2.x < 0)
            {
                vector2.x = -1;
            }
            else
            {
                vector2.x = 0;
            }
            if (vector2.y > 0)
            {
                vector2.y = 1;
            }
            else if (vector2.y < 0)
            {
                vector2.y = -1;
            }
            else
            {
                vector2.y = 0;
            }
            return vector2;
        }
    }
    private void OnDestroy()
    {
        inputMap.Disable();
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputController : Controller
{
    public void Axis(InputAction.CallbackContext ctx)
    {
        onAxis.Invoke(ctx.ReadValue<Vector2>());
    }

    public void RunInput(InputAction.CallbackContext ctx)
    {
        onRunStart.Invoke();
    }

    public void JumpInput(InputAction.CallbackContext ctx)
    {
        onJump.Invoke(ctx.performed);
    }

    public void CrouchInput(InputAction.CallbackContext ctx)
    {
        onCrouch.Invoke(ctx.performed);
    }

    public void MouseInput(InputAction.CallbackContext ctx)
    {
        onMouseAxis.Invoke(ctx.ReadValue<Vector2>());
    }
}
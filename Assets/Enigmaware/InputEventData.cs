
using UnityEngine.InputSystem;

/// <summary>
/// Generates data from new input system events.
/// </summary>
public class InputEventData
{
    public bool down { get; protected set; }
    public bool pressed { get; protected set; }
    public bool up { get; protected set; }
    
    public void RefreshKeyData()
    {
        down = false;
        up = false;
    }

    public void UpdateKeyState(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            up = false;
            down = true;
            pressed = true;
        }
        else if (context.canceled)
        {
            up = true;
            down = false;
            pressed = false;
        }
    }
}
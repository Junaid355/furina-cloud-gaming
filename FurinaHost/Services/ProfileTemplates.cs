// d:\cloud\FurinaHost\Services\ProfileTemplates.cs
using FurinaHost.Models;

namespace FurinaHost.Services;

public static class ProfileTemplates
{
    public static List<TouchControlElement> GetRobloxControls()
    {
        return new List<TouchControlElement>
        {
            // Left Joystick (Movement)
            new() { Id = "joy_move", Type = "joystick", Label = "MOVE", X = 0.14, Y = 0.72, Width = 0.18, Height = 0.32, PrimaryKey = "WASD", ColorScheme = "cyan" },
            
            // Right Camera Drag Region
            new() { Id = "cam_look", Type = "camera_region", Label = "CAMERA SWIPE", X = 0.50, Y = 0.15, Width = 0.48, Height = 0.75, PrimaryKey = "MouseLook", Sensitivity = 1.2, ColorScheme = "silver" },
            
            // Action Buttons
            new() { Id = "btn_jump", Type = "button", Label = "JUMP", SubLabel = "Space", X = 0.88, Y = 0.75, Width = 0.09, Height = 0.16, PrimaryKey = "Space", ColorScheme = "cyan" },
            new() { Id = "btn_sprint", Type = "button", Label = "SPRINT", SubLabel = "Shift", X = 0.88, Y = 0.54, Width = 0.08, Height = 0.14, PrimaryKey = "ShiftLeft", ActionMode = "hold", ColorScheme = "blue" },
            new() { Id = "btn_interact", Type = "button", Label = "INTERACT", SubLabel = "E", X = 0.78, Y = 0.68, Width = 0.08, Height = 0.14, PrimaryKey = "KeyE", ColorScheme = "violet" },
            new() { Id = "btn_action", Type = "button", Label = "USE / CLICK", SubLabel = "LMB", X = 0.78, Y = 0.48, Width = 0.08, Height = 0.14, PrimaryKey = "MouseLeft", ColorScheme = "cyan" },
            new() { Id = "btn_mouse_lock", Type = "toggle", Label = "MOUSE LOCK", SubLabel = "Shift", X = 0.88, Y = 0.12, Width = 0.09, Height = 0.10, PrimaryKey = "ShiftRight", ColorScheme = "silver" },
            new() { Id = "btn_chat", Type = "button", Label = "CHAT", SubLabel = "/", X = 0.12, Y = 0.12, Width = 0.08, Height = 0.10, PrimaryKey = "Slash", ColorScheme = "silver" }
        };
    }

    public static List<TouchControlElement> GetGtaVControls()
    {
        return new List<TouchControlElement>
        {
            // Left Joystick (Movement / Steer)
            new() { Id = "joy_move", Type = "joystick", Label = "MOVE / STEER", X = 0.14, Y = 0.72, Width = 0.18, Height = 0.32, PrimaryKey = "WASD", ColorScheme = "cyan" },
            
            // Right Camera Aim
            new() { Id = "cam_look", Type = "camera_region", Label = "LOOK / AIM", X = 0.48, Y = 0.15, Width = 0.50, Height = 0.70, PrimaryKey = "MouseLook", Sensitivity = 1.0, ColorScheme = "silver" },
            
            // On Foot Controls
            new() { Id = "btn_sprint", Type = "button", Label = "SPRINT", SubLabel = "Shift", X = 0.88, Y = 0.74, Width = 0.09, Height = 0.16, PrimaryKey = "ShiftLeft", ActionMode = "hold", ColorScheme = "cyan" },
            new() { Id = "btn_jump", Type = "button", Label = "JUMP / CLIMB", SubLabel = "Space", X = 0.88, Y = 0.53, Width = 0.08, Height = 0.14, PrimaryKey = "Space", ColorScheme = "blue" },
            new() { Id = "btn_vehicle", Type = "button", Label = "VEHICLE ENTER", SubLabel = "F", X = 0.77, Y = 0.74, Width = 0.08, Height = 0.14, PrimaryKey = "KeyF", ColorScheme = "violet" },
            new() { Id = "btn_aim", Type = "button", Label = "AIM", SubLabel = "RMB", X = 0.77, Y = 0.53, Width = 0.08, Height = 0.14, PrimaryKey = "MouseRight", ActionMode = "hold", ColorScheme = "silver" },
            new() { Id = "btn_shoot", Type = "button", Label = "FIRE", SubLabel = "LMB", X = 0.88, Y = 0.32, Width = 0.09, Height = 0.15, PrimaryKey = "MouseLeft", ColorScheme = "cyan" },
            new() { Id = "btn_reload", Type = "button", Label = "RELOAD", SubLabel = "R", X = 0.77, Y = 0.32, Width = 0.07, Height = 0.12, PrimaryKey = "KeyR", ColorScheme = "silver" },
            new() { Id = "btn_cover", Type = "button", Label = "COVER", SubLabel = "Q", X = 0.67, Y = 0.74, Width = 0.07, Height = 0.12, PrimaryKey = "KeyQ", ColorScheme = "blue" },
            new() { Id = "btn_handbrake", Type = "button", Label = "HANDBRAKE", SubLabel = "Space", X = 0.67, Y = 0.53, Width = 0.07, Height = 0.12, PrimaryKey = "Space", ColorScheme = "violet" }
        };
    }

    public static List<TouchControlElement> GetGenshinControls()
    {
        return new List<TouchControlElement>
        {
            // Left Movement Joystick
            new() { Id = "joy_move", Type = "joystick", Label = "MOVE", X = 0.14, Y = 0.72, Width = 0.18, Height = 0.32, PrimaryKey = "WASD", ColorScheme = "cyan" },
            
            // Right Camera Pan Area
            new() { Id = "cam_look", Type = "camera_region", Label = "CAMERA", X = 0.45, Y = 0.15, Width = 0.52, Height = 0.65, PrimaryKey = "MouseLook", Sensitivity = 1.1, ColorScheme = "silver" },
            
            // Normal Attack (Big Right Button)
            new() { Id = "btn_attack", Type = "button", Label = "ATTACK", SubLabel = "LMB", X = 0.86, Y = 0.74, Width = 0.11, Height = 0.19, PrimaryKey = "MouseLeft", ColorScheme = "cyan" },
            
            // Elemental Skill (E)
            new() { Id = "btn_skill", Type = "button", Label = "SKILL", SubLabel = "E", X = 0.73, Y = 0.78, Width = 0.08, Height = 0.15, PrimaryKey = "KeyE", ColorScheme = "blue" },
            
            // Elemental Burst (Q)
            new() { Id = "btn_burst", Type = "button", Label = "BURST", SubLabel = "Q", X = 0.73, Y = 0.58, Width = 0.08, Height = 0.15, PrimaryKey = "KeyQ", ColorScheme = "violet" },
            
            // Jump (Space)
            new() { Id = "btn_jump", Type = "button", Label = "JUMP", SubLabel = "Space", X = 0.87, Y = 0.49, Width = 0.08, Height = 0.14, PrimaryKey = "Space", ColorScheme = "silver" },
            
            // Sprint / Dash (RMB / Shift)
            new() { Id = "btn_dash", Type = "button", Label = "DASH", SubLabel = "RMB", X = 0.87, Y = 0.28, Width = 0.08, Height = 0.14, PrimaryKey = "MouseRight", ColorScheme = "cyan" },
            
            // Character Switching (1, 2, 3, 4)
            new() { Id = "btn_char1", Type = "button", Label = "1", SubLabel = "Char 1", X = 0.94, Y = 0.15, Width = 0.05, Height = 0.09, PrimaryKey = "Digit1", ColorScheme = "cyan" },
            new() { Id = "btn_char2", Type = "button", Label = "2", SubLabel = "Char 2", X = 0.94, Y = 0.26, Width = 0.05, Height = 0.09, PrimaryKey = "Digit2", ColorScheme = "cyan" },
            new() { Id = "btn_char3", Type = "button", Label = "3", SubLabel = "Char 3", X = 0.94, Y = 0.37, Width = 0.05, Height = 0.09, PrimaryKey = "Digit3", ColorScheme = "cyan" },
            new() { Id = "btn_char4", Type = "button", Label = "4", SubLabel = "Char 4", X = 0.94, Y = 0.48, Width = 0.05, Height = 0.09, PrimaryKey = "Digit4", ColorScheme = "cyan" },
            
            // Pickup / Interact (F)
            new() { Id = "btn_interact", Type = "button", Label = "PICKUP", SubLabel = "F", X = 0.62, Y = 0.72, Width = 0.07, Height = 0.13, PrimaryKey = "KeyF", ColorScheme = "silver" }
        };
    }

    public static List<TouchControlElement> GetWuWaControls()
    {
        return new List<TouchControlElement>
        {
            // Left Movement Joystick
            new() { Id = "joy_move", Type = "joystick", Label = "MOVE", X = 0.14, Y = 0.72, Width = 0.18, Height = 0.32, PrimaryKey = "WASD", ColorScheme = "cyan" },
            
            // Right Camera Pan Area
            new() { Id = "cam_look", Type = "camera_region", Label = "CAMERA", X = 0.45, Y = 0.15, Width = 0.52, Height = 0.65, PrimaryKey = "MouseLook", Sensitivity = 1.15, ColorScheme = "silver" },
            
            // Normal Attack
            new() { Id = "btn_attack", Type = "button", Label = "ATTACK", SubLabel = "LMB", X = 0.86, Y = 0.74, Width = 0.11, Height = 0.19, PrimaryKey = "MouseLeft", ColorScheme = "cyan" },
            
            // Resonance Skill (E)
            new() { Id = "btn_skill", Type = "button", Label = "SKILL", SubLabel = "E", X = 0.73, Y = 0.78, Width = 0.08, Height = 0.15, PrimaryKey = "KeyE", ColorScheme = "blue" },
            
            // Resonance Liberation (R)
            new() { Id = "btn_ult", Type = "button", Label = "LIBERATION", SubLabel = "R", X = 0.73, Y = 0.58, Width = 0.08, Height = 0.15, PrimaryKey = "KeyR", ColorScheme = "violet" },
            
            // Echo Skill (Q)
            new() { Id = "btn_echo", Type = "button", Label = "ECHO", SubLabel = "Q", X = 0.62, Y = 0.68, Width = 0.08, Height = 0.15, PrimaryKey = "KeyQ", ColorScheme = "silver" },
            
            // Dodge / Dash (Shift / RMB)
            new() { Id = "btn_dodge", Type = "button", Label = "DODGE", SubLabel = "Shift", X = 0.87, Y = 0.50, Width = 0.08, Height = 0.14, PrimaryKey = "ShiftLeft", ColorScheme = "cyan" },
            
            // Jump (Space)
            new() { Id = "btn_jump", Type = "button", Label = "JUMP", SubLabel = "Space", X = 0.87, Y = 0.29, Width = 0.08, Height = 0.14, PrimaryKey = "Space", ColorScheme = "blue" },
            
            // Character Switching (1, 2, 3)
            new() { Id = "btn_char1", Type = "button", Label = "1", SubLabel = "C1", X = 0.94, Y = 0.15, Width = 0.05, Height = 0.09, PrimaryKey = "Digit1", ColorScheme = "cyan" },
            new() { Id = "btn_char2", Type = "button", Label = "2", SubLabel = "C2", X = 0.94, Y = 0.27, Width = 0.05, Height = 0.09, PrimaryKey = "Digit2", ColorScheme = "cyan" },
            new() { Id = "btn_char3", Type = "button", Label = "3", SubLabel = "C3", X = 0.94, Y = 0.39, Width = 0.05, Height = 0.09, PrimaryKey = "Digit3", ColorScheme = "cyan" },
            
            // Interact (F)
            new() { Id = "btn_interact", Type = "button", Label = "INTERACT", SubLabel = "F", X = 0.62, Y = 0.84, Width = 0.07, Height = 0.12, PrimaryKey = "KeyF", ColorScheme = "silver" }
        };
    }

    public static List<TouchControlElement> GetMinecraftControls()
    {
        return new List<TouchControlElement>
        {
            new() { Id = "joy_move", Type = "joystick", Label = "MOVE", X = 0.14, Y = 0.72, Width = 0.18, Height = 0.32, PrimaryKey = "WASD", ColorScheme = "cyan" },
            new() { Id = "cam_look", Type = "camera_region", Label = "LOOK", X = 0.45, Y = 0.15, Width = 0.52, Height = 0.70, PrimaryKey = "MouseLook", Sensitivity = 1.0, ColorScheme = "silver" },
            new() { Id = "btn_mine", Type = "button", Label = "MINE / ATTACK", SubLabel = "LMB", X = 0.87, Y = 0.74, Width = 0.09, Height = 0.16, PrimaryKey = "MouseLeft", ActionMode = "hold", ColorScheme = "cyan" },
            new() { Id = "btn_place", Type = "button", Label = "PLACE / USE", SubLabel = "RMB", X = 0.76, Y = 0.74, Width = 0.08, Height = 0.14, PrimaryKey = "MouseRight", ColorScheme = "blue" },
            new() { Id = "btn_jump", Type = "button", Label = "JUMP", SubLabel = "Space", X = 0.87, Y = 0.53, Width = 0.08, Height = 0.14, PrimaryKey = "Space", ColorScheme = "silver" },
            new() { Id = "btn_sneak", Type = "button", Label = "SNEAK", SubLabel = "Shift", X = 0.76, Y = 0.53, Width = 0.08, Height = 0.14, PrimaryKey = "ShiftLeft", ActionMode = "toggle", ColorScheme = "violet" },
            new() { Id = "btn_inv", Type = "button", Label = "INVENTORY", SubLabel = "E", X = 0.87, Y = 0.32, Width = 0.08, Height = 0.13, PrimaryKey = "KeyE", ColorScheme = "silver" }
        };
    }

    public static List<TouchControlElement> GetHsrControls()
    {
        return new List<TouchControlElement>
        {
            new() { Id = "joy_move", Type = "joystick", Label = "MOVE", X = 0.14, Y = 0.72, Width = 0.18, Height = 0.32, PrimaryKey = "WASD", ColorScheme = "cyan" },
            new() { Id = "cam_look", Type = "camera_region", Label = "ROTATE", X = 0.45, Y = 0.15, Width = 0.52, Height = 0.70, PrimaryKey = "MouseLook", Sensitivity = 1.0, ColorScheme = "silver" },
            new() { Id = "btn_attack", Type = "button", Label = "ATTACK", SubLabel = "LMB", X = 0.87, Y = 0.74, Width = 0.10, Height = 0.18, PrimaryKey = "MouseLeft", ColorScheme = "cyan" },
            new() { Id = "btn_technique", Type = "button", Label = "TECHNIQUE", SubLabel = "E", X = 0.75, Y = 0.74, Width = 0.08, Height = 0.15, PrimaryKey = "KeyE", ColorScheme = "violet" },
            new() { Id = "btn_sprint", Type = "button", Label = "DASH", SubLabel = "Shift", X = 0.87, Y = 0.52, Width = 0.08, Height = 0.14, PrimaryKey = "ShiftLeft", ColorScheme = "blue" },
            new() { Id = "btn_interact", Type = "button", Label = "INTERACT", SubLabel = "F", X = 0.75, Y = 0.52, Width = 0.08, Height = 0.14, PrimaryKey = "KeyF", ColorScheme = "silver" }
        };
    }

    public static List<TouchControlElement> GetZzzControls()
    {
        return new List<TouchControlElement>
        {
            new() { Id = "joy_move", Type = "joystick", Label = "MOVE", X = 0.14, Y = 0.72, Width = 0.18, Height = 0.32, PrimaryKey = "WASD", ColorScheme = "cyan" },
            new() { Id = "cam_look", Type = "camera_region", Label = "CAMERA", X = 0.45, Y = 0.15, Width = 0.52, Height = 0.65, PrimaryKey = "MouseLook", Sensitivity = 1.1, ColorScheme = "silver" },
            new() { Id = "btn_attack", Type = "button", Label = "BASIC ATK", SubLabel = "LMB", X = 0.86, Y = 0.74, Width = 0.11, Height = 0.19, PrimaryKey = "MouseLeft", ColorScheme = "cyan" },
            new() { Id = "btn_special", Type = "button", Label = "SPECIAL", SubLabel = "E", X = 0.73, Y = 0.76, Width = 0.08, Height = 0.15, PrimaryKey = "KeyE", ColorScheme = "blue" },
            new() { Id = "btn_ult", Type = "button", Label = "ULTIMATE", SubLabel = "Q", X = 0.73, Y = 0.57, Width = 0.08, Height = 0.15, PrimaryKey = "KeyQ", ColorScheme = "violet" },
            new() { Id = "btn_dodge", Type = "button", Label = "DODGE", SubLabel = "RMB", X = 0.87, Y = 0.50, Width = 0.08, Height = 0.14, PrimaryKey = "MouseRight", ColorScheme = "cyan" },
            new() { Id = "btn_switch_prev", Type = "button", Label = "SWITCH <", SubLabel = "Space", X = 0.87, Y = 0.30, Width = 0.08, Height = 0.13, PrimaryKey = "Space", ColorScheme = "silver" },
            new() { Id = "btn_switch_next", Type = "button", Label = "SWITCH >", SubLabel = "C", X = 0.75, Y = 0.38, Width = 0.07, Height = 0.12, PrimaryKey = "KeyC", ColorScheme = "silver" }
        };
    }
}

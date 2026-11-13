using UnityEngine;

public static class Input_Extensions {


    public static bool down(this Event input_event, KeyCode key, bool use_event = true) {
        if (input_event.is_input_event() == false) {
            return false;
        }

        if (input_event.isKey && input_event.type == EventType.KeyDown && input_event.keyCode == key) {
            if (use_event) input_event.Use();
            return true;
        }
        
        return false;
    }
    
    public static bool up(this Event input_event, KeyCode key, bool use_event = true) {
        if (input_event.is_input_event() == false) {
            return false;
        }

        if (input_event.isKey && input_event.type == EventType.KeyUp && input_event.keyCode == key) {
            if (use_event) input_event.Use();
            return true;
        }
        
        return false;
    }
    
    public static bool held(this Event input_event, KeyCode key) {
        if (input_event.is_input_event() == false) {
            return false;
        }

        if (input_event.isKey && input_event.keyCode == key) {
            return true;
        }
        
        return false;
    }
    
    public static bool mouse_down(this Event input_event, KeyCode key, bool use_event = true) {
        if (input_event.is_input_event() == false) {
            return false;
        }

        if (input_event.isMouse && input_event.type == EventType.MouseDown && input_event.keyCode == key) {
            if (use_event) input_event.Use();
            return true;
        }
        
        return false;
    }
    
    public static bool mouse_up(this Event input_event, KeyCode key, bool use_event = true) {
        if (input_event.is_input_event() == false) {
            return false;
        }
        
        if (input_event.isMouse && input_event.type == EventType.MouseUp && input_event.keyCode == key) {
            if (use_event) input_event.Use();
            return true;
        }
        
        return false;
    }
    
        
    public static bool mouse_enter_window(this Event input_event, bool use_event = true) {
        if (input_event.is_input_event() == false) {
            return false;
        }
        
        if (input_event.isMouse && input_event.type == EventType.MouseEnterWindow) {
            if (use_event) input_event.Use();
            return true;
        }
        
        return false;
    }    
    
    public static bool mouse_leave_window(this Event input_event, bool use_event = true) {
        if (input_event.is_input_event() == false) {
            return false;
        }
        
        if (input_event.isMouse && input_event.type == EventType.MouseLeaveWindow) {
            if (use_event) input_event.Use();
            return true;
        }
        
        return false;
    }

    public static bool is_input_event(this Event input_event) {
        return input_event.type != EventType.Layout && input_event.type != EventType.Repaint;
    }

}
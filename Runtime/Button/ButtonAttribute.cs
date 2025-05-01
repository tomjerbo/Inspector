using System;

namespace Jerbo.Inspector
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute
    {
        public string displayText = "";
        public ButtonSize buttonSize = ButtonSize.MEDIUM;

        public ButtonAttribute(string displayText = "", ButtonSize buttonSize = ButtonSize.MEDIUM)
        {
            this.displayText = displayText;
            this.buttonSize = buttonSize;
        }
        
        public ButtonAttribute(ButtonSize buttonSize)
        {
            displayText = "";
            this.buttonSize = buttonSize;
        }
        
        
        public enum ButtonSize
        {
            SMALL = 20,
            MEDIUM = 40,
            LARGE = 80,
        }
    }
}
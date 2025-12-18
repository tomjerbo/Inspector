using UnityEngine;

namespace Jerbo.Inspector
{
    public class ShowIf : PropertyAttribute {
        public string argument;
        
        public ShowIf(string _argument) {
            argument = _argument;
        }
    }
}

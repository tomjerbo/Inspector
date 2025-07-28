using UnityEngine;

namespace Jerbo.Inspector
{
    public class NoteAttribute : PropertyAttribute {
        public string noteText;
        public NoteType noteType;

        public enum NoteType : byte{
            Info,
            Warning,
            Error,
        }

        public NoteAttribute(string text) {
            noteText = text;
            noteType = NoteType.Info;
        }
        
        public NoteAttribute(string text, NoteType type) {
            noteText = text;
            noteType = type;
        }
    }
}

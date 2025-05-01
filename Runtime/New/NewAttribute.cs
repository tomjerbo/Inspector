using System;
using UnityEngine;

namespace Jerbo.Inspector
{
    /// <summary>
    /// Only works on classes with ScriptableObject in it's inheritance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NewAttribute : PropertyAttribute
    {
        public float buttonWidthPercentage;
        /// <summary>
        /// Default layout of total inspector width is like this:
        /// 50%      50%
        /// [Label] [ObjectField]
        /// <paramref name="buttonWidthPercentage"/>> Is within the ObjectField portion.
        /// </summary>
        /// <param name="buttonWidthPercentage">
        /// How many percent of ObjectField width for the button to take up.
        /// Range between 0.0f to 1.0f, Default is 0.3f (30%)
        /// </param>
        public NewAttribute(float buttonWidthPercentage = 0.3f)
        {
            this.buttonWidthPercentage = Mathf.Clamp01(buttonWidthPercentage);
        }
    }
}

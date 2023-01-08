using System;

namespace References
{
    /// <summary>
    /// Attribute which allows to draw string fields as asset references.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ReferenceAttribute : Attribute { }
}
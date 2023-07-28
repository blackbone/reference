using System;

namespace References
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NoDirectAttribute : Attribute { }
}
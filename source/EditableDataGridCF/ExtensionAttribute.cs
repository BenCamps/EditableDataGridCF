#if NetCF_20
namespace System.Runtime.CompilerServices
{
    using System;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    internal sealed class ExtensionAttribute : Attribute
    {
    }
}
#endif

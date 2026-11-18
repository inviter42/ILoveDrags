using System;
using System.Reflection;
using UnityEngine;

namespace ILoveDrags;

internal static class ReflectionUtils
{
    internal static Type FindType(string assemblyName, string simpleName) {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
            if (asm.GetName().Name != assemblyName)
                continue;

            Type[] types;
            try {
                types = asm.GetTypes();
            } catch (ReflectionTypeLoadException e) {
                types = e.Types;
            }

            foreach (var t in types) {
                if (t == null || t.Name != simpleName) continue;
                Debug.Log($"[ILoveDrags]: {t.FullName} in {asm.GetName().Name}");
                return t;
            }
        }

        return null;
    }

    internal static T GetFieldValue<T>(object obj, FieldInfo fieldInfo) {
        if (obj == null)
            return default;

        if (fieldInfo == null)
            return default;

        return (T)fieldInfo.GetValue(obj);
    }

    internal static object GetMethodInvocationResult(object obj, MethodInfo methodInfo, params object[] args) {
        return obj == null || methodInfo == null
            ? null
            : methodInfo.Invoke(obj, args);
    }
}

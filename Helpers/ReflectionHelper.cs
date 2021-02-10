﻿using System;
using System.Reflection;

namespace MoreOrLessDense.Helpers {
    public static class ReflectionHelper {
        public static object GetField(object target, string fieldName, BindingFlags bindingFlags) {
            Type type = target.GetType();
            FieldInfo fi = type.GetField(fieldName, bindingFlags);
            object result = fi.GetValue(target);
            return result;
        }
        public static void SetField<T>(object target, string fieldName, T value, BindingFlags bindingFlags) {
            Type type = target.GetType();
            FieldInfo fi = type.GetField(fieldName, bindingFlags);
            fi.SetValue(target, value);
        }
    }
}

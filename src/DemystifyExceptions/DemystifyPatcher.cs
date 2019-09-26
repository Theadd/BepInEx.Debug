﻿using System;
using System.Collections.Generic;
using System.Reflection;
using DemystifyExceptions.Demystify;
using Mono.Cecil;
using MonoMod.RuntimeDetour;

namespace DemystifyExceptions
{
    public static class DemystifyPatcher
    {
        private static Detour exceptionToStringHook, exceptionStackTraceHook;

        private static readonly FieldInfo stackTrace =
            typeof(Exception).GetField("stack_trace", BindingFlags.Instance | BindingFlags.NonPublic);

        public static IEnumerable<string> TargetDLLs { get; } = new[] {"UnityEngine.dll"};


        public static void Initialize()
        {
            exceptionToStringHook = new Detour(typeof(Exception).GetMethod(nameof(Exception.ToString)),
                typeof(DemystifyPatcher).GetMethod(nameof(ExceptionToStringHook)));
            exceptionStackTraceHook =
                new Detour(typeof(Exception).GetProperty(nameof(Exception.StackTrace)).GetGetMethod(),
                    typeof(DemystifyPatcher).GetMethod(nameof(ExceptionGetStackTrace)));
            exceptionToStringHook.Apply();
            exceptionStackTraceHook.Apply();
        }

        public static string ExceptionGetStackTrace(Exception self)
        {
            if (stackTrace?.GetValue(self) is string st)
                return st;

            var exStackTrace = new EnhancedStackTrace(self);
            var stTemp = exStackTrace.ToString();

            stackTrace?.SetValue(self, stTemp);
            return stTemp;
        }

        public static string ExceptionToStringHook(Exception self)
        {
            return self.ToStringDemystified();
        }

        public static void Patch(AssemblyDefinition ad)
        {
        }
    }
}
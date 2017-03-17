using System;
using System.Reflection;

namespace iCarus.Log
{
    public static class LogRegistry
    {
        public static void Initialize()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (null != type.BaseType &&
                        type.BaseType.IsGenericType &&
                        type.BaseType.GetGenericTypeDefinition() == typeof(Logging.Define<>))
                    {
                        Register(type);
                    }
                }
            }
        }

        static void Register<T>() where T : Logging.Define<T>
        {
            Register(typeof(T));
        }

        static void Register(Type type)
        {
            string name = type.Name;
            if (name.EndsWith("Log"))
                name = name.Remove(name.Length - 3, 3);

            log4net.ILog log = log4net.LogManager.GetLogger(name);
            type.InvokeMember(
                "log",
                BindingFlags.FlattenHierarchy | BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                null, 
                new object[] { log });
        }
    }
}
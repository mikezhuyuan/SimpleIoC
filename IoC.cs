using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SimpleIoC
{
    public static class IoC
    {
        /// <summary>
        /// Register an instance
        /// </summary>
        public static void Register<T>(T instance) where T : class
        {
            _loaders[typeof(T)] = () => instance;
        }

        /// <summary>
        /// Register a callback to create instance
        /// </summary>
        public static void Register<T>(Func<T> loader) where T : class
        {
            _loaders[typeof(T)] = loader;
        }

        /// <summary>
        /// Resolve an interface
        /// </summary>
        public static T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// Create a concrete class, only call ctor with all interface args, recursively resolve args
        /// </summary>        
        public static T Create<T>() where T : class
        {
            var type = typeof(T);

            if (!type.IsClass)
                throw new ArgumentException(type + " is not class");

            if (type.IsAbstract)
                throw new ArgumentException(type + " is abstract");

            Func<object> creator = null;

            if (!_creators.TryGetValue(type, out creator))
            {
                _creators[type] = creator = GetCreator(type, creator);
            }

            return (T)creator();
        }

        /// <summary>
        /// Clear all registries
        /// </summary>
        public static void ClearAll()
        {
            _loaders.Clear();
        }

        #region Non-Public
        private readonly static Dictionary<Type, Func<object>> _loaders = new Dictionary<Type, Func<object>>();
        private readonly static Dictionary<Type, Func<object>> _creators = new Dictionary<Type, Func<object>>();

        private static object Resolve(Type type)
        {
            if (!type.IsInterface)
                throw new ArgumentException(type + " is not interface");

            Func<object> loader;

            if (_loaders.TryGetValue(type, out loader))
            {
                return loader();
            }

            throw new ArgumentException("Unable to resolve type " + type);
        }

        private static Func<object> GetCreator(Type type, Func<object> creator)
        {
            var ctors = type.GetConstructors();
            foreach (var ctor in ctors)
            {
                bool allInterfaces = true;
                foreach (var arg in ctor.GetParameters())
                {
                    if (!arg.ParameterType.IsInterface)
                    {
                        allInterfaces = false;
                        break;
                    }
                }

                if (allInterfaces)
                {
                    var method = new DynamicMethod("Create" + type.Name, type, Type.EmptyTypes, typeof(IoC).Module);
                    var il = method.GetILGenerator();
                    var resolve = typeof(IoC).GetMethod("Resolve", Type.EmptyTypes);

                    var argVals = new List<object>();
                    foreach (var arg in ctor.GetParameters())
                    {
                        il.Emit(OpCodes.Call, resolve.MakeGenericMethod(arg.ParameterType));
                    }

                    il.Emit(OpCodes.Newobj, ctor);
                    il.Emit(OpCodes.Ret);

                    creator = (Func<object>)method.CreateDelegate(typeof(Func<object>));
                }
            }

            if (creator == null)
                throw new ArgumentException("Unable to create type " + type);

            return creator;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.Configuration
{
    public class DependencyInjection
    {

        static DependencyInjection()
        {
            RegisterTypes();
        }

        private static Dictionary<Type, IList<Type>> _container = new Dictionary<Type, IList<Type>>();

        public static T Resolve<T>() where T : class
        {
            return ResolveAll<T>().FirstOrDefault();
        }

        public static IEnumerable<T> ResolveAll<T>() where T : class
        {
            if (_container.ContainsKey(typeof(T)))
            {
                var returnList = new List<T>();

                foreach (var type in _container[typeof(T)])
                {
                    returnList.Add((T)Activator.CreateInstance(type));
                }

                return returnList;
            }
            return new List<T>();
        }

        private static void RegisterTypes()
        {

            List<Assembly> assemblies = GetAssemblies().ToList();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    //ex.types is loaded with the successfully loaded types and nulls are types that failed to load
                    types = ex.Types.Where(p => p != null).ToArray();
                }
                foreach (Type type in types)
                {
                    if (type.IsClass)
                    {
                        var attribute = (ExportAttribute)Attribute.GetCustomAttribute(type, typeof(ExportAttribute));
                        if (attribute != null && attribute.ForInterface != null)
                        {
                            if (!_container.ContainsKey(attribute.ForInterface))
                            {
                                _container.Add(attribute.ForInterface, new List<Type>());
                            }

                            _container[attribute.ForInterface].Add(type);
                        }
                    }
                }
            }
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            var assemblies = new Dictionary<string, Assembly>();

            var assembly = typeof(DependencyInjection).Assembly;

            assemblies.Add(assembly.FullName, typeof(DependencyInjection).Assembly);

            foreach (string file in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(assembly.Location), "DatabaseTools*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    AssemblyName name = AssemblyName.GetAssemblyName(file);
                    if (!assemblies.ContainsKey(name.FullName))
                    {
                        Assembly referencedAssembly = Assembly.Load(name);
                        assemblies.Add(name.FullName, referencedAssembly);
                    }
                }
                catch
                {

                }

            }

            return assemblies.Values;

        }

    }
}

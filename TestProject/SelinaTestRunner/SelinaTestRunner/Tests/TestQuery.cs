using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SelinaTestRunner
{
    class TestQuery
    {
        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            try
            {
                var types = assembly.GetTypes();
                return types.Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                throw new Exception(sb.ToString());
                //Display or log the error based on your application.
            }
            return null;
        }

        private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.ReflectionOnlyLoad(args.Name);
        }

        public List<string> GetTestNames(string assemblyRelPath, string @namespace)
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;
            Type[] typelist = GetTypesInNamespace(Assembly.ReflectionOnlyLoadFrom(assemblyRelPath), @namespace);
            List<string> result = new List<string>();
            foreach (Type type in typelist)
            {
                if (type.CustomAttributes.ToList().Any(ca => ca.ToString().Contains("TestClassAttribute")))
                {
                    var methods = type.GetMethods();
                    foreach (var method in methods)
                    {
                        if (method.CustomAttributes.ToList().Any(ca => ca.ToString().Contains("TestMethodAttribute")))
                        {
                            result.Add(type.Name + "." + method.Name);
                        }
                    }
                }
            }
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= CurrentDomain_ReflectionOnlyAssemblyResolve;
            return result;
        }

    }
}

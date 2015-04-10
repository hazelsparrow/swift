using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Ninject;

namespace Swift
{
    public class DependencyResolver
    {
        public class Bindable
        {
            private readonly IKernel kernel;
            public Type Type { get; set; }

            public Bindable(Type type, IKernel kernel)
            {
                this.kernel = kernel;
                Type = type;
            }

            public void To(Type type)
            {
                kernel.Rebind(Type).To(type);
            }
        }

        public class Bindable<TAbstraction>
        {
            private readonly IKernel kernel;

            public Bindable(IKernel kernel)
            {
                this.kernel = kernel;
            }

            public void To<TImplementation>()
                where TImplementation : TAbstraction
            {
                kernel.Rebind<TAbstraction>().To<TImplementation>();
            }
        }

        static readonly IKernel kernel = new StandardKernel();

        public static Bindable<TAbstraction> Bind<TAbstraction>()
        {
            return new Bindable<TAbstraction>(kernel);
        }

        public static Bindable Bind(Type abstractionType)
        {
            return new Bindable(abstractionType, kernel);
        }

        public static T GetService<T>()
        {
            try
            {
                return kernel.Get<T>();
            }
            catch
            {
                DependencyResolver.LoadDependencyModules();
                return kernel.Get<T>();
            }
        }

        public static object GetService(Type serviceType)
        {
            return kernel.GetService(serviceType);
        }

        public static void LoadDependencyModules()
        {
            lock (typeof(DependencyResolver))
            {
                Type dm = typeof(IDependencyModule);
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var types = a.GetTypes().Where(i => dm.IsAssignableFrom(i) && i.IsClass && !i.IsAbstract).ToList();

                        foreach (var t in types)
                        {
                            try
                            {
                                var module = (IDependencyModule)Activator.CreateInstance(t);
                                module.Load();

                                foreach (var binding in module.GetBindings())
                                {
                                    DependencyResolver.Bind(binding.Key).To(binding.Value);
                                }
                            }
                            catch
                            { }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
    }

    public interface IDependencyModule
    {
        IDictionary<Type, Type> GetBindings();
        void Load();
    }

    public abstract class DependencyModule : IDependencyModule
    {
        public class DependencyModuleBindable<TAbstraction>
        {
            private readonly Dictionary<Type, Type> dic;

            public DependencyModuleBindable(Dictionary<Type, Type> dic)
            {
                this.dic = dic;
            }

            public void To<TImplementation>()
                where TImplementation : TAbstraction
            {
                dic[typeof(TAbstraction)] = typeof(TImplementation);
            }
        }

        readonly Dictionary<Type, Type> dic = new Dictionary<Type, Type>();

        protected DependencyModuleBindable<TAbstraction> Bind<TAbstraction>()
        {
            return new DependencyModuleBindable<TAbstraction>(dic);
        }

        public abstract void Load();

        public IDictionary<Type, Type> GetBindings()
        {
            return new Dictionary<Type, Type>(dic);
        }
    }
}
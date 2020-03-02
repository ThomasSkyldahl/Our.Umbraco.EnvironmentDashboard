using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Our.Umbraco.EnvironmentDashboard.Components
{
	public abstract class FactoryCollectionBuilder<TBuilder, TCollection, TItem> : ICollectionBuilder<TCollection, TItem> 
		where TBuilder : FactoryCollectionBuilder<TBuilder, TCollection, TItem>
		where TCollection : class, IBuilderCollection<TItem>
	{
		private readonly List<FactoryRegistration> _factoryRegistrations = new List<FactoryRegistration>(); 

		protected abstract TBuilder This { get; }

		public TCollection CreateCollection(IFactory factory)
		{
			var instances = new List<TItem>(_factoryRegistrations.Count);

			foreach (var factoryRegistration in _factoryRegistrations)
			{
				instances.Add((TItem)factoryRegistration.GetInstance(factory));
			}

			return factory.CreateInstance<TCollection>(instances);;
		}

		public void RegisterWith(IRegister register)
		{
			foreach (var factoryRegistration in _factoryRegistrations)
			{
				factoryRegistration.RegisterWith(register);
			}

			register.Register(CreateCollection);
		}

		public TBuilder Append<TServiceItem>(Lifetime lifetime = Lifetime.Transient) where TServiceItem: class, TItem
		{
			_factoryRegistrations.Add(new FactoryRegistration<TServiceItem>(lifetime));

			return This;
		}

		public TBuilder Append<TServiceItem>(Func<IFactory, TServiceItem> factory, Lifetime lifetime = Lifetime.Transient) where TServiceItem: class, TItem
		{
			_factoryRegistrations.Add(new FactoryRegistration<TServiceItem>(factory, lifetime));

			return This;
		}

		private abstract class FactoryRegistration
		{
			public abstract void RegisterWith(IRegister register);

			public abstract object GetInstance(IFactory factory);
		}

		private class FactoryRegistration<TService> : FactoryRegistration where TService: class, TItem
		{
			private readonly Lifetime _lifetime;
			private readonly Func<IFactory, TService> _factory;
			private readonly Type _serviceTargetType;
			private readonly MethodInfo _getInstanceMethod;

			public FactoryRegistration(Func<IFactory, TService> factory, Lifetime lifetime)
			{
				_factory = factory;

				if (_factory != null)
				{
					var typeName = $"Factory_{Guid.NewGuid():N}";

					var compilerResults = new CSharpCodeProvider()
						.CompileAssemblyFromSource(
							new CompilerParameters
							{
								GenerateInMemory = true,
								ReferencedAssemblies =
								{
									"System.dll",
								}
							},
							$"namespace Our.Umbraco.EnvironmentDashboard.Components.GeneratedFactoryTypes {{ public class {typeName} {{}} }}");

					_serviceTargetType = compilerResults.CompiledAssembly.ExportedTypes.First();
					var templateMethod = typeof(IFactory).GetMethods().First(m => m.ToString() == "TService GetInstanceFor[TService,TTarget]()");
					_getInstanceMethod = templateMethod.MakeGenericMethod(typeof(TService), _serviceTargetType);

				}

				_lifetime = lifetime;
			}

			public FactoryRegistration(Lifetime lifetime):this(null, lifetime)
			{
			}

			public override void RegisterWith(IRegister register)
			{
				if (_factory != null)
				{
					var foundMethod = register.GetType().GetMethods().First(m => m.ToString() == "Void RegisterFor[TService,TTarget](System.Func`2[Umbraco.Core.Composing.IFactory,TService], Umbraco.Core.Composing.Lifetime)");

					if (foundMethod != null)
					{
						var genericMethod = foundMethod.MakeGenericMethod(typeof(TService), _serviceTargetType);

						genericMethod.Invoke(register, new object[] {_factory, _lifetime});
					}
				}
				else
				{
					register.Register(typeof(TService), _lifetime);
				}
			}

			public override object GetInstance(IFactory factory)
			{
				if (_factory != null)
				{
					return _getInstanceMethod.Invoke(factory, null);
				}

				return factory.GetInstance(typeof(TService));
			}
		}
	}
}
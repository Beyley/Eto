using System;
using System.Reflection;
using sc = System.ComponentModel;
using System.ComponentModel;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Eto
{
	// generic property descriptor wrapper needed to support using PropertyDescriptor in .NET Standard 1.0
	// Can be removed after .NET Standard 1.0 support is dropped.
	interface IPropertyDescriptor
	{
		Type ComponentType { get; }
		object GetValue(object obj);
		void SetValue(object obj, object value);
		Type PropertyType { get; }
		string Name { get; }
		string DisplayName { get; }
		bool IsReadOnly { get; }
		bool CanRead { get; }
		bool IsBrowsable { get; }
		sc.TypeConverter Converter { get; }
		Attribute GetCustomAttribute(Type attributeType);
		T GetCustomAttribute<T>() where T : Attribute;
	}

	class PropertyInfoDescriptor : IPropertyDescriptor
	{
		PropertyInfo _property;

		public Type ComponentType => _property.DeclaringType;

		public Type PropertyType => _property.PropertyType;

		public string Name => _property.Name;

		public string DisplayName => _property.Name;

		public bool IsReadOnly => !_property.CanWrite || _property.SetMethod?.IsPrivate == true;

		public bool IsBrowsable => _property.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false;

		public sc.TypeConverter Converter => sc.TypeDescriptor.GetConverter(PropertyType);

		public bool CanRead => _property.CanRead && _property.GetMethod?.IsPublic == true;

		public PropertyInfoDescriptor(PropertyInfo property)
		{
			_property = property;
		}

		public T GetCustomAttribute<T>()
			where T : System.Attribute
		{
			return _property.GetCustomAttribute<T>();
		}

		public Attribute GetCustomAttribute(Type attributeType)
		{
			return _property.GetCustomAttribute(attributeType);
		}

		public object GetValue(object obj) => _property.GetValue(obj);

		public void SetValue(object obj, object value) => _property.SetValue(obj, value);
	}

	class PropertyDescriptorDescriptor : IPropertyDescriptor
	{
		PropertyDescriptor _propertyDescriptor;

		public static bool IsSupported => true;

		public static PropertyDescriptorDescriptor Get(object obj)
		{
			if (obj is PropertyDescriptor propertyDescriptor)
				return new PropertyDescriptorDescriptor(propertyDescriptor);

			return null;
		}

		public PropertyDescriptorDescriptor(PropertyDescriptor descriptor)
		{
			_propertyDescriptor = descriptor;
		}

		public Type ComponentType => _propertyDescriptor.ComponentType;

		public Type PropertyType => _propertyDescriptor.PropertyType;

		public string Name => _propertyDescriptor.Name;

		public string DisplayName => _propertyDescriptor.DisplayName;

		public bool IsReadOnly => _propertyDescriptor.IsReadOnly;

		public bool CanRead => true;

		public bool IsBrowsable => _propertyDescriptor.IsBrowsable;

		public sc.TypeConverter Converter => _propertyDescriptor.Converter;

		public object GetValue(object obj) => _propertyDescriptor.GetValue(obj);

		public void SetValue(object obj, object value) => _propertyDescriptor.SetValue(obj, value);

		ICollection Attributes => _propertyDescriptor.Attributes;

		public Attribute GetCustomAttribute(Type attributeType)
		{
			return Attributes.OfType<Attribute>().FirstOrDefault(r => r.GetType() == attributeType);
		}

		public T GetCustomAttribute<T>() where T : Attribute
		{
			return Attributes.OfType<T>().FirstOrDefault();
		}
	}

	static class EtoTypeDescriptor
	{
		public static IPropertyDescriptor Get(object obj)
		{
			if (obj is PropertyInfo propertyInfo)
				return new PropertyInfoDescriptor(propertyInfo);
			else
				return PropertyDescriptorDescriptor.Get(obj);
		}

		static MethodInfo s_GetPropertiesMethod = typeof(sc.TypeDescriptor).GetRuntimeMethod("GetProperties", new Type[] { typeof(Type) });

		public static IEnumerable<IPropertyDescriptor> GetProperties(Type type)
		{
			if (s_GetPropertiesMethod != null)
				((ICollection)s_GetPropertiesMethod.Invoke(null, new object[] { type })).OfType<object>().Select(r => Get(r));
			return type.GetRuntimeProperties().Select(r => Get(r));
		}

		public static IPropertyDescriptor GetProperty(Type type, string name)
		{
			foreach (var property in GetProperties(type))
			{
				if (property.Name == name)
					return property;
			}
			return null;
		}
	}
}

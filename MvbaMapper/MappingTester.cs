using System.Collections.Generic;
using System.Linq;

using CodeQuery;

using MvbaCore;

namespace MvbaMapper
{
	public class MappingTester<TSource, TDestination>
		where TSource : new()
	{
		private readonly Dictionary<string, object> _propertyValues = new Dictionary<string, object>();
		private readonly TSource _source;

		public MappingTester()
		{
			_source = new TSource();
			Populate(_source);
		}

		public TSource Source
		{
			get { return _source; }
		}

		private void Populate(TSource source)
		{
			foreach (var propertyInfo in source.GetType()
				.GetProperties()
				.ThatHaveAGetter()
				.ThatHaveASetter())
			{
				var value = RandomValueType.GetFor(propertyInfo.PropertyType.FullName).CreateRandomValue();
				_propertyValues.Add(propertyInfo.Name, value);
				propertyInfo.SetValue(source, value, null);
			}
		}

		public Notification Verify(TDestination actual, TDestination expected)
		{
			Notification notification = new Notification();
			var destinationProperties = expected.GetType()
				.GetProperties()
				.ThatHaveASetter()
				.ThatHaveAGetter()
				.ToDictionary(x => x.Name, x => x);

			foreach (var propertyInfo in destinationProperties.Values)
			{
				var expectedValue = propertyInfo.GetValue(expected, null);
				var actualValue = propertyInfo.GetValue(actual, null);

				if (expectedValue == null && actualValue == null)
				{
					continue;
				}
				if (expectedValue == null || actualValue == null)
				{
					notification.Add(Notification.WarningFor(propertyInfo.Name));
					continue;
				}
				if (!expectedValue.Equals(actualValue))
				{
					notification.Add(Notification.WarningFor(propertyInfo.Name));
				}
			}
			return notification;
		}
	}
}
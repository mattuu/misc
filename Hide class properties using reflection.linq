<Query Kind="Program">
  <Output>DataGrids</Output>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.Entity.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Expressions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Parallel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Queryable.dll</Reference>
  <NuGetReference>AutoFixture</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Serialization</Namespace>
  <Namespace>Ploeh.AutoFixture</Namespace>
  <Namespace>System.Data.Objects.SqlClient</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Collections.ObjectModel</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Ploeh.AutoFixture.Dsl</Namespace>
</Query>

void Main()
{
	var fixture = new Fixture();

	            var child = GetEmptyTestChildBuilder(fixture).With(c => c.Children, new[] {GetEmptyTestChildBuilder(fixture).Create()})
                                                         .With(e => e.Overrides, fixture.Build<Override>()
                                                                                        .With(o => o.IsResolved, false)
                                                                                        .With(o => o.OverrideType, OverrideType.Hiding)
																						.With(o => o.FieldName, "Children")
																						//.With(o => o.DataKey, $"{{\"Property1\": \"{grandChild.Property1}\"}}")
																						.Without(o => o.DataKey)
																						.CreateMany(1)
																						.ToList())
														 .Create();

	var overridable = fixture.Build<TestOverridable>()
							 .With(e => e.Children, new[] { child })
							 .Without(e => e.Overrides)
							 .Create();


	var sut = new ContentBlocker();

	sut.Run(ref overridable).ContinueWith(t =>
	{

		overridable.Children.Sum(c => c.Children?.Count()).Dump("TOTAL GRAND CHILDREN COUNT");
	});

	// assert..
	//	overridable.Children.Dump("CHILDREN");


}

private static IPostprocessComposer<TestChild> GetEmptyTestChildBuilder(IFixture fixture)
{
	return fixture.Build<TestChild>()
				  .Without(c => c.Children)
				  .Without(c => c.Overrides);
}

public class ContentBlocker : IContentBlocker
{
	public Task Run<TOverridable>(ref TOverridable overridable)
			 where TOverridable : class, IOverridable
	{
		return Task.Factory.StartNew(t =>
		{
			var overridableEntity = t as TOverridable;

			var hidings = overridableEntity.GetUnresolvedHidings();
			if (hidings != null)
			{
				foreach (var hiding in hidings)
				{
					var propertyInfo = typeof(TOverridable).GetProperty(hiding.FieldName);
					if (propertyInfo == null)
					{
						continue;
					}

					var isCollection = typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType);

					object newValue = null;

					if (isCollection && !string.IsNullOrEmpty(hiding.DataKey)) // processing child collection..
					{
						Type collectionElementType = null;
						if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
						{
							collectionElementType = propertyInfo.PropertyType.GetGenericArguments()[0]; // use this...
						}

						if (collectionElementType == null)
							continue;

						var dataKeyObject = (JObject)JsonConvert.DeserializeObject(hiding.DataKey);

						var dataItemList = propertyInfo.GetValue(overridableEntity) as IList;

						var listType = typeof(List<>);
						var constructedListType = listType.MakeGenericType(collectionElementType);

						var newItemList = (IList)Activator.CreateInstance(constructedListType);

						if (dataItemList != null)
						{
							foreach (var dataItem in dataItemList)
							{
								bool shouldDelete = true;
								foreach (var jsonProperty in dataKeyObject.Properties())
								{
									var jsonObjectValue = dataKeyObject.GetValue(jsonProperty.Name).ToString();
									var listItemValue = $"{collectionElementType.GetProperty(jsonProperty.Name)?.GetValue(dataItem)}";

									if (!string.Equals(listItemValue, jsonObjectValue))
									{
										shouldDelete = false;
									}
								}

								if (!shouldDelete)
									newItemList.Add(dataItem);
							}

							newValue = newItemList;
						}
					}
					else
					{
						newValue = GetDefault(propertyInfo.PropertyType);
					}

					propertyInfo?.SetValue(overridableEntity, newValue);
				}
			}
			
			var propertyInfos = typeof(TOverridable).GetProperties()
													.Where(pi => pi.Name != "Overrides")
													.ToList();

			foreach (var propertyInfo in propertyInfos)
			{
				Console.WriteLine($"{propertyInfo.Name}");

				if (typeof(IOverridable).IsAssignableFrom(propertyInfo.PropertyType))
				{
					var value = (TOverridable)propertyInfo.GetValue(overridableEntity);
					Run(ref value);
				}

				Type collectionElementType = null;
				if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					collectionElementType = propertyInfo.PropertyType.GetGenericArguments()[0]; // use this...
				}

				if (collectionElementType != null && typeof(IOverridable).IsAssignableFrom(collectionElementType))
				{
					var dataItemList = propertyInfo.GetValue(overridableEntity) as IList;

					if (dataItemList == null)
						continue;

					foreach (var value in dataItemList)
					{
						object[] arguments = { value };
						Console.WriteLine($"arguments: {arguments}");
						var methodInfo = GetType().GetMethod("Run").MakeGenericMethod(collectionElementType);
						methodInfo.Invoke(this, arguments);
					}
				}
			}
		}, overridable);
	}

	private static object GetDefault(Type type)
	{
		return type.IsValueType ? Activator.CreateInstance(type) : null;
	}
}

public interface IContentBlocker
{
	Task Run<TOverridable>(ref TOverridable overridable)
		where TOverridable : class, IOverridable;
}

public interface IOverridable
{
	ICollection<Override> Overrides { get; set; }
}

public class TestOverridable : IOverridable
{
	public string Property1 { get; set; }

	public IEnumerable<TestChild> Children { get; set; }

	public ICollection<Override> Overrides { get; set; }
}

public class TestChild : IOverridable
{
	public string Property1 { get; set; }

	public int Property2 { get; set; }

	public IEnumerable<TestChild> Children { get; set; }

	public ICollection<Override> Overrides { get; set; }
}

public class Override
{
	public int OverrideId { get; set; }

	public int PropertyId { get; set; }

		public string FieldName { get; set; }

	public string DataKey { get; set; }

	public bool IsResolved { get; set; }

	public virtual OverrideType OverrideType { get; set; }
}

public enum OverrideType
{
	Hiding = 1,
	Query
}


public static class OverridableQueries
{
	public static IEnumerable<Override> GetUnresolvedHidings(this IOverridable overridable)
	{
		return overridable.Overrides?.Where(o => o.OverrideType == OverrideType.Hiding)
					.Where(o => !o.IsResolved);
	}
}
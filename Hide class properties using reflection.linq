<Query Kind="Program">
  <Output>DataGrids</Output>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.Entity.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Expressions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Parallel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Queryable.dll</Reference>
  <NuGetReference>AutoFixture</NuGetReference>
  <Namespace>Ploeh.AutoFixture</Namespace>
  <Namespace>System.Data.Objects.SqlClient</Namespace>
</Query>

void Main()
{
	
}

public class ContentBlocker : IContentBlocker
{
	public TOverridable RemoveBlockedContent<TOverridable>(TOverridable overridable)
		where TOverridable : class, IOverridable
	{
		overridable = HandleSimpleProperties(overridable);

		overridable = HandleChildCollectionProperties(overridable);

		return overridable;
	}

	private static TOverridable HandleSimpleProperties<TOverridable>(TOverridable overridable)
		where TOverridable : IOverridable
	{
		var propertyInfos = typeof(TOverridable).GetProperties()
												.Where(pi => pi.Name != "Overrides")
												.ToList();

		var overrideNames = overridable.GetUnresolvedHidings()
									   .Select(o => o.Configuration.ColumnName);

		var propertiesToBlock = propertyInfos.Select(pi => pi.Name).Intersect(overrideNames);

		foreach (var property in propertiesToBlock)
		{
			var propertyInfo = propertyInfos.SingleOrDefault(pi => pi.Name == property);
			propertyInfo?.SetValue(overridable, GetDefault(propertyInfo.PropertyType));
		}

		return overridable;
	}

	private static TOverridable HandleChildCollectionProperties<TOverridable>(TOverridable overridable)
		where TOverridable : IOverridable
	{
		var propertyInfos = typeof(TOverridable).GetProperties()
												.Where(pi => typeof(IEnumerable).IsAssignableFrom(pi.PropertyType))
												.Where(pi => pi.PropertyType.IsGenericType)
												.Where(pi => pi.Name != "Overrides")
												.ToList();

		foreach (var propertyInfo in propertyInfos)
		{
			Type collectionElementType = null;
			if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				collectionElementType = propertyInfo.PropertyType.GetGenericArguments()[0]; // use this...
			}

			if (collectionElementType == null)
				continue;

			var hidings = overridable.GetUnresolvedHidings()
									 .Where(o => o.Configuration.TableName == collectionElementType.Name)
									 .ToList();

			if (!hidings.Any())
				continue;

			foreach (var hiding in hidings)
			{
				// copy propertyInfo.GetValue() into IList
				var list = propertyInfo.GetValue(overridable) as IList;

				// find item to remove..
				var pi = collectionElementType.GetProperty(hiding.Configuration.ColumnName);
				object itemToDelete = null;
				foreach (var item in list)
				{
					if (pi.GetValue(item).ToString() == hiding.Configuration.DataKey)
					{
						itemToDelete = item;
					}
				}

				// remove item from list
				if (itemToDelete != null)
				{
					list.Remove(itemToDelete);
				}

				// shift list back into propertyValue
				propertyInfo.SetValue(overridable, list);
			}
		}

		return overridable;
	}

	private static object GetDefault(Type type)
	{
		return type.IsValueType ? Activator.CreateInstance(type) : null;
	}
}

public interface IContentBlocker
{
	TOverridable RemoveBlockedContent<TOverridable>(TOverridable overridable)
		where TOverridable : class, IOverridable;
}

public interface IOverridable
{
	ICollection<Override> Overrides { get; set; }
}

public class Override
{
	public int OverrideId { get; set; }

	public int PropertyId { get; set; }

	public Configuration Configuration { get; set; }

	public bool IsResolved { get; set; }

	public virtual OverrideType OverrideType { get; set; }
}

public enum OverrideType
{
	Hiding = 1,
	Query
}

public class Configuration
{
	public int ConfigurationId { get; set; }

	public string TableName { get; set; }

	public string ColumnName { get; set; }

	public string DataKey { get; set; }

	public bool CanHide { get; set; }

	public bool CanQuery { get; set; }

	public bool CanSentToJ2H { get; set; }

	public bool IsEnabled { get; set; }
}

public static class OverridableQueries
{
	public static IEnumerable<Override> GetUnresolvedHidings(this IOverridable overridable)
	{
		return overridable.Overrides.Where(o => o.OverrideType == OverrideType.Hiding)
					.Where(o => !o.IsResolved);
	}
}
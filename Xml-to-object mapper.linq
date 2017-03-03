<Query Kind="Program">
  <Output>DataGrids</Output>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.Entity.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Expressions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Parallel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Queryable.dll</Reference>
  <Namespace>System.Data.Objects.SqlClient</Namespace>
  <Namespace>System.Collections.ObjectModel</Namespace>
</Query>

void Main()
{
	var xml = @"<MoreInfo>
      <InfoItem>
        <InfoName>Ground Floor - Bedroom 1</InfoName>
        <InfoValue>Double Bed, Ensuite with shower, toilet and basin, Air Conditioning*, Balcony</InfoValue>
      </InfoItem>
      <InfoItem>
        <InfoName>Ground Floor - Bedroom 2</InfoName>
        <InfoValue>Double Bed, Air Conditioning*, Balcony</InfoValue>
      </InfoItem>
      <InfoItem>
        <InfoName>Ground Floor - Kitchen</InfoName>
        <InfoValue>Oven, Hob, Microwave, Dishwasher, Fridge Freezer</InfoValue>
      </InfoItem>
      <InfoItem>
        <InfoName>Ground Floor - Lounge</InfoName>
        <InfoValue>Flat Screen TV, DVD Player, Music Player, Air Conditioning*, Comfortable Seating, Dining Facilities, Doors to Terrace Gardens</InfoValue>
      </InfoItem>	
	<InfoItem>
	    <InfoName>Pool Size</InfoName>
	    <InfoValue>Pool Depth - 1.5m, Pool Size - 758</InfoValue>
	  </InfoItem>
  <InfoItem>
    <InfoName>Facilities</InfoName>
    <InfoValue>Sailing</InfoValue>
  </InfoItem>
  <InfoItem>
    <InfoName>Villa Type</InfoName>
    <InfoValue>Family friendly villas</InfoValue>
  </InfoItem>
  <InfoItem>
    <InfoName>Facilities Solmar</InfoName>
    <InfoValue>Private Pool, Off Road Parking, Air Conditioning, WIFI, Dishwasher, Standard Changeover Day: Sunday</InfoValue>
  </InfoItem>
  <InfoItem>
    <InfoName>Location</InfoName>
    <InfoValue>Car Optional, Walking Distance to Beach, Walking Distance to Restaurant, Walking Distance to Shops</InfoValue>
  </InfoItem>
  <InfoItem>
    <InfoName>Property Type</InfoName>
    <InfoValue>New Villa, Something Special, Exclusive Villas</InfoValue>
  </InfoItem>
  <InfoItem>
    <InfoName>Distances to nearest</InfoName>
    <InfoValue>Beach - 0.3km, Restaurant - 0.5km, Shop - 1.3km</InfoValue>
  </InfoItem>
  <InfoItem>
    <InfoName>Check In Time</InfoName>
    <InfoValue>16:00</InfoValue>
  </InfoItem>
  <InfoItem>
    <InfoName>Check Out Time</InfoName>
    <InfoValue>10.00</InfoValue>
  </InfoItem>
   <InfoItem>
        <InfoName>Changeover Day</InfoName>
        <InfoValue>Usually: Sunday</InfoValue>
	</InfoItem>
</MoreInfo>";

	new Runner().Run(xml);
}


public class Runner
{
	public void Run(string xml)
	{
		
		var xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xml);


		var mapper = new PropertyModeMapper();
		var model = mapper.Map(xmlDocument);
		model.Dump();
	}
}

public class AreaModelMapper : ModelMapperBase<AreaModel>
{
	public AreaModelMapper()
	{
		_mappingExpressions = new Collection<IMappingExpression<AreaModel>>
		{
			new RegExMappingExpression<AreaModel, string>(m => m.Name, "//InfoItem[contains(InfoName, 'Floor')]/InfoName/text()", "[a-zA-z1-9 ]* - "),
			new RegExMappingExpression<AreaModel, string>(m => m.Location, "//InfoItem[contains(InfoName, 'Floor')]/InfoName/text()", " - [a-zA-z1-9 ]*"),
			new RegExMappingExpression<AreaModel, string>(m => m.Type,"//InfoItem[contains(InfoName, 'Floor')]/InfoName/text()", "[a-zA-z1-9 ]* - | [1-9]", s => s.ToLower()),
		};
	}
}

public class RegExMappingExpression<TModel, TPropertyType> : IMappingExpression<TModel>
{
	string _xPath;
	string _pattern;
	Expression<Func<TModel, TPropertyType>> _expressionFunc;
	Func<string, TPropertyType> _propertyConverterFunc;

	public RegExMappingExpression(Expression<Func<TModel, TPropertyType>> expressionFunc, string xPath, string replacementPattern, Func<string, TPropertyType> propertyConverterFunc = null)
	{
		_pattern = replacementPattern;
		_xPath = xPath;
		_expressionFunc = expressionFunc;
		_propertyConverterFunc = propertyConverterFunc;
	}

	public void Map(XmlDocument xmlDocument, ref TModel model)
	{
		XmlNode root = xmlDocument.DocumentElement;
		var node = root.SelectSingleNode(_xPath, new XmlNamespaceManager(xmlDocument.NameTable));

		if (node == null || string.IsNullOrEmpty(node.Value))
		{
			return;
		}
		
		Console.WriteLine(node.Value);
		
		var matchedValue = Regex.Replace(node.Value, _pattern, "");

		object value = matchedValue;
		if (_propertyConverterFunc != null)
		{
			value = _propertyConverterFunc.Invoke(matchedValue);
		}
		Console.WriteLine(node.Value + " -> " + value);

		PropertySetter.GetSetter(_expressionFunc).Invoke(model, (TPropertyType)value);
	}
}

public class GeneralModelMapper : ModelMapperBase<GeneralModel>
{
	
	public GeneralModelMapper()
	{
		_mappingExpressions = new Collection<IMappingExpression<GeneralModel>>
		{
			new XPathMappingExpression<GeneralModel, string>(m => m.CheckIn, "//InfoItem[InfoName/text() = 'Check In Time']/InfoValue/text()"),
			new XPathMappingExpression<GeneralModel, string>(m => m.CheckOut, "//InfoItem[InfoName/text() = 'Check Out Time']/InfoValue/text()")
		};
	}	
}

public class PropertyModeMapper : ModelMapperBase<PropertyModel>
{
	public PropertyModeMapper()
	{
		_mappingExpressions = new Collection<IMappingExpression<PropertyModel>>
		{
			new ChildMappingExpression<PropertyModel, AreaModel>(m => m.Area, new AreaModelMapper()),
            new ChildMappingExpression<PropertyModel, GeneralModel>(m => m.General, new GeneralModelMapper())
		};
	}
}

public abstract class ModelMapperBase<TModel>
	where TModel : new()
{
	protected ICollection _mappingExpressions;
	
	protected ModelMapperBase()
	{
		
	}

	public TModel Map(XmlDocument xmlDocument)
	{
		var model = new TModel();

		foreach (IMappingExpression<TModel> mappingExpression in _mappingExpressions)
		{
			mappingExpression.Map(xmlDocument, ref model);
		}

		return model;
	}
}


public interface IMappingExpression<T>
{
	void Map(XmlDocument xmlDocument, ref T model);
}

public class ChildMappingExpression<TModel, TPropertyType> : IMappingExpression<TModel>
	where TPropertyType: new()
{
	Expression<Func<TModel, TPropertyType>> _expressionFunc;
	ModelMapperBase<TPropertyType> _propertyMapperFunc;
	
	public ChildMappingExpression(Expression<Func<TModel, TPropertyType>> expressionFunc, ModelMapperBase<TPropertyType> propertyMapperFunc)
	{
		_expressionFunc = expressionFunc;
		_propertyMapperFunc = propertyMapperFunc;
	}
	
	public void Map(XmlDocument xmlDocument, ref TModel model)
	{
		PropertySetter.GetSetter(_expressionFunc).Invoke(model, _propertyMapperFunc.Map(xmlDocument));
	}
}

public class XPathMappingExpression<TModel, TPropertyType> : IMappingExpression<TModel>
//	where TModel: GeneralModel
{
	string _xPath;
	Expression<Func<TModel, TPropertyType>> _expressionFunc; 
	Func<string, TPropertyType> _propertyConverterFunc;
	
	public XPathMappingExpression(Expression<Func<TModel, TPropertyType>> expressionFunc, string xPath, Func<string, TPropertyType> propertyConverterFunc = null)
	{
		_xPath = xPath;
		_expressionFunc = expressionFunc;
		_propertyConverterFunc = propertyConverterFunc;
	}

	public void Map(XmlDocument xmlDocument, ref TModel model)
	{
		XmlNode root = xmlDocument.DocumentElement;
		var node = root.SelectSingleNode(_xPath, new XmlNamespaceManager(xmlDocument.NameTable));

		if (node == null || string.IsNullOrEmpty(node.Value))
		{
			return;
		}

		object value = node.Value;
		if (_propertyConverterFunc != null)
		{
			value = _propertyConverterFunc.Invoke(node.Value);
		}
		PropertySetter.GetSetter(_expressionFunc).Invoke(model, (TPropertyType) value);
	}
}

public class GeneralModel
{
	public string CheckIn { get; set; }
	
	public string CheckOut { get; set; }
}

public class PropertyModel
{
	public GeneralModel General { get; set;}

	public AreaModel Area { get; set;}
}


public class AreaModel
{
	public string Name { get; set; }

	public string Type { get; set; }

	public string Location { get; set; }
}

public static class PropertySetter
{
	public static Action<T, TProperty> GetSetter<T, TProperty>(Expression<Func<T, TProperty>> expression)
	{
		var memberExpression = (MemberExpression)expression.Body;
		var property = (PropertyInfo)memberExpression.Member;
		var setMethod = property.GetSetMethod();

		var parameterT = Expression.Parameter(typeof(T), "x");
		var parameterTProperty = Expression.Parameter(typeof(TProperty), "y");

		var newExpression = Expression.Lambda<Action<T, TProperty>>(Expression.Call(parameterT, setMethod, parameterTProperty), parameterT, parameterTProperty);

		return newExpression.Compile();
	}
}
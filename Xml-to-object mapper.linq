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
        <InfoName>Outdoors - Terraces and Gardens</InfoName>
        <InfoValue>Covered Terrace(s), Open Terrace(s), Alfresco Dining Facilities, Sun Loungers, Brick Built BBQ, Parasol, Garden</InfoValue>
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


		XmlNode root = xmlDocument.DocumentElement;
		var node = root.SelectSingleNode("//InfoItem[contains(InfoName, 'Floor') or contains(InfoName, 'Outdoors')]/InfoName");

node.Dump();

//		var m = new RegExMappingExpression<AreaModel, string>(_ => _.Location, "//InfoItem[contains(InfoName, 'Floor')]/InfoName", "\\w+");
		
//		var mapper = new AreaModelMapper();
//		
//		var model = mapper.Map(xmlDocument);
//		
//		model.Dump();


//		var mapper = new PropertyModeMapper();
//		
//		var model = mapper.Map(xmlDocument);
//		
//		model.Dump();
	}
}


public interface INodeValueProcessor<TPropertyType>
{
	TPropertyType Process(string nodeValue);
}

public class FacilityIdsNodeValueProcessor : INodeValueProcessor<IEnumerable<int>>
{	
	FacilityProvider _facilityProvider;

	public FacilityIdsNodeValueProcessor(FacilityProvider facilityProvider)
	{
		_facilityProvider = facilityProvider;
	}
	
	public IEnumerable<int> Process(string nodeValue)
	{
		var facilities = nodeValue.Split(',');

		return facilities.Select(f => f.Trim())
			.Select(f => _facilityProvider.GetFacility(f))
			.ToArray();
		
//		return new int[] { 1, 2, 3, 4};
//		throw new NotImplementedException();
	}
}

public class FacilityProvider
{
	IDictionary<int, string> _facilities;
	
	public FacilityProvider()
	{
		_facilities = new Dictionary<int, string>();
		_facilities.Add(1, "Air Conditioning");
		_facilities.Add(2, "Balcony");
		_facilities.Add(3, "Oven");
		_facilities.Add(4, "Microwave");
		_facilities.Add(5, "Dishwasher");
		_facilities.Add(6, "Fridge Freezer");		
		_facilities.Add(7, "Flat Screen TV");
		_facilities.Add(8, "DVD Player");
		_facilities.Add(9, "Music Player");
		_facilities.Add(10, "Air Conditioning");
		_facilities.Add(11, "Comfortable Seating");
		_facilities.Add(13, "Doors to Terrace Gardens");
	}

	public int GetFacility(string facilityName)
	{
		Console.WriteLine(facilityName);
		return _facilities.SingleOrDefault(x => x.Value == facilityName).Key;
	}
}


public class GeneralModelMapper : ModelMapperBase<GeneralModel>
{
	
	public GeneralModelMapper()
	{
		_mappingExpressions = new Collection<IMappingExpression<GeneralModel>>
		{
			new XPathMappingExpression<GeneralModel, string>(m => m.CheckIn, "//InfoItem[InfoName/text() = 'Check In Time']/InfoValue/text()"),
			new XPathMappingExpression<GeneralModel, string>(m => m.CheckOut, "//InfoItem[InfoName/text() = 'Check Out Time']/InfoValue/text()"),
			//			new MappingExpression<Model, decimal?>("//InfoItem[InfoName/text() = 'Pool Size']/InfoValue/text()", m => m.PoolDepth, str => string.IsNullOrEmpty(str) ? default(decimal?) : decimal.Parse(str) )									   
//			new MappingExpression<GeneralModel, string>("//InfoItem[InfoName/text() = 'Pool Size']/InfoValue/text()", m => m.CheckOut),
		};
	}	
}

public class PropertyModeMapper : ModelMapperBase<PropertyModel>
{
	public PropertyModeMapper()
	{
		_mappingExpressions = new Collection<IMappingExpression<PropertyModel>>
		{
//			new ChildMappingExpression<PropertyModel, AreaModel>(m => m.Area, new AreaModelMapper()),
//            new ChildMappingExpression<PropertyModel, GeneralModel>(m => m.General, new GeneralModelMapper())
			//			new MappingExpression<GeneralModel, string>("//InfoItem[InfoName/text() = 'Check In Time']/InfoValue/text()", m => m.CheckIn),
//			new MappingExpression<GeneralModel, string>("//InfoItem[InfoName/text() = 'Check Out Time']/InfoValue/text()", m => m.CheckOut),
			//			new MappingExpression<Model, decimal?>("//InfoItem[InfoName/text() = 'Pool Size']/InfoValue/text()", m => m.PoolDepth, str => string.IsNullOrEmpty(str) ? default(decimal?) : decimal.Parse(str) )									   
			//			new MappingExpression<GeneralModel, string>("//InfoItem[InfoName/text() = 'Pool Size']/InfoValue/text()", m => m.CheckOut),
		};
	}
}

public class AreaModelMapper : ModelMapperBase<AreaModel>
{
	public AreaModelMapper()
	{
		_mappingExpressions = new Collection<IMappingExpression<AreaModel>>
		{
			//			new XPathMappingExpression<AreaModel, string>(m => m.Name, "//InfoItem[InfoName/contains('Ground Floor')]/InfoValue/text()"),
			new XPathMappingExpression<AreaModel, string>(m => m.Type,"//InfoItem[contains(InfoName, 'Ground Floor')]/InfoName/text()"),
			new XPathMappingExpression<AreaModel, IEnumerable<int>>(m => m.FacilityIds,"//InfoItem[contains(InfoName, 'Floor')]/InfoValue/text()", new FacilityIdsNodeValueProcessor(new FacilityProvider())),
			//			new MappingExpression<Model, decimal?>("//InfoItem[InfoName/text() = 'Pool Size']/InfoValue/text()", m => m.PoolDepth, str => string.IsNullOrEmpty(str) ? default(decimal?) : decimal.Parse(str) )									   
			//			new MappingExpression<GeneralModel, string>("//InfoItem[InfoName/text() = 'Pool Size']/InfoValue/text()", m => m.CheckOut),
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


public class XPathMappingExpression<TModel, TPropertyType> : IMappingExpression<TModel>
//	where TModel: GeneralModel
{
	string _xPath;
	Expression<Func<TModel, TPropertyType>> _expressionFunc; 
	INodeValueProcessor<TPropertyType> _nodeValueProcessor;
	
	public XPathMappingExpression(Expression<Func<TModel, TPropertyType>> expressionFunc, string xPath, INodeValueProcessor<TPropertyType> nodeValueProcessor = null)
	{
		_xPath = xPath;
		_expressionFunc = expressionFunc;
		_nodeValueProcessor = nodeValueProcessor;
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
		if (_nodeValueProcessor != null)
		{
			value = _nodeValueProcessor.Process(node.Value);
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
//	public int PropertyId { get; set; }
//	
//	public string PropertyName { get; set; }
	
	public GeneralModel General { get; set;}

	public AreaModel Area { get; set;}
}


public class AreaModel
{
	public string Name { get; set; }

	public string Type { get; set; }

	public IEnumerable<int> FacilityIds { get; set; }

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
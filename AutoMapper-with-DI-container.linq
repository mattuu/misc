<Query Kind="Program">
  <Output>DataGrids</Output>
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\PresentationUI.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\ReachFramework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.Entity.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Expressions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Parallel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Queryable.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\System.Printing.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Xaml.dll</Reference>
  <NuGetReference>AutoFixture</NuGetReference>
  <NuGetReference>AutoMapper</NuGetReference>
  <NuGetReference>Unity</NuGetReference>
  <Namespace>AutoMapper</Namespace>
  <Namespace>AutoMapper.Configuration</Namespace>
  <Namespace>AutoMapper.Configuration.Conventions</Namespace>
  <Namespace>AutoMapper.Configuration.Internal</Namespace>
  <Namespace>AutoMapper.Execution</Namespace>
  <Namespace>AutoMapper.Internal</Namespace>
  <Namespace>AutoMapper.Mappers</Namespace>
  <Namespace>AutoMapper.Mappers.Internal</Namespace>
  <Namespace>AutoMapper.QueryableExtensions</Namespace>
  <Namespace>AutoMapper.QueryableExtensions.Impl</Namespace>
  <Namespace>AutoMapper.XpressionMapper</Namespace>
  <Namespace>AutoMapper.XpressionMapper.Extensions</Namespace>
  <Namespace>AutoMapper.XpressionMapper.Structures</Namespace>
  <Namespace>Ploeh.AutoFixture</Namespace>
  <Namespace>Microsoft.Practices.Unity</Namespace>
  <Namespace>Ploeh.AutoFixture.Kernel</Namespace>
</Query>

void Main()
{
	var fixture = new Fixture();
	fixture.Register<IStringProvider>(() => new StringProvider());

	var container = new UnityContainer();
	container.RegisterType<IStringProvider, StringProvider>();

	Func<Type, object> serviceCtor = t => fixture.Create(t, new SpecimenContext(fixture));

	var config = new MapperConfiguration(cfg =>
			   {
				   cfg.AddProfile<IntToStringProfile>();

				   cfg.AllowNullCollections = true;

				   cfg.Advanced.AllowAdditiveTypeMapCreation = true;

				   cfg.ConstructServicesUsing(serviceCtor);
			   });

	IMapper mapper = new Mapper(config);

	var result = mapper.Map<string>(new Random().Next());

	result.Dump();
}

public class IntToStringProfile : Profile
{
	public IntToStringProfile()
	{
		CreateMap<int, string>()
			.ConvertUsing<IntToStringValueConverter>();
	}
}

public class IntToStringValueConverter : ITypeConverter<int, string>
{
	IStringProvider _stringProvider;

	public IntToStringValueConverter(IStringProvider stringProvider)
	{
		_stringProvider = stringProvider;
	}

	public string Convert(int source, string destination, ResolutionContext context)
	{
		return _stringProvider.GetString(source);
	}
}

public class StringProvider : IStringProvider
{
	public string GetString(int source)
	{
		return $"GETSTRING(): {source}";
	}
}

public interface IStringProvider
{
	string GetString(int source);
}

public static class FixtureExtensions
{
	public static object CreateUsingType(this IFixture fixture, Type type)
	{
		return fixture.Create(type, new SpecimenContext(fixture));
	}

}
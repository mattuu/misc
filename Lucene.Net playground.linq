<Query Kind="Program">
  <NuGetReference>Lucene.Net</NuGetReference>
  <NuGetReference>Lucene.Net.ObjectMapping</NuGetReference>
  <NuGetReference>NBuilder</NuGetReference>
  <Namespace>FizzWare.NBuilder</Namespace>
  <Namespace>FizzWare.NBuilder.Dates</Namespace>
  <Namespace>FizzWare.NBuilder.Extensions</Namespace>
  <Namespace>FizzWare.NBuilder.Generators</Namespace>
  <Namespace>FizzWare.NBuilder.Implementation</Namespace>
  <Namespace>FizzWare.NBuilder.PropertyNaming</Namespace>
  <Namespace>Lucene.Net</Namespace>
  <Namespace>Lucene.Net.Analysis</Namespace>
  <Namespace>Lucene.Net.Analysis.Standard</Namespace>
  <Namespace>Lucene.Net.Analysis.Tokenattributes</Namespace>
  <Namespace>Lucene.Net.Documents</Namespace>
  <Namespace>Lucene.Net.Index</Namespace>
  <Namespace>Lucene.Net.Messages</Namespace>
  <Namespace>Lucene.Net.QueryParsers</Namespace>
  <Namespace>Lucene.Net.Search</Namespace>
  <Namespace>Lucene.Net.Search.Function</Namespace>
  <Namespace>Lucene.Net.Search.Payloads</Namespace>
  <Namespace>Lucene.Net.Search.Spans</Namespace>
  <Namespace>Lucene.Net.Store</Namespace>
  <Namespace>Lucene.Net.Support</Namespace>
  <Namespace>Lucene.Net.Support.Compatibility</Namespace>
  <Namespace>Lucene.Net.Util</Namespace>
  <Namespace>Lucene.Net.Util.Cache</Namespace>
</Query>

void Main()
{
	var provider = new ItemProvider();
	var list = provider.GetList(100);

//	list.Dump();

	var searchEngine = new SearchEngine();
	searchEngine.BuildCache(list);
	
	var result = searchEngine.Search("Item");
	result.Dump();
}

// Define other methods and classes here
public class Item
{
	public int Id { get; set; }

	public string Name { get; set; }
}

public class ItemProvider
{
	public IEnumerable<Item> GetList(int count)
	{
		return new Builder().CreateListOfSize<Item>(count)
		.All()
		.Do(i => i.Name = $"Property {i.Id}")
		.Random(new Random().Next(count))
//		.Do(i => i.Name = $"Item {i.Id}")
		.Build();
	}
}

public class SearchEngine
{
	public string _directoryPath = "C:\\temp\\lucene_index";
	
	public void BuildCache(IEnumerable<Item> list)
	{
		using (var directory = FSDirectory.Open(_directoryPath))
		{
			using (var analyser = new SimpleAnalyzer())
			{
				using (var indexer = new Lucene.Net.Index.IndexWriter(directory, analyser, IndexWriter.MaxFieldLength.UNLIMITED))
				{
					foreach (var item in list)
					{
						var document = new Document();
						document.Add(new Field("id", item.Id.ToString(), Field.Store.YES, Field.Index.ANALYZED));
						document.Add(new Field("name", item.Name, Field.Store.YES, Field.Index.NO));
						indexer.AddDocument(document);
					}
				}
			}
		}
	}

	public IEnumerable<int> Search(string name)
	{
		using (var directory = FSDirectory.Open(_directoryPath))
		{
			var query = new TermQuery(new Term("name", name));

			using (var searcher = new IndexSearcher(directory))
			{
				var hits = searcher.Search(query, null, 1000, Sort.RELEVANCE);
				return hits.ScoreDocs.Select(d => d.Doc);
			}

		}
	}
}
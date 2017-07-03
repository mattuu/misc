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


	var directoryPath = "C:\\temp\\lucene_index";

	var cacheBuilder = new CacheBuilder(directoryPath);
	cacheBuilder.Build(list);


	var searchEngine = new SearchEngine(directoryPath);

	//	searchEngine.List().Count().Dump("Index Count");

	var result = searchEngine.Search("name", "item");
	result.Dump("Results");

	list.Dump("List");
}

// Define other methods and classes here
public class Item
{
	public int Id { get; set; }

	public string Name { get; set; }

	public int RandomNumber { get; set; }
}

public class ItemProvider
{
	public IEnumerable<Item> GetList(int count)
	{
		return new Builder().CreateListOfSize<Item>(count)
		.All()
		.Do(i =>
		{
			i.Name = $"Property {i.Id}";
			i.RandomNumber = new RandomGenerator().Byte();
		})
		.Random(new Random().Next(count))
		.Do(i => i.Name = $"Item {i.Id}")
		.Build();
	}
}

public class CacheBuilder
{
	private string _directoryPath;

	public CacheBuilder(string directoryPath)
	{
		_directoryPath = directoryPath;
	}

	public void Build(IEnumerable<Item> list)
	{
		using (var directory = FSDirectory.Open(_directoryPath))
		{
			using (var analyser = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
			{
				using (var indexer = new Lucene.Net.Index.IndexWriter(directory, analyser, IndexWriter.MaxFieldLength.UNLIMITED))
				{
					indexer.DeleteAll();

					foreach (var item in list)
					{
						var document = new Document();
						document.Add(new Field("id", item.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
						document.Add(new Field("name", item.Name, Field.Store.YES, Field.Index.ANALYZED));
						document.Add(new Field("random_number", item.RandomNumber.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

						indexer.AddDocument(document);
					}

					indexer.Optimize();
				}
			}
		}
	}
}

public class SearchEngine
{
	private string _directoryPath;

	public SearchEngine(string directoryPath)
	{
		_directoryPath = directoryPath;
	}

	public IEnumerable<int> Search(string searchField, string searchQuery)
	{
		using (var directory = FSDirectory.Open(_directoryPath))
		{
			var hits_limit = 1000;
			//			var query = new TermQuery(new Term(searchField, searchTerm));
			ScoreDoc[] scoreDocs;
			using (var searcher = new IndexSearcher(directory))
			{
				using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
				//				var hits = searcher.Search(query, null, 1000, Sort.RELEVANCE);
				{
					var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, searchField, analyzer);

					Term term = new Term(searchField, searchQuery);
					Query query = new TermQuery(term);

					MapFieldSelector field = new MapFieldSelector("name");
					TopDocs hits = searcher.Search(query, hits_limit);

					//					hits.Dump("Hits");

					scoreDocs = hits.ScoreDocs;
					scoreDocs.Dump();
				}
			}
			using (var reader = IndexReader.Open(directory, false))
			{
				return scoreDocs.Select(d => reader.Document(d.Doc))
				.Select(doc => int.Parse(doc.Get("id")));
			}
		}
	}

	public IEnumerable<int> List()
	{
		using (var directory = FSDirectory.Open(_directoryPath))
		{
			using (var searcher = new IndexSearcher(directory, false))
			{
				using (var reader = IndexReader.Open(directory, false))
				{
					var term = reader.TermDocs();
					var docs = new List<Document>();
					while (term.Next())
					{
						docs.Add(searcher.Doc(term.Doc));
						//						Co	nsole.WriteLine(searcher.Doc(term.Doc));
					}
					return docs.Select(d => int.Parse(d.Get("name")));
				}
			}
		}
	}
}
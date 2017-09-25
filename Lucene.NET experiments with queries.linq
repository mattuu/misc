<Query Kind="Program">
  <Output>DataGrids</Output>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.Entity.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Expressions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Parallel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Queryable.dll</Reference>
  <NuGetReference>Lucene.Net</NuGetReference>
  <NuGetReference>NBuilder</NuGetReference>
  <Namespace>Lucene.Net.Analysis</Namespace>
  <Namespace>Lucene.Net.Analysis.Standard</Namespace>
  <Namespace>Lucene.Net.Documents</Namespace>
  <Namespace>Lucene.Net.Index</Namespace>
  <Namespace>Lucene.Net.QueryParsers</Namespace>
  <Namespace>Lucene.Net.Search</Namespace>
  <Namespace>Lucene.Net.Store</Namespace>
  <Namespace>FizzWare.NBuilder</Namespace>
  <AppConfig>
    <Path>C:\GitSrc\J2BI.Holidays.PCPS\src\J2BI.Holidays.PCPS.Content.Api\Web.config</Path>
  </AppConfig>
</Query>

void Main()
{
	var data = new Builder().CreateListOfSize<Property>(1000)
		.All()
		.Do(p => p.Name = new RandomGenerator().NextString(0, 100).Trim())
	.Build();

	using (var analyzer = new SimpleAnalyzer())
	{
		AddOrUpdateIndex(analyzer, data.ToArray());



		//		var query = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Name", analyzer).Parse("et");
		var query = new WildcardQuery(new Term("Name", "lorem ipsum*"));

		Search(query).Dump(nameof(QueryParser));
		Search(new MatchAllDocsQuery()).Dump(nameof(MatchAllDocsQuery));
	}
	//	Search(new MatchAllDocsQuery()).Dump(nameof(MatchAllDocsQuery));
}

IEnumerable Search(Query query)
{
	using (var searcher = new IndexSearcher(Directory))
	{
		//		Query query = new MatchAllDocsQuery();
		//		query = new WildcardQuery(new Term("Name", "Nic*"));
		//

		var hits = searcher.Search(query, int.MaxValue).ScoreDocs;
		//
		return hits.Select(h =>
		{
			var doc = searcher.Doc(h.Doc);
			return doc.GetField("Name")?.StringValue;

		}).ToList();
	}
}


private FSDirectory _directoryTemp;

// Define other methods and classes here
private FSDirectory Directory
{
	get
	{
		if (_directoryTemp == null)
		{
			_directoryTemp = FSDirectory.Open(new DirectoryInfo(LuceneDir));
		}
		if (IndexWriter.IsLocked(_directoryTemp))
		{
			IndexWriter.Unlock(_directoryTemp);
		}

		var lockFilePath = Path.Combine(LuceneDir, "write.lock");

		if (File.Exists(lockFilePath))
		{
			File.Delete(lockFilePath);
		}
		return _directoryTemp;
	}
}

internal string LuceneDir => Path.Combine(@"C:\temp\experiment", "lucene_index");

public void AddOrUpdateIndex(Analyzer analyzer, params Property[] data)
{
	using (analyzer)
	{
		using (var writer = new IndexWriter(Directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
		{
			foreach (var property in data)
			{
				var searchQuery = new TermQuery(new Term("OrlandoId", property.OrlandoId.ToString()));
				writer.DeleteDocuments(searchQuery);

				var doc = new Document();

				doc.Add(new Field("OrlandoId", property.OrlandoId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

				if (!string.IsNullOrEmpty(property.Name))
					doc.Add(new Field("Name", property.Name, Field.Store.YES, Field.Index.NOT_ANALYZED));

				if (doc != null)
					writer.AddDocument(doc);
			}
		}
	}
}

public class Property
{
	public int OrlandoId { get; set; }

	public string Name { get; set; }

	public string Gateway { get; set; }

	public string Resort { get; set; }
}
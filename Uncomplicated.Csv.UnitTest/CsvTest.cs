using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Uncomplicated.Csv;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Uncomplicated.Csv.UnitTest
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestReadQualifierNone()
		{

			string csv = @"1,2,3,NULL
a1,""b,c"",""NULL"",d1
"""","""","""",""""
x1,NULL,y1,""z1""
,,,
""fghi"",""k
lmop"",""qrsu"",""vwxy""
,,,
,,,";

			string expectedResult = @"[1],[2],[3],NULL
[a1],[""b],[c""],[""NULL""],[d1]
[""""],[""""],[""""],[""""]
[x1],NULL,[y1],[""z1""]
[],[],[],[]
[""fghi""],[""k]
[lmop""],[""qrsu""],[""vwxy""]
[],[],[],[]
[],[],[],[]";

			var settings = new CsvReaderSettings()
			{
				ColumnSeparator = ',',
				Encoding = Encoding.UTF8,
				NullValue = "NULL",
				TextQualification = CsvTextQualification.None,
				TextQualifier = '"'
			};

			var stream = new MemoryStream(settings.Encoding.GetBytes(csv));
			var result = new StringBuilder();

			using (var reader = new CsvReader(stream, settings))
			{
				string[] line = null;
				while ((line = reader.Read()) != null)
				{
					result.AppendLine(string.Join(",", line.Select(c => c == null ? "NULL" : string.Format("[{0}]", c))));
				}
			}

			Console.WriteLine("Expects:");
			Console.WriteLine(expectedResult);
			Console.WriteLine();
			Console.WriteLine("Result:");
			Console.WriteLine(result);

			Assert.AreEqual(result.ToString().Trim(), expectedResult);
		}

		[TestMethod]
		public void TestRead()
		{

			string expectedResult = @"""1"",""2"",""3"",NULL
""a1"",""b,c"",""NULL"",""d1""
"""","""","""",""""
""x1"",NULL,""y1"",""z1""
"""","""","""",""""
""fghi"",""k
lmop"",""qrsu"",""vwxy""
"""","""","""",""""
"""","""","""",""""";

			string csv = @"1,2,3,NULL
a1,""b,c"",""NULL"",d1
"""","""","""",""""
x1,NULL,y1,""z1""
,,,
""fghi"",""k
lmop"",""qrsu"",""vwxy""
,,,
,,,";

			var settings = new CsvReaderSettings()
			{
				ColumnSeparator = ',',
				Encoding = Encoding.UTF8,
				NullValue = "NULL",
				TextQualification = CsvTextQualification.AsNeeded,
				TextQualifier = '"'
			};

			var stream = new MemoryStream(settings.Encoding.GetBytes(csv));

			var result = new StringBuilder();

			using (var reader = new CsvReader(stream, settings))
			{
				string[] line = null;
				while ((line = reader.Read()) != null)
				{
					result.AppendLine(string.Join(",", line.Select(c => c == null ? "NULL" : string.Format("\"{0}\"", c))));
				}
			}

			Console.WriteLine("Expects:");
			Console.WriteLine(expectedResult);
			Console.WriteLine();
			Console.WriteLine("Result:");
			Console.WriteLine(result);

			Assert.AreEqual(result.ToString().Trim(), expectedResult);

		}


		[TestMethod]
		public void TestWrite()
		{

			string expectedResult = @"""1"",""2"",""3"",NULL
""a1"",""b1"",""NULL"",""c1""
"""","""","""",""""
""x1"",NULL,""y1"",""z1""
"""","""","""",""""
""fghi"",""jk,lm"",""no""""pq"",""rstu""
"""","""","""",""""";

			var csv = new List<string[]>() {
				new string[]{ "1",		"2",	"3",	null },
				new string[]{ "a1",		"b1",	"NULL",	"c1" },
				new string[]{ "",		"",		"",		"" },
				new string[]{ "x1",		null,	"y1",	"z1" },
				new string[]{ "",		"",		"",		"" },
				new string[]{ "fghi",	"jk,lm",	"no\"pq",	"rstu" },
				new string[]{ "",		"",		"",		"" }
			};

			var settings = new CsvWriterSettings()
			{
				ColumnSeparator = ',',
				Encoding = new UTF8Encoding(false),
				NullValue = "NULL",
				TextQualification = CsvTextQualification.Always,
				TextQualifier = '"'
			};

			var stream = new MemoryStream();


			using (var writer = new CsvWriter(stream, settings))
			{
				foreach(var line in csv)
				{
					writer.WriteRow(line.Select(s=>s));
				}
			}

			string result = settings.Encoding.GetString(stream.ToArray());

			Console.WriteLine("Expects:");
			Console.WriteLine(expectedResult);
			Console.WriteLine();
			Console.WriteLine("Result:");
			Console.WriteLine(result);

			Assert.AreEqual(result.Trim(), expectedResult);

		}
	}
}

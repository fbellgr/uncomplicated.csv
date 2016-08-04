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
		public void TestRead()
		{

			string expectedResult = @"""1"",""2"",""3"",NULL
""aa"",""b,b"",""NULL"",""cc""
"""","""","""",""""
""xx"",NULL,""yy"",""zz""
"""","""","""",""""
""ffff"",""g
ggg"",""hhhh"",""iiii""
"""","""","""",""""
"""","""","""",""""";

			string csv = @"1,2,3,NULL
aa,""b,b"",""NULL"",cc
"""","""","""",""""
xx,NULL,yy,""zz""
,,,
""ffff"",""g
ggg"",""hhhh"",""iiii""
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
""aa"",""bb"",""NULL"",""cc""
"""","""","""",""""
""xx"",NULL,""yy"",""zz""
"""","""","""",""""
""ffff"",""gggg"",""hhhh"",""iiii""
"""","""","""",""""";

			var csv = new List<string[]>() {
				new string[]{ "1",		"2",	"3",	null },
				new string[]{ "aa",		"bb",	"NULL",	"cc" },
				new string[]{ "",		"",		"",		"" },
				new string[]{ "xx",		null,	"yy",	"zz" },
				new string[]{ "",		"",		"",		"" },
				new string[]{ "ffff",	"gggg",	"hhhh",	"iiii" },
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
					writer.WriteRow(line);
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

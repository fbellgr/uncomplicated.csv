using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Uncomplicated.Csv;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Uncomplicated.Csv.UnitTest
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestReadQualifierNone()
		{

			var settings = new CsvReaderSettings()
			{
				ColumnSeparator = ',',
				Encoding = Encoding.UTF8,
				NullValue = "NULL",
				TextQualification = CsvTextQualification.None,
				TextQualifier = '"'
			};


			var rawExpectedResult = new StreamReader(
				Assembly.GetExecutingAssembly().GetManifestResourceStream("Uncomplicated.Csv.UnitTest.test-read-no-qualifiers.json")
			, true).ReadToEnd();

			var jsonExpectedResult = JToken.Parse(rawExpectedResult);

			var csvStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Uncomplicated.Csv.UnitTest.test-read.csv");

			var lines = new List<string[]>();
			using (var reader = new CsvReader(csvStream, settings))
			{
				string[] line = null;
				while ((line = reader.Read()) != null)
				{
					lines.Add(line);
				}
			}

			string result = JsonConvert.SerializeObject(lines, Formatting.Indented);
			string expectedResult = jsonExpectedResult.ToString(Formatting.Indented);

			Console.WriteLine("Expects:");
			Console.WriteLine(expectedResult);
			Console.WriteLine();
			Console.WriteLine("Result:");
			Console.WriteLine(result);

			Assert.AreEqual(result, expectedResult);
		}

		[TestMethod]
		public void TestRead()
		{


			var settings = new CsvReaderSettings()
			{
				ColumnSeparator = ',',
				Encoding = Encoding.UTF8,
				NullValue = "NULL",
				TextQualification = CsvTextQualification.AsNeeded,
				TextQualifier = '"'
			};


			var rawExpectedResult = new StreamReader(
				Assembly.GetExecutingAssembly().GetManifestResourceStream("Uncomplicated.Csv.UnitTest.test-read-with-qualifiers.json")
			, true).ReadToEnd();

			var jsonExpectedResult = JToken.Parse(rawExpectedResult);

			var csvStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Uncomplicated.Csv.UnitTest.test-read.csv");

			var lines = new List<string[]>();
			using (var reader = new CsvReader(csvStream, settings))
			{
				string[] line = null;
				while ((line = reader.Read()) != null)
				{
					lines.Add(line);
				}
			}

			string result = JsonConvert.SerializeObject(lines, Formatting.Indented);
			string expectedResult = jsonExpectedResult.ToString(Formatting.Indented);

			Console.WriteLine("Expects:");
			Console.WriteLine(expectedResult);
			Console.WriteLine();
			Console.WriteLine("Result:");
			Console.WriteLine(result);

			Assert.AreEqual(result, expectedResult);
		}


		[TestMethod]
		public void TestWrite()
		{

			var expectedResult = new StreamReader(
				Assembly.GetExecutingAssembly().GetManifestResourceStream("Uncomplicated.Csv.UnitTest.test-write-with-qualifiers.csv")
			, true).ReadToEnd().Trim();


			var src = new StreamReader(
				Assembly.GetExecutingAssembly().GetManifestResourceStream("Uncomplicated.Csv.UnitTest.test-write-with-qualifiers.json")
			, true).ReadToEnd();
			var jsonSrc = JToken.Parse(src);

			var settings = new CsvWriterSettings()
			{
				ColumnSeparator = ',',
				Encoding = new UTF8Encoding(false),
				NullValue = "NULL",
				TextQualification = CsvTextQualification.AsNeeded,
				TextQualifier = '"',
				ShouldUseTextQualifiers = (defaultAction, cell) => defaultAction || cell.Contains("a")
			};

			var stream = new MemoryStream();


			using (var writer = new CsvWriter(stream, settings))
			{
				foreach (var line in jsonSrc)
				{
					var arr = line.Select(s => s.Type == JTokenType.Null ? null : s.ToString()).ToList();
					writer.WriteRow(arr);
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


		[TestMethod]
		public void TestWriteQualifierNone()
		{

			var expectedResult = new StreamReader(
				Assembly.GetExecutingAssembly().GetManifestResourceStream("Uncomplicated.Csv.UnitTest.test-write-no-qualifiers.csv")
			, true).ReadToEnd().Trim();


			var src = new StreamReader(
				Assembly.GetExecutingAssembly().GetManifestResourceStream("Uncomplicated.Csv.UnitTest.test-write-no-qualifiers.json")
			, true).ReadToEnd();
			var jsonSrc = JToken.Parse(src);

			var settings = new CsvWriterSettings()
			{
				ColumnSeparator = ',',
				Encoding = new UTF8Encoding(false),
				NullValue = "NULL",
				TextQualification = CsvTextQualification.None,
				TextQualifier = '"'
			};

			var stream = new MemoryStream();


			using (var writer = new CsvWriter(stream, settings))
			{
				foreach (var line in jsonSrc)
				{
					var arr = line.Select(s => s.Type == JTokenType.Null ? null : s.ToString()).ToList();
					writer.WriteRow(arr);
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

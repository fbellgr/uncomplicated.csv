using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Uncomplicated.Csv
{

	internal class CsvUtil
	{
		internal static void ValidateCsvSettings(string nullValue, char separator, char textQualifier)
		{
			if (!string.IsNullOrWhiteSpace(nullValue)
				&& nullValue.Intersect(new char[] { '\r', '\n', separator, textQualifier }).Any())
			{
				throw new CsvException(string.Format("NullValue cannot contain '\\r', '\\n', '{0}', '{1}'.", separator, textQualifier));
			}
		}
	}
}

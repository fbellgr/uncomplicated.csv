using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uncomplicated.Csv
{
	public class CsvException : Exception
	{
		public CsvException(string message)
			: base(message)
		{

		}
	}
}

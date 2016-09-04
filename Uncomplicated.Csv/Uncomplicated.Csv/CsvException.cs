using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uncomplicated.Csv
{
	/// <summary>
	/// Generic csv operation exception
	/// </summary>
	public class CsvException : Exception
	{
		internal CsvException(string message)
			: base(message)
		{

		}
	}
}

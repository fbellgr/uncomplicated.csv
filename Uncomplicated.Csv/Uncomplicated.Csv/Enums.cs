using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uncomplicated.Csv
{
	public enum CsvNewLineMode
	{
		Windows, Unix, OldMac
	}

	public enum CsvTextQualification
	{
		Always, AsNeeded, None
	}
}

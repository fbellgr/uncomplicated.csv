using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uncomplicated.Csv
{
	/// <summary>
	/// End of line specification.
	/// </summary>
	public enum CsvNewLineMode
	{
		/// <summary>
		/// {CR}{LF}
		/// </summary>
		Windows,
		/// <summary>
		/// {LF}
		/// </summary>
		Unix,
		/// <summary>
		/// {CR} 
		/// </summary>
		OldMac
	}

	/// <summary>
	/// Text qualification strategies
	/// </summary>
	public enum CsvTextQualification
	{
		/// <summary>
		/// When writing, text qualification is always used.
		/// When reading, the parser will handle text qualifiers as needed.
		/// </summary>
		Always,
		/// <summary>
		/// When writing, text qualification is used if the content of the cell requires it.
		/// When reading, the parser will handle text qualifiers as needed.
		/// </summary>
		AsNeeded,
		/// <summary>
		/// Text qualifiers are ignored. Period.
		/// </summary>
		None
	}
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uncomplicated.Csv.Collection
{


	/// <summary>
	/// Fake stack. Does not actually pop for better performane.
	/// </summary>
	public class FakeStack
	{
		private List<string> _list = new List<string>();
		private int _currentPosition = -1;
		public int _size = 0;

		public int Count { get { return _size; } }

		public void Push(char c)
		{
			Push(new string(c, 1));
		}

		public void Push(string str)
		{
			++_currentPosition;
			++_size;
			if (_currentPosition == _list.Count)
			{
				_list.Add(str);
			}
			else
			{
				_list[_currentPosition] = str;
			}
		}

		public string Pop()
		{
			string popped = null;
			if (_currentPosition > -1)
			{
				popped = _list[_currentPosition];
				--_currentPosition;
				--_size;
			}
			return popped;
		}

		public object Peek()
		{
			string peeked = null;
			if (_currentPosition > -1)
			{
				peeked = _list[_currentPosition];
			}
			return peeked;
		}

		public bool Peek(char c)
		{
			bool match = false;
			string peeked = null;
			if (_currentPosition > -1)
			{
				peeked = _list[_currentPosition];
				match = peeked.Length == 1 && c.Equals(peeked[0]);
			}
			return match;
		}

	}
}

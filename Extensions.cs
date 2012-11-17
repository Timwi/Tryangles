using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tryangles
{
	static class Extensions
	{
		/// <summary>
		/// Returns an enumeration of tuples containing all consecutive pairs of the elements.
		/// </summary>
		/// <param name="source">The input enumerable.</param>
		public static IEnumerable<Tuple<T, T>> ConsecutivePairs<T>(this IEnumerable<T> source)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			return consecutivePairsIterator(source);
		}
		private static IEnumerable<Tuple<T, T>> consecutivePairsIterator<T>(IEnumerable<T> source)
		{
			using (var enumer = source.GetEnumerator())
			{
				bool any = enumer.MoveNext();
				if (!any)
					yield break;
				T first = enumer.Current;
				T last = enumer.Current;
				while (enumer.MoveNext())
				{
					yield return new Tuple<T, T>(last, enumer.Current);
					last = enumer.Current;
				}
			}
		}
	}
}

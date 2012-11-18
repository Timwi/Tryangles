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

		/// <summary>
		/// Enumerates the items of this collection, skipping the last <paramref name="count"/> items. Note that the
		/// memory usage of this method is proportional to <paramref name="count"/>, but the source collection is
		/// only enumerated once, and in a lazy fashion. Also, enumerating the first item will take longer than
		/// enumerating subsequent items.
		/// </summary>
		public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int count)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", "count cannot be negative.");
			if (count == 0)
				return source;

			var collection = source as ICollection<T>;
			if (collection != null)
				return collection.Take(Math.Max(0, collection.Count - count));

			return skipLastIterator(source, count);
		}
		private static IEnumerable<T> skipLastIterator<T>(IEnumerable<T> source, int count)
		{
			var queue = new T[count];
			int headtail = 0; // tail while we're still collecting, both head & tail afterwards because the queue becomes completely full
			int collected = 0;

			foreach (var item in source)
			{
				if (collected < count)
				{
					queue[headtail] = item;
					headtail++;
					collected++;
				}
				else
				{
					if (headtail == count)
						headtail = 0;
					yield return queue[headtail];
					queue[headtail] = item;
					headtail++;
				}
			}
		}

		public static void Swap(ref int a, ref int b)
		{
			var temp = a;
			a = b;
			b = temp;
		}

		public static int GCD(int a, int b)
		{
			if (b == 0)
				return a;
			else
				return GCD(b, a % b);
		}
	}
}

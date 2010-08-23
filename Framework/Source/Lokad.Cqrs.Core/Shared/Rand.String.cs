#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Globalization;
using System.Text;
using System.Linq;

namespace Lokad
{
	partial class Rand
	{
		/// <summary>
		/// Helper random methods related to strings
		/// </summary>
		public static class String
		{
			static readonly string[] Words;
			static readonly string[] WordBase = new[]
				{
					"2 accumsan", "7 accusam", "2 ad", "2 adipiscing", "2 aliquam", "2 aliquip", "7 aliquyam", "13 amet", "1 assum",
					"9 at", "2 augue", "3 autem", "2 blandit", "7 clita", "2 commodo", "1 congue", "2 consectetuer", "5 consequat",
					"7 consetetur", "1 cum", "2 delenit", "15 diam", "2 dignissim", "16 dolor", "14 dolore", "7 dolores", "1 doming",
					"5 duis", "7 duo", "9 ea", "7 eirmod", "1 eleifend", "2 elit", "7 elitr", "2 enim", "7 eos", "9 erat", "2 eros",
					"3 esse", "7 est", "32 et", "3 eu", "2 euismod", "3 eum", "2 ex", "2 exerci", "1 facer", "2 facilisi",
					"3 facilisis", "2 feugait", "3 feugiat", "7 gubergren", "3 hendrerit", "1 id", "3 illum", "1 imperdiet", "6 in",
					"7 invidunt", "14 ipsum", "3 iriure", "2 iusto", "7 justo", "7 kasd", "7 labore", "2 laoreet", "1 liber",
					"2 lobortis", "14 lorem", "2 luptatum", "9 magna", "1 mazim", "2 minim", "3 molestie", "1 nam", "2 nibh", "1 nihil"
					, "2 nisl", "7 no", "1 nobis", "2 nonummy", "7 nonumy", "2 nostrud", "5 nulla", "2 odio", "1 option", "1 placerat",
					"1 possim", "2 praesent", "2 qui", "2 quis", "1 quod", "7 rebum", "7 sadipscing", "7 sanctus", "7 sea", "15 sed",
					"13 sit", "1 soluta", "7 stet", "2 suscipit", "7 takimata", "2 tation", "2 te", "8 tempor", "2 tincidunt",
					"2 ullamcorper", "13 ut", "6 vel", "3 velit", "2 veniam", "9 vero", "6 voluptua", "2 volutpat", "3 vulputate",
					"2 wisi", "2 zzril",
				};

			static readonly char[] PunctuationBase = ".............!?".ToArray();

			static String()
			{
				Words = WordBase.SelectMany(w =>
					{
						var split = w.Split(' ');
						return Range.Repeat(split[1], int.Parse(split[0]));
					}).ToArray();
			}

			/// <summary>
			/// Gets the Lorem Ipsum sentence with random word count.
			/// </summary>
			/// <param name="lowerBound">The lower bound for the word count (inclusive).</param>
			/// <param name="upperBound">The upper bound for the word count (exclusive).</param>
			/// <returns>random sentence of Lorem ipsum</returns>
			public static string NextSentence(int lowerBound, int upperBound)
			{
				var builder = new StringBuilder();
				var count = Next(lowerBound, upperBound);

				AppendSentence(builder, count);

				return builder.ToString();
			}

			/// <summary>
			/// Gets random word from the Lorem Ipsum dictionary.
			/// </summary>
			/// <returns>random word from the Lorem Ipsum dictionary</returns>
			public static string NextWord()
			{
				return NextItem(Words);
			}

			static void AppendSentence(StringBuilder builder, int count)
			{
				for (int i = 0; i < count; i++)
				{
					var value = NextWord();
					if (i == 0)
					{
						value = char.ToUpper(value[0], CultureInfo.InvariantCulture) + value.Remove(0, 1);
					}
					else
					{
						value = ' ' + value;
					}
					builder.Append(value);
				}
				if (count > 0)
				{
					builder.Append(NextItem(PunctuationBase));
				}
			}

			/// <summary>
			/// Gets the Lorem ipsum text with the random word count.
			/// </summary>
			/// <param name="lowerBound">The lower bound for the word count (inclusive).</param>
			/// <param name="upperBound">The upper bound for the word count (exclusive).</param>
			/// <returns>random text of Lorem Ipsum</returns>
			public static string NextText(int lowerBound, int upperBound)
			{
				int count = Next(lowerBound, upperBound);
				const int wordsInSentenceUpper = 10;
				var builder = new StringBuilder();
				while (count > 0)
				{
					var wordCount = Next(1, Math.Min(wordsInSentenceUpper, count));
					AppendSentence(builder, wordCount);

					count -= wordCount;

					if (count > 0)
					{
						if (NextBool(0.08D))
						{
							builder.AppendLine();
						}
						else builder.Append(' ');
					}
				}
				return builder.ToString();
			}
		}
	}
}
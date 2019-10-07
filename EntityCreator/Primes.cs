using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EntityCreator {
	public static class Primes {
		private static readonly Random random;
		private static readonly int[] PrimeList;
		private static readonly IEnumerable<int> BigPrimeList;
		private static readonly IEnumerable<int> LittlePrimeList;

		static Primes() {
			random = new Random();

			PrimeList = new StreamReader(
				Assembly
					.GetExecutingAssembly()
					.GetManifestResourceStream( "EntityCreator.primes.txt" )
			)
			.ReadToEnd()
			.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
			.Select( s => int.Parse( s.Trim() ) )
			.ToArray();

			BigPrimeList =
				PrimeList.Where( p => p > 10_000  );

			LittlePrimeList =
				PrimeList.Where( p => p < 10_000 );
		}

		public static int Random =>
			PrimeList[random.Next( 0, PrimeList.Length )];

		public static int BigPrime =>
			BigPrimeList
				.Skip( random.Next( BigPrimeList.Count() ) )
				.Take( 1 )
				.Single();

		public static int LittlePrime =>
			LittlePrimeList
				.Skip( random.Next( LittlePrimeList.Count() ) )
				.Take( 1 )
				.Single();
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EntityCreator {
	public static class Primes {
		private static readonly Random random;
		private static readonly int[] PrimeList;

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
		}

		public static int Random =>
			PrimeList[random.Next( 0, PrimeList.Length )];
	}
}

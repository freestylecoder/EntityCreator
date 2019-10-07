using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EntityCreator {
	public class Program {
		private static string JoinWithNewLines( IEnumerable<string> lines ) =>
			string.Join(
				Environment.NewLine,
				lines
			);

		private static string PrintUsings( IEnumerable<string> Usings ) =>
			JoinWithNewLines( Usings );

		private static string PrintFields( IEnumerable<(string DataType, string FieldName)> Fields ) =>
			JoinWithNewLines( Fields.Select( x => $"		public readonly {x.DataType} {x.FieldName};" ) );

		private static string DefaultCtor( string Class, IEnumerable<(string DataType,string FieldName)> Fields ) =>
$@"		public {Class} (
{JoinWithNewLines( Fields.Select( x => $"			{x.DataType} {x.FieldName.ToLower()} = default," ) ).TrimEnd( ',' )}
		) {{
{JoinWithNewLines( Fields.Select( x => $"			this.{x.FieldName} = {x.FieldName.ToLower()};" ) )}
		}}";

		private static string CopyCtor( string Class, IEnumerable<(string DataType, string FieldName)> Fields ) =>
$@"		public {Class}( {Class} copy )
			: this( {string.Join( ", ", Fields.Select(x => $"copy.{x.FieldName}" ) )} ) {{ }}";

		private static string Withers( string Class, IEnumerable<(string DataType, string FieldName)> Fields ) {
			string newCopy = $"new {Class}( { string.Join( ", ", Fields.Select( x => $"this.{x.FieldName}" ) ) } );";

			return JoinWithNewLines(
				Fields
					.Select( x =>
$@"		public {Class} With{x.FieldName}( {x.DataType} {x.FieldName.ToLower()} ) =>
			{newCopy.Replace( $"this.{x.FieldName}", x.FieldName.ToLower() )}"
					)
			);
		}

		private static string GetHashCode( IEnumerable<(string DataType, string FieldName)> Fields ) =>
$@"		private int? _hash = null;
		private const int _bigPrime = {Primes.BigPrime};
		private const int _littlePrime = {Primes.LittlePrime};
		public override int GetHashCode() {{
			Func<object, int> SafeHashCode = ( obj ) =>
				obj is object ish
				? ish.GetHashCode()
				: 0;

			if( !_hash.HasValue ) {{
				unchecked {{
					_hash = _bigPrime;

{string.Join( "\r\n", Fields.Select( x => $"					_hash = _hash * _littlePrime + SafeHashCode( this.{x.FieldName} );" ) )}
				}}
			}}

			return _hash.Value;
		}}";

		private static string EquatableEquals( string Class, IEnumerable<(string DataType, string FieldName)> Fields ) =>
$@"		public bool Equals( {Class} that ) {{
			if( ReferenceEquals( that, null ) )
				return false;

			return
				ReferenceEquals( this, that )
				|| (
					this.GetHashCode() == that.GetHashCode()
{string.Join( "\r\n", Fields.Select( x => $"					&& this.{x.FieldName} == that.{x.FieldName}" ) )}
				);
		}}";

		static void Main( string[] args ) {
			IEnumerable<string> lines = File.ReadAllLines( args[0] );

			IEnumerable<string> Usings = lines
				.Where( line => line.StartsWith( "using " ) );

			string Namespace = lines
				.Where( line => line.StartsWith( "namespace " ) )
				.Single()
				.Split( ' ' )
				[1];

			string Class = lines
				.Where( line => line.Contains( "class " ) )
				.Single()
				.Split( new[] { "class " }, StringSplitOptions.RemoveEmptyEntries )
				[1]
				.Split()
				[0];

			IEnumerable<(string DataType, string FieldName)> Fields = lines
				.Where( line => line.Contains( "public readonly " ) )
				.Select( line =>
					line.Split( new[] { "public readonly ", " ", "\t", ";" }, StringSplitOptions.RemoveEmptyEntries )
				)
				.Select( x => (x[0], x[1]) );

			File.WriteAllText(
				args[0],
$@"{PrintUsings( Usings )}

namespace {Namespace} {{
	public class {Class} : IEquatable<{Class}> {{
{PrintFields( Fields )}

{DefaultCtor( Class, Fields )}

{CopyCtor( Class, Fields )}

{Withers( Class, Fields )}

		public override bool Equals( object obj ) {{
			if( obj is {Class} that )
				return this.Equals( that );

			return base.Equals( obj );
		}}

{GetHashCode( Fields )}

		public override string ToString() =>
			throw new NotImplementedException();

{EquatableEquals( Class, Fields )}

		public static bool operator ==( {Class} left, {Class} right ) =>
			ReferenceEquals( left, null )
				? ReferenceEquals( right, null )
				: left.Equals( right );

		public static bool operator !=( {Class} left, {Class} right ) =>
			!( left == right );
	}}
}}"
			);
		}
	}
}

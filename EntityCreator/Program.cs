﻿using System;
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

			StringBuilder sb = new StringBuilder();

			sb.AppendLine( 
$@"{JoinWithNewLines( Usings )}
namespace {Namespace} {{
	public class {Class} : IEquatable<{Class}> {{
{JoinWithNewLines( Fields.Select( x => $"		public readonly {x.DataType} {x.FieldName};" ) )}

		public {Class} (" );
			sb.AppendLine(
				string.Join(
					$",{Environment.NewLine}",
					Fields.Select( x => $"\t\t\t{x.DataType} {x.FieldName.ToLower()} = default" )
				)
			);

			sb.AppendLine( 
$@"		) {{
{JoinWithNewLines( Fields.Select( x => $"			this.{x.FieldName} = {x.FieldName.ToLower()};" ) )}
		}}

		public {Class}( {Class} copy )
			: this( {string.Join( ", ", Fields.Select( x => $"copy.{x.FieldName}" ) )} ) {{ }}

" );
			string newCopy = $"new {Class}( { string.Join( ", ", Fields.Select( x => $"this.{x.FieldName}" ) ) } );";
			Fields
				.Select( x => 
$@"		public {Class} With{x.FieldName}( {x.DataType} {x.FieldName.ToLower()} ) =>
			{newCopy.Replace( $"this.{x.FieldName}", x.FieldName.ToLower() )}"
				)
				.ToList()
				.ForEach( s => sb.AppendLine( s ) );
			sb.AppendLine();

			sb.AppendLine(
$@"		public override bool Equals( object obj ) {{
			if( obj is {Class} that )
				return this.Equals( that );

			return base.Equals( obj );
		}}" );
			sb.AppendLine();

			sb.AppendLine(
$@"		private int? _hash = null;
		public override int GetHashCode() {{
			Func<object, int> SafeHashCode = ( obj ) =>
				 obj is object ish
				 ? ish.GetHashCode()
				 : 0;

			if( !_hash.HasValue ) {{
				unchecked {{
					_hash = 3;

{ string.Join( "\r\n", Fields.Select( x => $"					_hash = _hash * 5 + SafeHashCode( {x.FieldName} );" ) )}
				}}
			}}

			return _hash.Value;
		}}

		public override string ToString() =>
			throw new NotImplementedException();

		public bool Equals( {Class} that ) {{
			if( ReferenceEquals( that, null ) )
				return false;

			return
				ReferenceEquals( this, that )
				|| (
					this.GetHashCode() == that.GetHashCode()
{ string.Join( "\r\n", Fields.Select( x => $"					&& this.{x.FieldName} == that.{x.FieldName}" ) )}
				);
		}}

		public static bool operator ==( {Class} left, {Class} right ) =>
			ReferenceEquals( left, null )
				? ReferenceEquals( right, null )
				: left.Equals( right );

		public static bool operator !=( {Class} left, {Class} right ) =>
			!( left == right );
	}}
}}" );

			File.WriteAllText( args[0], sb.ToString() );
			Console.WriteLine( File.ReadAllText( args[0] ) );
			Console.ReadKey();
		}
	}
}
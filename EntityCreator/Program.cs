using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EntityCreator {
	public class Program {
		private static readonly IDictionary<string, Type> BuildInTypes = new Dictionary<string,Type> {
				{ "bool",       typeof( bool )      },
				{ "byte",       typeof( byte )      },
				{ "sbyte",      typeof( sbyte )     },
				{ "char",       typeof( char )      },
				{ "decimal",    typeof( decimal )   },
				{ "double",     typeof( double )    },
				{ "float",      typeof( float )     },
				{ "int",        typeof( int )       },
				{ "uint",       typeof( uint )      },
				{ "long",       typeof( long )      },
				{ "ulong",      typeof( ulong )     },
				{ "object",     typeof( object )    },
				{ "short",      typeof( short )     },
				{ "ushort",     typeof( ushort )    },
				{ "string",     typeof( string )    }
			};
		
		private static IEnumerable<IDictionary<string,Type>> KnownTypesList;

		private static string JoinWithNewLines( IEnumerable<string> lines ) =>
			string.Join(
				Environment.NewLine,
				lines
			);

		private static string PrintUsings( IEnumerable<string> Usings ) =>
			JoinWithNewLines( Usings );

		private static string PrintFields( IEnumerable<FieldData> Fields ) =>
			JoinWithNewLines( Fields.Select( x =>
				x.IsEnumerable
				? $"		public readonly IEnumerable<{x.DataType}> {x.Name};{( x.Parameters.Any() ? $"	// {string.Join( " ", x.Parameters )}" : string.Empty )}"
				: $"		public readonly {x.DataType} {x.Name};{( x.Parameters.Any() ? $"	// {string.Join( " ", x.Parameters )}" : string.Empty )}"
			)
		);

		private static string DefaultCtor( string Class, IEnumerable<FieldData> Fields ) =>
$@"		public {Class} (
{JoinWithNewLines( Fields.Select( x =>
	x.IsEnumerable
	? $"			IEnumerable<{x.DataType}> {x.Name.ToLower()} = default,"
	: $"			{x.DataType} {x.Name.ToLower()} = default,"
) ).TrimEnd( ',' )}
		) {{
{JoinWithNewLines(
	Fields.Select( x => $"			this.{x.Name} = {GetCopyValue( x )};" )
)}
		}}";

		private static string GetCopyValue( FieldData fieldData ) {
			if( fieldData.IsEnumerable ) {
				if( fieldData.HasCopyCtor ) {
					return $"{fieldData.Name.ToLower()}?.Select( x => new {fieldData.DataType}( x ) ) ?? Enumerable.Empty<{fieldData.DataType}>()";
				} else if( fieldData.IsCloneable ) {
					return $"{fieldData.Name.ToLower()}?.Select( x => x.Clone() as {fieldData.DataType} ) ?? Enumerable.Empty<{fieldData.DataType}>()";
				} else {
					return $"this.{fieldData.Name} = {fieldData.Name.ToLower()}?.Select( x => x ) ?? Enumerable.Empty<{fieldData.DataType}>()";
				}
			} else {
				if( fieldData.HasCopyCtor ) {
					return $"new {fieldData.DataType}( {fieldData.Name.ToLower()} )";
				} else if( fieldData.IsCloneable ) {
					return $"{fieldData.Name.ToLower()}.Clone() as {fieldData.DataType}";
				} else {
					return fieldData.Name.ToLower();
				}
			}
		}

		private static string CopyCtor( string Class, IEnumerable<FieldData> Fields ) =>
$@"		public {Class}( {Class} copy )
			: this( {string.Join( ", ", Fields.Select( x => $"copy.{x.Name}" ) )} ) {{ }}";

		private static string Withers( string Class, IEnumerable<FieldData> Fields ) {
			string newCopy = $"new {Class}( { string.Join( ", ", Fields.Select( x => $"this.{x.Name}" ) ) } );";

			return JoinWithNewLines(
				Fields
					.Select( x =>
$@"		public {Class} With{x.Name}( {( x.IsEnumerable ? $"IEnumerable<{x.DataType}>" : x.DataType )} {x.Name.ToLower()} ) =>
			{newCopy.Replace( $"this.{x.Name},", $"{x.Name.ToLower()}," ).Replace( $"this.{x.Name} ", $"{x.Name.ToLower()} " )}"
					)
			);
		}

		private static string GetHashCode( IEnumerable<FieldData> Fields ) =>
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

{string.Join(
	"\r\n",
	Fields
		.OrderBy( x => x.IsEnumerable )
		.Select( x =>
			x.IsEnumerable
			? $@"
					foreach( {x.DataType} x in {x.Name}{( x.Parameters.Contains( "Ordered" ) ? string.Empty : $".OrderBy( y => y )" )} )
						_hash = _hash * _littlePrime + SafeHashCode( x );"
			: $"					_hash = _hash * _littlePrime + SafeHashCode( this.{x.Name} );"
		)
)}
				}}
			}}

			return _hash.Value;
		}}";

		private static string EquatableEquals( string Class, IEnumerable<FieldData> Fields ) =>
$@"		public bool Equals( {Class} that ) {{
			if( ReferenceEquals( that, null ) )
				return false;

			return
				ReferenceEquals( this, that )
				|| (
					this.GetHashCode() == that.GetHashCode()
{string.Join(
	"\r\n",
	Fields
		.OrderBy( x => x.IsEnumerable )
		.Select( x =>
			x.IsEnumerable
			? $@"					&& Enumerable.SequenceEqual(
						this.{x.Name}{( x.Parameters.Contains( "Ordered" ) ? string.Empty : $".OrderBy( y => y )" )},
						that.{x.Name}{( x.Parameters.Contains( "Ordered" ) ? string.Empty : $".OrderBy( y => y )" )}
					)"
			: $"					&& this.{x.Name} == that.{x.Name}"
		)
)}
				);
		}}";

		private static FieldData GetFieldData( FieldData fieldData ) {
			Func<Type, (bool, bool, bool)> GetTypeInformation = ( t ) =>
				t.FullName.Equals( "System.String" )
					? (false, false, false)
					: (
						t.IsValueType,
						t.GetInterfaces().Contains( typeof( ICloneable ) ),
						null != t.GetConstructor(
							BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
							null,
							new[] { t },
							null
						)
					);

			Type type = Type.GetType(
				Parser( fieldData.DataType ).ToString(),
				null,
				TypeResolver,
				false
			);

			bool isEnumerable =
				!type.FullName.Equals( "System.String" )
				&& type.GetInterfaces().Contains( typeof( IEnumerable ) );

			bool isValueType, isCloneable, hasCopyCtor;
			(isValueType, isCloneable, hasCopyCtor) = GetTypeInformation(
				isEnumerable
					? type.GenericTypeArguments.First()
					: type
			);

			return new FieldData(
					datatype: isEnumerable
						? Regex.Match( fieldData.DataType, "\\<(.*R?)\\>" ).Groups[1].Value
						: fieldData.DataType,
					name: fieldData.Name,
					isenumerable: isEnumerable,
					isvaluetype: isValueType,
					iscloneable: isCloneable,
					hascopyctor: hasCopyCtor,
					parameters: fieldData.Parameters
				);
		}

		private static Type TypeResolver( Assembly assembly, string typeName, bool caseSensitive ) =>
				BuildInTypes.ContainsKey( typeName )
					? BuildInTypes[typeName]
					: KnownTypesList
						.SelectMany( d => d.Values )
						.Where( t =>
							t.Name
								.Equals(
									typeName,
									caseSensitive
										? StringComparison.InvariantCulture
										: StringComparison.InvariantCultureIgnoreCase
								)
						)
						.FirstOrDefault();

		private static StringTree Parser( string typeName ) {
			typeName = typeName
				.Replace( "(", "ValueTuple<" )
				.Replace( ")", ">" )
				.Trim();

			if( !typeName.Contains( '<' ) ) {
				return new StringTree( typeName.Split( ' ' )[0] );
			}

			string genericTypeName = typeName.Substring( 0, typeName.IndexOf( '<' ) );
			string innerType = Regex.Match( typeName, "\\<(.*R?)\\>" ).Groups[1].Value;

			int nestingLevel = 0;
			int[] nesting = new int[innerType.Length];
			for( int index = 0; index < nesting.Length; ++index ) {
				switch( innerType[index] ) {
					case ',':
						nesting[index] = 0 == nestingLevel ? -1 : nestingLevel;
						break;

					case '<':
						nestingLevel++;
						goto default;

					case '>':
						nestingLevel--;
						goto default;

					default:
						nesting[index] = nestingLevel;
						break;
				}
			}

			if( nesting.Any( i => i == -1 ) ) {
				IEnumerable<int> indexes = nesting
					.Select( ( val, ind ) => -1 == val ? ind : -1 )
					.Where( val => -1 != val );

				foreach( int index in indexes ) {
					char[] temp = innerType.ToCharArray();
					temp[index] = '\0';
					innerType = new string( temp );

				}

				IEnumerable<StringTree> x = innerType.Split( new[] { '\0' } ).Select( s => Parser( s ) );
				return new StringTree( genericTypeName, x );
			} else {
				return new StringTree( genericTypeName, new[] { Parser( innerType ) } );
			}
		}

		private static int Main( string[] args ) {
			if( !args.Any() ) {
				Console.WriteLine();
				Console.WriteLine( "EntityCreator.exe" );
				Console.WriteLine( "Tools to add standard functionality to basic POCOs" );
				Console.WriteLine();
				Console.WriteLine( "Usage:" );
				Console.WriteLine( "EntityCreator.exe [drive:][path]filename [[drive:][path]assemblies[ ...]]" );
				Console.WriteLine();
				Console.WriteLine( "\tfilename\tFile to add functionality to" );
				Console.WriteLine( "\tassemblies\tExtra assemblies needed to resolve types" );
				Console.WriteLine();
				return 1;
			}

			IEnumerable<string> lines = File.ReadAllLines( args[0] );

			KnownTypesList = args
				.Skip( 1 )
				.Select( s => Assembly.LoadFile( s ) )
				.SelectMany( a => a.GetReferencedAssemblies().Select( an => Assembly.Load( an ) ).Prepend( a ) )
				.Distinct()
				.Select( a => a.GetTypes() )
				.Select( lot => lot.ToDictionary( t => t.FullName ) )
				.Prepend( BuildInTypes );

			if( 1 == KnownTypesList.Count() ) {
				KnownTypesList = Assembly
					.GetExecutingAssembly()
					.GetReferencedAssemblies()
					.Select( an => Assembly.Load( an ) )
					.Distinct()
					.Select( a => a.GetTypes() )
					.Select( lot => lot.ToDictionary( t => t.FullName ) )
					.Prepend( BuildInTypes );
			}

			IEnumerable<string> Usings = lines
				.Where( line => line.StartsWith( "using " ) );

			string Namespace = lines
				.Where( line => line.StartsWith( "namespace " ) )
				.Single()
				.Split( ' ' )
				[1];

			if( string.IsNullOrWhiteSpace( Namespace ) ) {
				Console.WriteLine( "Namespace not found" );
				return 1;
			}

			string Class = lines
				.Where( line => line.Contains( "class " ) )
				.Single()
				.Split( new[] { "class " }, StringSplitOptions.RemoveEmptyEntries )
				[1]
				.Split()
				[0];

			if( string.IsNullOrWhiteSpace( Class ) ) {
				Console.WriteLine( "Class not found" );
				return 1;
			}

			IEnumerable<FieldData> Fields = lines
				.Where( line => line.Contains( "public readonly " ) )
				.Select( line =>
					line.Split(
						new[] { "public readonly ", " ", "\t", ";", "//" },
						StringSplitOptions.RemoveEmptyEntries
					)
				)
				.Select( x => GetFieldData( new FieldData( x[0], x[1], parameters: x.Skip( 2 ) ) ) );

			if( Fields.Any( fd => fd.IsEnumerable ) ) {
				if( !Usings.Contains( "using System.Linq;" ) )
					Usings = Usings.Concat( new[] { "using System.Linq;" } );
			} else {
				Console.WriteLine( "No fields found" );
				return 1;
			}

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

			return 0;
		}
	}
}

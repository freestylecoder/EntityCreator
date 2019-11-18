using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EntityCreator {
	public class Program {
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
				? $"		public readonly IEnumerable<{x.DataType}> {x.Name};"
				: $"		public readonly {x.DataType} {x.Name};"
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
					return $"{fieldData.Name.ToLower()}?.Select( x => new {fieldData.DataType}( {fieldData.Name.ToLower()} ) ) ?? Enumerable.Empty<{fieldData.DataType}>()";
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
			: this( {string.Join( ", ", Fields.Select(x => $"copy.{x.Name}" ) )} ) {{ }}";

		private static string Withers( string Class, IEnumerable<FieldData> Fields ) {
			string newCopy = $"new {Class}( { string.Join( ", ", Fields.Select( x => $"this.{x.Name}" ) ) } );";

			return JoinWithNewLines(
				Fields
					.Select( x =>
$@"		public {Class} With{x.Name}( {(x.IsEnumerable ? $"IEnumerable<{x.DataType}>" : x.DataType)} {x.Name.ToLower()} ) =>
			{newCopy.Replace( $"this.{x.Name}", x.Name.ToLower() )}"
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
					foreach( {x.DataType} x in {x.Name} )
						_hash = _hash * _littlePrime + SafeHashCode( this.{x.Name} );"
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
						this.{x.Name},
						that.{x.Name}
					)"
			: $"					&& this.{x.Name} == that.{x.Name}" 
		)
)}
				);
		}}";

		private static object CreateTargetObject( string sourceCode, IEnumerable<(string path, string name)> assemblies ) {
			Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();

			System.CodeDom.Compiler.CompilerParameters parameters = new System.CodeDom.Compiler.CompilerParameters(
				Assembly
					.GetExecutingAssembly()
					.GetReferencedAssemblies()
					.Select( x => x.Name + ".dll" )
					.Concat( assemblies.Select( t => t.name ) )
					.ToArray()
			) {
				GenerateExecutable = false,
				GenerateInMemory = true,
				CompilerOptions =
					assemblies.Any()
					? $"-lib:{string.Join( ",", assemblies.Select( t => t.path ) )}"
					: string.Empty
			};

			System.CodeDom.Compiler.CompilerResults results = 
				provider.CompileAssemblyFromSource( parameters, sourceCode );

			return results.Errors.Count == 0
				? results
					.CompiledAssembly
					.GetExportedTypes()
					.First()
					.GetConstructor( Type.EmptyTypes )
					.Invoke( Type.EmptyTypes )
				: null;
		}

		private static FieldData GetFieldData( FieldInfo f, string dataType, Assembly assembly = default ) {
			Type type = null;
			try {
				type = f.FieldType;
			} catch {
				if( null != assembly ) {
					type = assembly.GetTypes()
						.Where( t => t.FullName.EndsWith( $".{dataType}" ) )
						.FirstOrDefault();
				}
			}

			bool isEnumerable, isValueType, isCloneable, hasCopyCtor;
			if( !type.FullName.Equals( "System.String" )
				&& type.GetInterfaces().Contains( typeof( IEnumerable ) )
			) {
				isEnumerable = true;

				Type blah = type.GenericTypeArguments.First();
				isValueType = blah.IsValueType;
				isCloneable = blah.GetInterfaces().Contains( typeof( ICloneable ) );
				hasCopyCtor = null != blah.GetConstructor(
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
					null,
					new[] { blah },
					null
				);
			} else {
				isEnumerable = false;
				isValueType = type.IsValueType;
				isCloneable = type.GetInterfaces().Contains( typeof( ICloneable ) );
				hasCopyCtor = null != type.GetConstructor(
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
					null,
					new[] { type },
					null
				);
			}

			return new FieldData(
					datatype: isEnumerable
						? dataType.Substring( 0, dataType.Length - 1 ).Remove( 0, "IEnumerable<".Length )
						: dataType,
					name: f.Name,
					isenumerable: isEnumerable,
					isvaluetype: isValueType,
					iscloneable: isCloneable,
					hascopyctor: hasCopyCtor
				);
		}

		static void Main( string[] args ) {
			IEnumerable<string> lines = File.ReadAllLines( args[0] );

			IEnumerable<Assembly> LoadedAssemblies =
				args
					.Skip( 1 )
					.Select( s => Assembly.LoadFile( s ) );

			IEnumerable<(string path, string name)> assemblies =
				args.Length > 1
				? args
					.Skip( 1 )
					.Select( s => new FileInfo( s ) )
					.Select( fi => (fi.DirectoryName, fi.Name) )
				: Enumerable.Empty<(string, string)>();

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

			IEnumerable<FieldData> Fields0 = lines
				.Where( line => line.Contains( "public readonly " ) )
				.Select( line =>
					line.Split(
						new[] { "public readonly ", " ", "\t", ";", "//" },
						StringSplitOptions.RemoveEmptyEntries
					)
				)
				.Select( x => new FieldData( x[0], x[1] ) );

			object target = CreateTargetObject(
$@"{PrintUsings( Usings )}

namespace {Namespace} {{
	public class {Class} {{
{PrintFields( Fields0 )}
	}}
}}",
				assemblies
			);

			IEnumerable<FieldData> Fields = target.GetType()
				.GetFields()
				.Select( f => LoadedAssemblies
					.Prepend( Assembly.GetExecutingAssembly() )
					.Select( a => GetFieldData(
						f,
						Fields0.Where( x => x.Name == f.Name ).Single().DataType,
						a
					) )
					.Where( o => null != o )
					.First()
				);

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

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
	Fields.Select( x =>
		x.IsEnumerable
			? (
				x.IsValueType
					? $"			this.{x.Name} = {x.Name.ToLower()}?.Select( x => x ) ?? Enumerable.Empty<{x.DataType}>();"
					: $"			this.{x.Name} = {x.Name.ToLower()}?.Select( x => new {x.DataType}( x ) ) ?? Enumerable.Empty<{x.DataType}>();"
			)
			: $"			this.{x.Name} = {x.Name.ToLower()};"
	)
)}
		}}";

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

		private static object CreateTargetObject( string sourceCode ) {
			Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();

			System.CodeDom.Compiler.CompilerParameters parameters = new System.CodeDom.Compiler.CompilerParameters(
				Assembly.GetExecutingAssembly().GetReferencedAssemblies().Select( x => x.Name + ".dll" ).ToArray()
			) {
				GenerateExecutable = false,
				GenerateInMemory = true
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

			IEnumerable<FieldData> Fields0 = lines
				.Where( line => line.Contains( "public readonly " ) )
				.Select( line =>
					line.Split(
						new[] { "public readonly ", " ", "\t", ";" },
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
}}"
			);

			IEnumerable<FieldData> Fields = target.GetType()
				.GetFields()
				.Select( f => {
					string dt = Fields0.Where( x => x.Name == f.Name ).Single().DataType;

					return !f.FieldType.FullName.Equals( "System.String" )
					&& f.FieldType.GetInterfaces().Contains( typeof( IEnumerable ) )
						? new FieldData(
							datatype: dt
								.Substring( 0, dt.Length - 1 )
								.Remove( 0, "IEnumerable<".Length ),
							name: f.Name,
							isenumerable: true,
							isvaluetype: f.FieldType.GenericTypeArguments.First().IsValueType
						)
						: new FieldData(
							datatype: dt,
							name: f.Name,
							isenumerable: false,
							isvaluetype: f.FieldType.IsValueType
						);
					}
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

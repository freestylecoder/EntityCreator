using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityCreator {
	public class FieldData : IEquatable<FieldData> {
		public readonly string DataType;
		public readonly string Name;
		public readonly bool IsEnumerable;
		public readonly bool IsValueType;
		public readonly bool IsCloneable;
		public readonly bool HasCopyCtor;
		public readonly IEnumerable<int> Ints;
		public readonly IEnumerable<string> Parameters;

		public FieldData (
			string datatype = default,
			string name = default,
			bool isenumerable = default,
			bool isvaluetype = default,
			bool iscloneable = default,
			bool hascopyctor = default,
			IEnumerable<int> ints = default,
			IEnumerable<string> parameters = default
		) {
			this.DataType = datatype.Clone() as string;
			this.Name = name.Clone() as string;
			this.IsEnumerable = isenumerable;
			this.IsValueType = isvaluetype;
			this.IsCloneable = iscloneable;
			this.HasCopyCtor = hascopyctor;
			this.Ints = this.Ints = ints?.Select( x => x ) ?? Enumerable.Empty<int>();
			this.Parameters = parameters?.Select( x => x.Clone() as string ) ?? Enumerable.Empty<string>();
		}

		public FieldData( FieldData copy )
			: this( copy.DataType, copy.Name, copy.IsEnumerable, copy.IsValueType, copy.IsCloneable, copy.HasCopyCtor, copy.Ints, copy.Parameters ) { }

		public FieldData WithDataType( string datatype ) =>
			new FieldData( datatype, this.Name, this.IsEnumerable, this.IsValueType, this.IsCloneable, this.HasCopyCtor, this.Ints, this.Parameters );
		public FieldData WithName( string name ) =>
			new FieldData( this.DataType, name, this.IsEnumerable, this.IsValueType, this.IsCloneable, this.HasCopyCtor, this.Ints, this.Parameters );
		public FieldData WithIsEnumerable( bool isenumerable ) =>
			new FieldData( this.DataType, this.Name, isenumerable, this.IsValueType, this.IsCloneable, this.HasCopyCtor, this.Ints, this.Parameters );
		public FieldData WithIsValueType( bool isvaluetype ) =>
			new FieldData( this.DataType, this.Name, this.IsEnumerable, isvaluetype, this.IsCloneable, this.HasCopyCtor, this.Ints, this.Parameters );
		public FieldData WithIsCloneable( bool iscloneable ) =>
			new FieldData( this.DataType, this.Name, this.IsEnumerable, this.IsValueType, iscloneable, this.HasCopyCtor, this.Ints, this.Parameters );
		public FieldData WithHasCopyCtor( bool hascopyctor ) =>
			new FieldData( this.DataType, this.Name, this.IsEnumerable, this.IsValueType, this.IsCloneable, hascopyctor, this.Ints, this.Parameters );
		public FieldData WithInts( IEnumerable<int> ints ) =>
			new FieldData( this.DataType, this.Name, this.IsEnumerable, this.IsValueType, this.IsCloneable, this.HasCopyCtor, ints, this.Parameters );
		public FieldData WithParameters( IEnumerable<string> parameters ) =>
			new FieldData( this.DataType, this.Name, this.IsEnumerable, this.IsValueType, this.IsCloneable, this.HasCopyCtor, this.Ints, parameters );

		public override bool Equals( object obj ) {
			if( obj is FieldData that )
				return this.Equals( that );

			return base.Equals( obj );
		}

		private int? _hash = null;
		private const int _bigPrime = 10559;
		private const int _littlePrime = 8429;
		public override int GetHashCode() {
			Func<object, int> SafeHashCode = ( obj ) =>
				obj is object ish
				? ish.GetHashCode()
				: 0;

			if( !_hash.HasValue ) {
				unchecked {
					_hash = _bigPrime;

					_hash = _hash * _littlePrime + SafeHashCode( this.DataType );
					_hash = _hash * _littlePrime + SafeHashCode( this.Name );
					_hash = _hash * _littlePrime + SafeHashCode( this.IsEnumerable );
					_hash = _hash * _littlePrime + SafeHashCode( this.IsValueType );
					_hash = _hash * _littlePrime + SafeHashCode( this.IsCloneable );
					_hash = _hash * _littlePrime + SafeHashCode( this.HasCopyCtor );

					foreach( int x in Ints )
						_hash = _hash * _littlePrime + SafeHashCode( this.Ints );

					foreach( string x in Parameters )
						_hash = _hash * _littlePrime + SafeHashCode( this.Parameters );
				}
			}

			return _hash.Value;
		}

		public override string ToString() =>
			$"{this.DataType} {this.Name}";

		public bool Equals( FieldData that ) {
			if( ReferenceEquals( that, null ) )
				return false;

			return
				ReferenceEquals( this, that )
				|| (
					this.GetHashCode() == that.GetHashCode()
					&& this.DataType == that.DataType
					&& this.Name == that.Name
					&& this.IsEnumerable == that.IsEnumerable
					&& this.IsValueType == that.IsValueType
					&& this.IsCloneable == that.IsCloneable
					&& this.HasCopyCtor == that.HasCopyCtor
					&& Enumerable.SequenceEqual(
						this.Ints,
						that.Ints
					)
					&& Enumerable.SequenceEqual(
						this.Parameters,
						that.Parameters
					)
				);
		}

		public static bool operator ==( FieldData left, FieldData right ) =>
			ReferenceEquals( left, null )
				? ReferenceEquals( right, null )
				: left.Equals( right );

		public static bool operator !=( FieldData left, FieldData right ) =>
			!( left == right );
	}
}
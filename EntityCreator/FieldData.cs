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
		public readonly IEnumerable<string> Parameters;

		public FieldData (
			string datatype = default,
			string name = default,
			bool isenumerable = default,
			bool isvaluetype = default,
			bool iscloneable = default,
			bool hascopyctor = default,
			IEnumerable<string> parameters = default
		) {
			this.DataType = datatype;
			this.Name = name;
			this.IsEnumerable = isenumerable;
			this.IsValueType = isvaluetype;
			this.IsCloneable = iscloneable;
			this.HasCopyCtor = hascopyctor;
			this.Parameters = this.Parameters = parameters?.Select( x => x ) ?? Enumerable.Empty<string>();
		}

		public FieldData( FieldData copy )
			: this( copy.DataType, copy.Name, copy.IsEnumerable, copy.IsValueType, copy.IsCloneable, copy.HasCopyCtor, copy.Parameters ) { }

		public FieldData WithDataType( string datatype ) =>
			new FieldData( datatype, this.Name, this.IsEnumerable, this.IsValueType, this.IsCloneable, this.HasCopyCtor, this.Parameters );
		public FieldData WithName( string name ) =>
			new FieldData( this.DataType, name, this.IsEnumerable, this.IsValueType, this.IsCloneable, this.HasCopyCtor, this.Parameters );
		public FieldData WithIsEnumerable( bool isenumerable ) =>
			new FieldData( this.DataType, this.Name, isenumerable, this.IsValueType, this.IsCloneable, this.HasCopyCtor, this.Parameters );
		public FieldData WithIsValueType( bool isvaluetype ) =>
			new FieldData( this.DataType, this.Name, this.IsEnumerable, isvaluetype, this.IsCloneable, this.HasCopyCtor, this.Parameters );
		public FieldData WithIsCloneable( bool iscloneable ) =>
			new FieldData( this.DataType, this.Name, this.IsEnumerable, this.IsValueType, iscloneable, this.HasCopyCtor, this.Parameters );
		public FieldData WithHasCopyCtor( bool hascopyctor ) =>
			new FieldData( this.DataType, this.Name, this.IsEnumerable, this.IsValueType, this.IsCloneable, hascopyctor, this.Parameters );
		public FieldData WithParameters( IEnumerable<string> parameters ) =>
			new FieldData( this.DataType, this.Name, this.IsEnumerable, this.IsValueType, this.IsCloneable, this.HasCopyCtor, parameters );

		public override bool Equals( object obj ) {
			if( obj is FieldData that )
				return this.Equals( that );

			return base.Equals( obj );
		}

		private int? _hash = null;
		private const int _bigPrime = 45887;
		private const int _littlePrime = 7243;
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

					foreach( string x in Parameters.OrderBy( y => y ) )
						_hash = _hash * _littlePrime + SafeHashCode( x );
				}
			}

			return _hash.Value;
		}

		public override string ToString() =>
			$"{DataType} {Name}";

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
						this.Parameters.OrderBy( y => y ),
						that.Parameters.OrderBy( y => y )
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
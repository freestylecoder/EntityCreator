using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityCreator {
	public class FieldData : IEquatable<FieldData> {
		public readonly string DataType;
		public readonly string Name;
		public readonly bool IsEnumerable;
		public readonly bool IsValueType;
		public readonly IEnumerable<int> Ints;
		public readonly IEnumerable<FieldData> Objects;

		public FieldData (
			string datatype = default,
			string name = default,
			bool isenumerable = default,
			bool isvaluetype = default,
			IEnumerable<int> ints = default,
			IEnumerable<FieldData> objects = default
		) {
			this.DataType = datatype;
			this.Name = name;
			this.IsEnumerable = isenumerable;
			this.IsValueType = isvaluetype;
			this.Ints = ints?.Select( x => x ) ?? Enumerable.Empty<int>();
			this.Objects = objects?.Select( x => new FieldData( x ) ) ?? Enumerable.Empty<FieldData>();
		}

		public FieldData( FieldData copy )
			: this( copy.DataType, copy.Name, copy.IsEnumerable, copy.IsValueType, copy.Ints, copy.Objects ) { }

		public FieldData WithDataType( string datatype ) =>
			new FieldData( datatype, this.Name, this.IsEnumerable, this.IsValueType, this.Ints, this.Objects );
		public FieldData WithName( string name ) =>
			new FieldData( this.DataType, name, this.IsEnumerable, this.IsValueType, this.Ints, this.Objects );
		public FieldData WithIsEnumerable( bool isenumerable ) =>
			new FieldData( this.DataType, this.Name, isenumerable, this.IsValueType, this.Ints, this.Objects );
		public FieldData WithIsValueType( bool isvaluetype ) =>
			new FieldData( this.DataType, this.Name, this.IsEnumerable, isvaluetype, this.Ints, this.Objects );
		public FieldData WithInts( IEnumerable<int> ints ) =>
			new FieldData( this.DataType, this.Name, this.IsEnumerable, this.IsValueType, ints, this.Objects );
		public FieldData WithObjects( IEnumerable<FieldData> objects ) =>
			new FieldData( this.DataType, this.Name, this.IsEnumerable, this.IsValueType, this.Ints, objects );

		public override bool Equals( object obj ) {
			if( obj is FieldData that )
				return this.Equals( that );

			return base.Equals( obj );
		}

		private int? _hash = null;
		private const int _bigPrime = 45491;
		private const int _littlePrime = 5689;
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

					foreach( int x in Ints )
						_hash = _hash * _littlePrime + SafeHashCode( this.Ints );

					foreach( FieldData x in Objects )
						_hash = _hash * _littlePrime + SafeHashCode( this.Objects );
				}
			}

			return _hash.Value;
		}

		public override string ToString() =>
			throw new NotImplementedException();

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
					&& Enumerable.SequenceEqual(
						this.Ints,
						that.Ints
					)
					&& Enumerable.SequenceEqual(
						this.Objects,
						that.Objects
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
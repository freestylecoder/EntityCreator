using System;

namespace SomeRandomLib {
	public class Class1 : IEquatable<Class1> {
		public readonly Class2 class2;

		public Class1 (
			Class2 class2 = default
		) {
			this.class2 = new Class2( class2 );
		}

		public Class1( Class1 copy )
			: this( copy.class2 ) { }

		public Class1 Withclass2( Class2 class2 ) =>
			new Class1( class2 );

		public override bool Equals( object obj ) {
			if( obj is Class1 that )
				return this.Equals( that );

			return base.Equals( obj );
		}

		private int? _hash = null;
		private const int _bigPrime = 37139;
		private const int _littlePrime = 9533;
		public override int GetHashCode() {
			Func<object, int> SafeHashCode = ( obj ) =>
				obj is object ish
				? ish.GetHashCode()
				: 0;

			if( !_hash.HasValue ) {
				unchecked {
					_hash = _bigPrime;

					_hash = _hash * _littlePrime + SafeHashCode( this.class2 );
				}
			}

			return _hash.Value;
		}

		public override string ToString() =>
			throw new NotImplementedException();

		public bool Equals( Class1 that ) {
			if( ReferenceEquals( that, null ) )
				return false;

			return
				ReferenceEquals( this, that )
				|| (
					this.GetHashCode() == that.GetHashCode()
					&& this.class2 == that.class2
				);
		}

		public static bool operator ==( Class1 left, Class1 right ) =>
			ReferenceEquals( left, null )
				? ReferenceEquals( right, null )
				: left.Equals( right );

		public static bool operator !=( Class1 left, Class1 right ) =>
			!( left == right );
	}
}
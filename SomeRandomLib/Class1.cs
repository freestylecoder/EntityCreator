using System;
using System.Collections.Generic;
using System.Linq;

namespace SomeRandomLib {
	public class Class1 : IEquatable<Class1> {
		public readonly Class2 class2;
		public readonly string MyString;
		public readonly IEnumerable<int> Ordered;	// Ordered
		public readonly IEnumerable<int> Disordered;
		public readonly IEnumerable<string> MyStrings;

		public Class1 (
			Class2 class2 = default,
			string mystring = default,
			IEnumerable<int> ordered = default,
			IEnumerable<int> disordered = default,
			IEnumerable<string> mystrings = default
		) {
			this.class2 = new Class2( class2 );
			this.MyString = mystring;
			this.Ordered = this.Ordered = ordered?.Select( x => x ) ?? Enumerable.Empty<int>();
			this.Disordered = this.Disordered = disordered?.Select( x => x ) ?? Enumerable.Empty<int>();
			this.MyStrings = this.MyStrings = mystrings?.Select( x => x ) ?? Enumerable.Empty<string>();
		}

		public Class1( Class1 copy )
			: this( copy.class2, copy.MyString, copy.Ordered, copy.Disordered, copy.MyStrings ) { }

		public Class1 Withclass2( Class2 class2 ) =>
			new Class1( class2, this.MyString, this.Ordered, this.Disordered, this.MyStrings );
		public Class1 WithMyString( string mystring ) =>
			new Class1( this.class2, mystring, this.Ordered, this.Disordered, this.MyStrings );
		public Class1 WithOrdered( IEnumerable<int> ordered ) =>
			new Class1( this.class2, this.MyString, ordered, this.Disordered, this.MyStrings );
		public Class1 WithDisordered( IEnumerable<int> disordered ) =>
			new Class1( this.class2, this.MyString, this.Ordered, disordered, this.MyStrings );
		public Class1 WithMyStrings( IEnumerable<string> mystrings ) =>
			new Class1( this.class2, this.MyString, this.Ordered, this.Disordered, mystrings );

		public override bool Equals( object obj ) {
			if( obj is Class1 that )
				return this.Equals( that );

			return base.Equals( obj );
		}

		private int? _hash = null;
		private const int _bigPrime = 23911;
		private const int _littlePrime = 2719;
		public override int GetHashCode() {
			Func<object, int> SafeHashCode = ( obj ) =>
				obj is object ish
				? ish.GetHashCode()
				: 0;

			if( !_hash.HasValue ) {
				unchecked {
					_hash = _bigPrime;

					_hash = _hash * _littlePrime + SafeHashCode( this.class2 );
					_hash = _hash * _littlePrime + SafeHashCode( this.MyString );

					foreach( int x in Ordered )
						_hash = _hash * _littlePrime + SafeHashCode( this.Ordered );

					foreach( int x in Disordered.OrderBy( y => y ) )
						_hash = _hash * _littlePrime + SafeHashCode( this.Disordered );

					foreach( string x in MyStrings.OrderBy( y => y ) )
						_hash = _hash * _littlePrime + SafeHashCode( this.MyStrings );
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
					&& this.MyString == that.MyString
					&& Enumerable.SequenceEqual(
						this.Ordered,
						that.Ordered
					)
					&& Enumerable.SequenceEqual(
						this.Disordered.OrderBy( y => y ),
						that.Disordered.OrderBy( y => y )
					)
					&& Enumerable.SequenceEqual(
						this.MyStrings.OrderBy( y => y ),
						that.MyStrings.OrderBy( y => y )
					)
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
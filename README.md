# EntityCreator
App to create fleshed out objects for POCOs

# Usage
EntityCreator.exe [drive:][path]filename [[drive:][path]assemblies[ ...]]

  filename      File to add functionality to
  assemblies    Extra assemblies needed to resolve types


The purpose of the list of assemblies is to allow the application to refect upon the data types of the fields. We do this to decide how to had vrious things. For example, we need to know whether a given type is a ReferenceType or an ValueType.

If no assemblies are passed in, we load `mscorelib`, `System`, and `System.Core`.  If assemblies are passed in, we load those assemblies and and referenced assemblies.  This is useful for when the POCO uses other types you created.

# Details
This app takes files with a minimally fleshed out POCO and adds several useful methods ond overrides.

For example, The project contains an assembly for testing with a file called Class1.cs. If this file were made a plain as possible, we could use the following as input:

```using System;
using System.Collections.Generic;
using System.Linq;

namespace SomeRandomLib {
	public class Class1 : IEquatable<Class1> {
		public readonly Class2 class2;
		public readonly string MyString;
		public readonly IEnumerable<int> Ordered;	// Ordered
		public readonly IEnumerable<int> Disordered;
		public readonly IEnumerable<string> MyStrings;
		public readonly IEnumerable<Class2> MyClasses;
		public readonly IEnumerable<Tuple<int,string>> Test1;
		public readonly (int,string) Test2;
  }
}
```

and the application would produce the following output:

```
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
		public readonly IEnumerable<Class2> MyClasses;
		public readonly IEnumerable<Tuple<int,string>> Test1;
		public readonly (int,string) Test2;

		public Class1 (
			Class2 class2 = default,
			string mystring = default,
			IEnumerable<int> ordered = default,
			IEnumerable<int> disordered = default,
			IEnumerable<string> mystrings = default,
			IEnumerable<Class2> myclasses = default,
			IEnumerable<Tuple<int,string>> test1 = default,
			(int,string) test2 = default
		) {
			this.class2 = new Class2( class2 );
			this.MyString = mystring;
			this.Ordered = this.Ordered = ordered?.Select( x => x ) ?? Enumerable.Empty<int>();
			this.Disordered = this.Disordered = disordered?.Select( x => x ) ?? Enumerable.Empty<int>();
			this.MyStrings = this.MyStrings = mystrings?.Select( x => x ) ?? Enumerable.Empty<string>();
			this.MyClasses = myclasses?.Select( x => new Class2( x ) ) ?? Enumerable.Empty<Class2>();
			this.Test1 = this.Test1 = test1?.Select( x => x ) ?? Enumerable.Empty<Tuple<int,string>>();
			this.Test2 = test2;
		}

		public Class1( Class1 copy )
			: this( copy.class2, copy.MyString, copy.Ordered, copy.Disordered, copy.MyStrings, copy.MyClasses, copy.Test1, copy.Test2 ) { }

		public Class1 Withclass2( Class2 class2 ) =>
			new Class1( class2, this.MyString, this.Ordered, this.Disordered, this.MyStrings, this.MyClasses, this.Test1, this.Test2 );
		public Class1 WithMyString( string mystring ) =>
			new Class1( this.class2, mystring, this.Ordered, this.Disordered, this.MyStrings, this.MyClasses, this.Test1, this.Test2 );
		public Class1 WithOrdered( IEnumerable<int> ordered ) =>
			new Class1( this.class2, this.MyString, ordered, this.Disordered, this.MyStrings, this.MyClasses, this.Test1, this.Test2 );
		public Class1 WithDisordered( IEnumerable<int> disordered ) =>
			new Class1( this.class2, this.MyString, this.Ordered, disordered, this.MyStrings, this.MyClasses, this.Test1, this.Test2 );
		public Class1 WithMyStrings( IEnumerable<string> mystrings ) =>
			new Class1( this.class2, this.MyString, this.Ordered, this.Disordered, mystrings, this.MyClasses, this.Test1, this.Test2 );
		public Class1 WithMyClasses( IEnumerable<Class2> myclasses ) =>
			new Class1( this.class2, this.MyString, this.Ordered, this.Disordered, this.MyStrings, myclasses, this.Test1, this.Test2 );
		public Class1 WithTest1( IEnumerable<Tuple<int,string>> test1 ) =>
			new Class1( this.class2, this.MyString, this.Ordered, this.Disordered, this.MyStrings, this.MyClasses, test1, this.Test2 );
		public Class1 WithTest2( (int,string) test2 ) =>
			new Class1( this.class2, this.MyString, this.Ordered, this.Disordered, this.MyStrings, this.MyClasses, this.Test1, test2 );

		public override bool Equals( object obj ) {
			if( obj is Class1 that )
				return this.Equals( that );

			return base.Equals( obj );
		}

		private int? _hash = null;
		private const int _bigPrime = 23893;
		private const int _littlePrime = 3361;
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
					_hash = _hash * _littlePrime + SafeHashCode( this.Test2 );

					foreach( int x in Ordered )
						_hash = _hash * _littlePrime + SafeHashCode( x );

					foreach( int x in Disordered.OrderBy( y => y ) )
						_hash = _hash * _littlePrime + SafeHashCode( x );

					foreach( string x in MyStrings.OrderBy( y => y ) )
						_hash = _hash * _littlePrime + SafeHashCode( x );

					foreach( Class2 x in MyClasses.OrderBy( y => y ) )
						_hash = _hash * _littlePrime + SafeHashCode( x );

					foreach( Tuple<int,string> x in Test1.OrderBy( y => y ) )
						_hash = _hash * _littlePrime + SafeHashCode( x );
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
					&& this.Test2 == that.Test2
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
					&& Enumerable.SequenceEqual(
						this.MyClasses.OrderBy( y => y ),
						that.MyClasses.OrderBy( y => y )
					)
					&& Enumerable.SequenceEqual(
						this.Test1.OrderBy( y => y ),
						that.Test1.OrderBy( y => y )
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
```

# Known Issues
We currently override ToString, but we throw a NotImplementedException. From the outseide, I cannot guess the best field or fields to use to create the string. If you end up using it, the NotImplementedException will make it obvious very quickly.

# Notes
We've been building this project on my live streams Fell free to join me on Mondays and Wednesdays at 1PM CST :)
https://twitch.tv/freestylecoder
https://mixer.com/freestylecoder


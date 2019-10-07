using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityCreator {
	public class TestEntity : IEquatable<TestEntity> {
		public readonly int Id;
		public readonly string Descrption;
		public readonly DateTime? Updated;
		public readonly IEnumerable<int> ListOfInts;

		public TestEntity (
			int id = default,
			string descrption = default,
			DateTime? updated = default,
			IEnumerable<int> listofints = default
		) {
			this.Id = id;
			this.Descrption = descrption;
			this.Updated = updated;
			this.ListOfInts = listofints;
		}

		public TestEntity( TestEntity copy )
			: this( copy.Id, copy.Descrption, copy.Updated, copy.ListOfInts ) { }

		public TestEntity WithId( int id ) =>
			new TestEntity( id, this.Descrption, this.Updated, this.ListOfInts );
		public TestEntity WithDescrption( string descrption ) =>
			new TestEntity( this.Id, descrption, this.Updated, this.ListOfInts );
		public TestEntity WithUpdated( DateTime? updated ) =>
			new TestEntity( this.Id, this.Descrption, updated, this.ListOfInts );
		public TestEntity WithListOfInts( IEnumerable<int> listofints ) =>
			new TestEntity( this.Id, this.Descrption, this.Updated, listofints );

		public override bool Equals( object obj ) {
			if( obj is TestEntity that )
				return this.Equals( that );

			return base.Equals( obj );
		}

		private int? _hash = null;
		private const int _bigPrime = 14767;
		private const int _littlePrime = 7841;
		public override int GetHashCode() {
			Func<object, int> SafeHashCode = ( obj ) =>
				 obj is object ish
				 ? ish.GetHashCode()
				 : 0;

			if( !_hash.HasValue ) {
				unchecked {
					_hash = _bigPrime;

					_hash = _hash * _littlePrime + SafeHashCode( Id );
					_hash = _hash * _littlePrime + SafeHashCode( Descrption );
					_hash = _hash * _littlePrime + SafeHashCode( Updated );
					_hash = _hash * _littlePrime + SafeHashCode( ListOfInts );
				}
			}

			return _hash.Value;
		}

		public override string ToString() =>
			throw new NotImplementedException();

		public bool Equals( TestEntity that ) {
			if( ReferenceEquals( that, null ) )
				return false;

			return
				ReferenceEquals( this, that )
				|| (
					this.GetHashCode() == that.GetHashCode()
					&& this.Id == that.Id
					&& this.Descrption == that.Descrption
					&& this.Updated == that.Updated
					&& this.ListOfInts == that.ListOfInts
				);
		}

		public static bool operator ==( TestEntity left, TestEntity right ) =>
			ReferenceEquals( left, null )
				? ReferenceEquals( right, null )
				: left.Equals( right );

		public static bool operator !=( TestEntity left, TestEntity right ) =>
			!( left == right );
	}
}

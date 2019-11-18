using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeRandomLib {
	public class Class3 : IEquatable<Class3> {
		public readonly int ClassId;
		public readonly int OtherClassId;
		public readonly string Field1;
		public readonly string Field2;
		public readonly char Field3;
		public readonly string Field4;
		public readonly string Field5;
		public readonly string Field6;
		public readonly int MOAR_Id;

		public Class3 (
			int classid = default,
			int otherclassid = default,
			string field1 = default,
			string field2 = default,
			char field3 = default,
			string field4 = default,
			string field5 = default,
			string field6 = default,
			int moar_id = default
		) {
			this.ClassId = classid;
			this.OtherClassId = otherclassid;
			this.Field1 = field1.Clone() as string;
			this.Field2 = field2.Clone() as string;
			this.Field3 = field3;
			this.Field4 = field4.Clone() as string;
			this.Field5 = field5.Clone() as string;
			this.Field6 = field6.Clone() as string;
			this.MOAR_Id = moar_id;
		}

		public Class3( Class3 copy )
			: this( copy.ClassId, copy.OtherClassId, copy.Field1, copy.Field2, copy.Field3, copy.Field4, copy.Field5, copy.Field6, copy.MOAR_Id ) { }

		public Class3 WithClassId( int classid ) =>
			new Class3( classid, this.OtherClassId, this.Field1, this.Field2, this.Field3, this.Field4, this.Field5, this.Field6, this.MOAR_Id );
		public Class3 WithOtherClassId( int otherclassid ) =>
			new Class3( this.ClassId, otherclassid, this.Field1, this.Field2, this.Field3, this.Field4, this.Field5, this.Field6, this.MOAR_Id );
		public Class3 WithField1( string field1 ) =>
			new Class3( this.ClassId, this.OtherClassId, field1, this.Field2, this.Field3, this.Field4, this.Field5, this.Field6, this.MOAR_Id );
		public Class3 WithField2( string field2 ) =>
			new Class3( this.ClassId, this.OtherClassId, this.Field1, field2, this.Field3, this.Field4, this.Field5, this.Field6, this.MOAR_Id );
		public Class3 WithField3( char field3 ) =>
			new Class3( this.ClassId, this.OtherClassId, this.Field1, this.Field2, field3, this.Field4, this.Field5, this.Field6, this.MOAR_Id );
		public Class3 WithField4( string field4 ) =>
			new Class3( this.ClassId, this.OtherClassId, this.Field1, this.Field2, this.Field3, field4, this.Field5, this.Field6, this.MOAR_Id );
		public Class3 WithField5( string field5 ) =>
			new Class3( this.ClassId, this.OtherClassId, this.Field1, this.Field2, this.Field3, this.Field4, field5, this.Field6, this.MOAR_Id );
		public Class3 WithField6( string field6 ) =>
			new Class3( this.ClassId, this.OtherClassId, this.Field1, this.Field2, this.Field3, this.Field4, this.Field5, field6, this.MOAR_Id );
		public Class3 WithMOAR_Id( int moar_id ) =>
			new Class3( this.ClassId, this.OtherClassId, this.Field1, this.Field2, this.Field3, this.Field4, this.Field5, this.Field6, moar_id );

		public override bool Equals( object obj ) {
			if( obj is Class3 that )
				return this.Equals( that );

			return base.Equals( obj );
		}

		private int? _hash = null;
		private const int _bigPrime = 29437;
		private const int _littlePrime = 8317;
		public override int GetHashCode() {
			Func<object, int> SafeHashCode = ( obj ) =>
				obj is object ish
				? ish.GetHashCode()
				: 0;

			if( !_hash.HasValue ) {
				unchecked {
					_hash = _bigPrime;

					_hash = _hash * _littlePrime + SafeHashCode( this.ClassId );
					_hash = _hash * _littlePrime + SafeHashCode( this.OtherClassId );
					_hash = _hash * _littlePrime + SafeHashCode( this.Field1 );
					_hash = _hash * _littlePrime + SafeHashCode( this.Field2 );
					_hash = _hash * _littlePrime + SafeHashCode( this.Field3 );
					_hash = _hash * _littlePrime + SafeHashCode( this.Field4 );
					_hash = _hash * _littlePrime + SafeHashCode( this.Field5 );
					_hash = _hash * _littlePrime + SafeHashCode( this.Field6 );
					_hash = _hash * _littlePrime + SafeHashCode( this.MOAR_Id );
				}
			}

			return _hash.Value;
		}

		public override string ToString() =>
			throw new NotImplementedException();

		public bool Equals( Class3 that ) {
			if( ReferenceEquals( that, null ) )
				return false;

			return
				ReferenceEquals( this, that )
				|| (
					this.GetHashCode() == that.GetHashCode()
					&& this.ClassId == that.ClassId
					&& this.OtherClassId == that.OtherClassId
					&& this.Field1 == that.Field1
					&& this.Field2 == that.Field2
					&& this.Field3 == that.Field3
					&& this.Field4 == that.Field4
					&& this.Field5 == that.Field5
					&& this.Field6 == that.Field6
					&& this.MOAR_Id == that.MOAR_Id
				);
		}

		public static bool operator ==( Class3 left, Class3 right ) =>
			ReferenceEquals( left, null )
				? ReferenceEquals( right, null )
				: left.Equals( right );

		public static bool operator !=( Class3 left, Class3 right ) =>
			!( left == right );
	}
}
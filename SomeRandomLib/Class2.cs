using System;

namespace SomeRandomLib {
	public class Class2 : ICloneable {
		public Class2( Class2 copy ) { }
		public object Clone() => throw new NotImplementedException();
	}
}
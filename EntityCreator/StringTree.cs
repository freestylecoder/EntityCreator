using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityCreator {
	public class StringTree {
		public readonly string Root;
		public readonly IEnumerable<StringTree> SubTree;

		public StringTree( string root ) : this( root, null ) {}

		public StringTree( string root, IEnumerable<StringTree> subtree ) {
			this.Root = root ?? throw new ArgumentNullException( "root" );
			this.SubTree = subtree ?? Enumerable.Empty<StringTree>();
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder( Root );

			if( SubTree.Any() ) {
				sb.Append( $"`{SubTree.Count()}" );
				sb.Append( '[' );
					sb.Append( string.Join( ",", SubTree.Select( x => x.ToString() ) ) );
				sb.Append( ']' );
			}

			return sb.ToString();
		}
	}
}

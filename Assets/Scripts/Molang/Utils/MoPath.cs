namespace CraftSharp.Molang.Utils
{
	/// <summary>
	///		Describes the path of a variable/function
	/// </summary>
	public class MoPath
	{
		/// <summary>
		///		The root of this path
		/// </summary>
		public MoPath Root { get; }
		
		/// <summary>
		///		The next element
		/// </summary>
		public MoPath Next { get; private set; }

		/// <summary>
		///		The full path
		/// </summary>
		public string Path { get; }
		
		/// <summary>
		///		The value of this path
		/// </summary>
		/// <remarks>
		/// Think of this is the filename in a path
		/// </remarks>
		public string Value { get; private set; }

		/// <summary>
		///		Whether this path has any child elements
		/// </summary>
		public bool HasChildren => Next != null;

		public MoPath(string path)
		{
			Next = null;
			Root = this;
			Path = path;

			var segments = path.Split('.');
			Value = segments[0];

			MoPath current = this;
			if (segments.Length > 1)
			{
				string currentPath = $"{Value}";

				for (int i = 1; i < segments.Length; i++)
				{
					var value = segments[i];

					if (string.IsNullOrWhiteSpace(value))
						break;

					currentPath += $".{value}";

					var moPath = new MoPath(Root, currentPath, value);
					current.Next = moPath;
					current = moPath;
				}
			}
		}

		internal MoPath(MoPath root,string path, string value)
		{
			Root = root;
			Path = path;
			Value = value;
		}

		internal void SetValue(string value)
		{
			Value = value;
		}
		
		/// <inheritdoc />
		public override string ToString()
		{
			return Path;
		}
	}
}
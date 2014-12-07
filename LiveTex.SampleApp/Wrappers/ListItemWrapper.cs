namespace LiveTex.SampleApp.Wrappers
{
	public abstract class ListItemWrapper
	{
		public abstract string Description { get; }
	}

	public class ListItemWrapper<T>
		: ListItemWrapper
	{
		private readonly T _sourceObject;
		private readonly string _description;

		public ListItemWrapper(T sourceObject, string description)
		{
			_sourceObject = sourceObject;
			_description = description ?? "";
		}

		public override string Description
		{
			get { return _description; }
		}

		public T SourceObject
		{
			get { return _sourceObject; }
		}
	}
}

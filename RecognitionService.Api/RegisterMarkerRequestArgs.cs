namespace RecognitionService.Api
{
	public readonly struct RegisterMarkerRequestArgs
	{
		public readonly int id;
		public readonly Triangle triangle;

		public RegisterMarkerRequestArgs(int id, Triangle triangle)
		{
			this.id = id;
			this.triangle = triangle;
		}
	}
}

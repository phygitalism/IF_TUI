namespace RecognitionService.Api
{
	public readonly struct RegisterRequestArgs
	{
		public readonly int id;
		public readonly Rectangle rectangle;

		public RegisterRequestArgs(int id, Rectangle rectangle)
		{
			this.id = id;
			this.rectangle = rectangle;
		}
	}
}

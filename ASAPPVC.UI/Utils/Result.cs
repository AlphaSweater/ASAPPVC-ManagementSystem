namespace ASAPPVC.UI.Utils
{
	/// <summary>
	/// Universal lightweight result wrapper for service and repository operations.
	/// Use <see cref="Result"/> for void-like results, or <see cref="Result{T}"/> for typed results.
	/// </summary>
	public readonly record struct Result
	{
		public bool Ok { get; }
		public string? Error { get; }

		private Result(bool ok, string? error = null) {
			(Ok, Error) = (ok, error);
		}

		// Factory helpers
		public static Result Success() {
			return new(true);
		}

		public static Result Fail(string error) {
			return new(false, error ?? "Unknown error");
		}

		public override string ToString() {
			return Ok ? "Success" : $"Fail({Error})";
		}
	}

	/// <summary>
	/// Generic variant of <see cref="Result"/> that carries a value.
	/// </summary>
	public readonly record struct Result<T>
	{
		public bool Ok { get; }
		public T? Value { get; }
		public string? Error { get; }

		private Result(bool ok, T? value = default, string? error = null) {
			(Ok, Value, Error) = (ok, value, error);
		}

		// Factory helpers
		public static Result<T> Success(T value) {
			return new(true, value);
		}

		public static Result<T> Fail(string error) {
			return new(false, default, error ?? "Unknown error");
		}

		public override string ToString() {
			return Ok ? $"Success({Value})" : $"Fail({Error})";
		}
	}
}
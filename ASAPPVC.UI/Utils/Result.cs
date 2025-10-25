namespace ASAPPVC.UI.Utils
{
	/// <summary>
	/// Universal lightweight result wrapper for service and repository operations.
	/// Use <see cref="Result"/> for void-like results, or <see cref="Result{T}"/> for typed results.
	///
	/// Example usage for non-generic <see cref="Result"/>:
	/// <code language="csharp">
	/// Creating results
	///   var ok = Result.Success();
	///   var fail = Result.Fail("Something went wrong");
	///
	/// Reading results
	///   if (ok.Ok) { "Success path" }
	///   if (!fail.Ok) { Console.WriteLine(fail.Error); // "Something went wrong" }
	/// </code>
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
	///
	/// Example usage for <see cref="Result{T}"/>:
	/// <code language="csharp">
	/// Creating results
	///	  var created = Result&lt;ComponentModel&gt;.Success(new ComponentModel { Name = "Bolt" });
	///	  var notFound = Result&lt;ComponentModel&gt;.Fail("Component not found");
	///
	/// Reading results
	///   if (created.Ok) { var component = created.Value; }
	///   if (!notFound.Ok) { Console.WriteLine(notFound.Error); }
	/// </code>
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
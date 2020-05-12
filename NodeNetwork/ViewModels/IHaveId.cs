namespace NodeNetwork.ViewModels
{
	/// <summary>
	/// Ensures Has Id for Serialisation
	/// </summary>
	public interface IHaveId
	{
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		string Id { get; set; }
	}
}

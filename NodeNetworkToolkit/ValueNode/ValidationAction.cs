namespace NodeNetwork.Toolkit.ValueNode
{

    /// <summary>
    /// Action that should be taken based on the validation result
    /// </summary>
    public enum ValidationAction
    {
        /// <summary>
        /// Don't run the validation. (LatestValidation is not updated)
        /// </summary>
        DontValidate,
        /// <summary>
        /// Run the validation, but ignore the result and assume the network is valid.
        /// </summary>
        IgnoreValidation,
        /// <summary>
        /// Run the validation and if the network is invalid then wait until it is valid.
        /// </summary>
        WaitForValid,
        /// <summary>
        /// Run the validation and if the network is invalid then make default(T) the current value.
        /// </summary>
        PushDefaultValue
    }
}

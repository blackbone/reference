namespace References
{
    /// <summary>
    /// Just marker interface to determine usage in modes
    /// </summary>
    public interface IReference
    {
        string AssetGuid { get; }
        int InstanceId { get; }
    }
}
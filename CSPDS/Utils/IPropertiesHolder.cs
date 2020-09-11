namespace CSPDS.Utils
{
    public interface IPropertiesHolder
    {
        string this[string key] { get; }
    }
}
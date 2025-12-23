namespace UnsecuredAPIKeys.Data.Common;

[AttributeUsage(AttributeTargets.Assembly)]
public class ProviderApiVersionAttribute : Attribute
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }

    public ProviderApiVersionAttribute(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }
}
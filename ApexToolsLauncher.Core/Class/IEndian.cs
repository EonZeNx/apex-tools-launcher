namespace ApexToolsLauncher.Core.Class;

public enum EEndian
{
    Little,
    Big
}

public interface IEndian
{
    public EEndian Endian { get; protected set; }
}

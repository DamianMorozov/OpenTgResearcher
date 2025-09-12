namespace TgInfrastructure.Contracts;

/// <summary> Base ViewModel </summary>
public interface ITgViewModelBase : ITgDebug
{
	bool IsLoad { get; set; }
}

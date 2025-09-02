// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Contracts;

/// <summary> Base ViewModel </summary>
public interface ITgViewModelBase : ITgDebug
{
	bool IsLoad { get; set; }
}

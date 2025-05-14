// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgSharedData.Enums;

public enum TgEnumFilterType
{
	None = 0,
	SingleName = 1,
	SingleExtension = 2,
	MultiName = 3,
	MultiExtension = 4,
	MinSize = 5,
	MaxSize = 6,
}
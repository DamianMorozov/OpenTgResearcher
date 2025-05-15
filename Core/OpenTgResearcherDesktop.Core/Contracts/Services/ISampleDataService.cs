// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Core.Contracts.Services;

// Remove this class once your pages/features are using your data.
public interface ISampleDataService
{
	Task<IEnumerable<SampleOrder>> GetContentGridDataAsync();

	Task<IEnumerable<SampleOrder>> GetGridDataAsync();

	Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();
}

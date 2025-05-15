// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Core.Models;

// Model for the SampleDataService. Replace with your own model.
public class SampleCompany
{
    public string CompanyID
    {
        get; set;
    }

    public string CompanyName
    {
        get; set;
    }

    public string ContactName
    {
        get; set;
    }

    public string ContactTitle
    {
        get; set;
    }

    public string Address
    {
        get; set;
    }

    public string City
    {
        get; set;
    }

    public string PostalCode
    {
        get; set;
    }

    public string Country
    {
        get; set;
    }

    public string Phone
    {
        get; set;
    }

    public string Fax
    {
        get; set;
    }

    public ICollection<SampleOrder> Orders
    {
        get; set;
    }
}

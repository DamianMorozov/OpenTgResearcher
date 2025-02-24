﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.License;

public partial class TgLicensePaid : TgLicenseTest
{
	#region Public and private fields, properties, constructor

	//

	#endregion

	#region Public and private methods

	public override bool IsValid()
	{
		return DateTime.Now < ExpirationDate;
	}

	#endregion
}

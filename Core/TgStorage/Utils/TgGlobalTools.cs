﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Utils;

/// <summary> Global tools </summary>
public static class TgGlobalTools
{
    #region Public and private fields, properties, constructor

    public static TgEnumAppType AppType { get; private set; }

    public static bool IsXmlReady => TgAppSettingsHelper.Instance.AppXml.IsExistsEfStorage;

    /// <summary> Autofac Container </summary>
	public static Autofac.IContainer Container = null!;

    #endregion

	#region Public and private methods

	public static void SetAppType(TgEnumAppType appType)
    {
        AppType = appType;
    }

    #endregion
}
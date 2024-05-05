﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Models;

/// <summary> Download settings </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgDownloadSettingsViewModel : ObservableObject, ITgCommon
{
	#region Public and private fields, properties, constructor

	private TgEfSourceRepository SourceRepository { get; } = new(TgEfUtils.EfContext);
	public TgEfSourceViewModel SourceVm { get; set; }
	public TgEfVersionViewModel VersionVm { get; set; }
	
	[DefaultValue(false)]
	public bool IsRewriteFiles { get; set; }
	[DefaultValue(false)]
	public bool IsRewriteMessages { get; set; }
	[DefaultValue(true)]
	public bool IsJoinFileNameWithMessageId { get; set; }

	public TgDownloadSettingsViewModel()
	{
		SourceVm = new();
		VersionVm = new();
		IsJoinFileNameWithMessageId = this.GetDefaultPropertyBool(nameof(IsJoinFileNameWithMessageId));
		IsRewriteFiles = this.GetDefaultPropertyBool(nameof(IsRewriteFiles));
		IsRewriteMessages = this.GetDefaultPropertyBool(nameof(IsRewriteMessages));
	}

	#endregion

	#region Public and private methods

    public string ToDebugString() => $"{SourceVm.ToDebugString()}";

    public void UpdateSourceWithSettings()
    {
        if (!SourceVm.IsReadySourceId)
            return;
        TgEfOperResult<TgEfSourceEntity> operResult = SourceRepository.Save(SourceVm.Item);
        if (operResult.IsExists) 
	        SourceVm.Item = operResult.Item;
    }

    #endregion
}
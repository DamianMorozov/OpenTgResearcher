// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.ViewModels;

/// <summary> Wrapper for exception </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgExceptionViewModel : TgViewModelBase
{
	#region Fields, properties, constructor

	private Exception? _exception;

	private Exception? Exception
	{
		get => _exception;
		set
		{
			_exception = value;
			Message = value is null ? string.Empty : GetInnerException(value);
			SetProperty(ref _exception, value);
			IsExist = value is not null;
			OnPropertyChanged();
		}
	}
	[ObservableProperty]
	public partial bool IsExist { get; set; }
	[ObservableProperty]
	public partial string Message { get; set; }

	public TgExceptionViewModel() : base()
	{
		Message = string.Empty;
		Exception = null;
	}

	public TgExceptionViewModel(Exception ex) : base()
	{
		Message = string.Empty;
		Exception = ex;
	}

	#endregion

	#region Methods

	public override string ToString() => Message;

	public void Default()
	{
		Exception = null;
	}

	public void Set(Exception ex)
	{
		Exception = ex;
	}

	public void Clear()
	{
		Exception = null;
	}

	private string GetInnerException(Exception ex) =>
		ex.InnerException is null ? ex.Message : ex.Message + Environment.NewLine + GetInnerException(ex.InnerException);

	#endregion
}
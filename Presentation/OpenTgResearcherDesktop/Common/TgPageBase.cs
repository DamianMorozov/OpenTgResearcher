// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Common;

public abstract class TgPageBase : Page
{
    #region Fields, properties, constructor

    public virtual ITgPageViewModel ViewModel => null!;
    public Grid? GridContent { get; set; }
    public double FadeAnimationDuration { get; private set; } = 10.0;
    public double OffsetAnimationDuration { get; private set; } = 10.0;

    #endregion

    #region Methods

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (App.MainWindow?.DispatcherQueue is not null)
        {
            var tcs = new TaskCompletionSource();
            await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
            {
                try
                {
                    await ViewModel.OnNavigatedToAsync(e);
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            await tcs.Task;
        }
    }

    protected void PageLoaded(object sender, RoutedEventArgs e) => ViewModel.OnLoaded(XamlRoot);

    protected void PageLoadedWithAnimation(object sender, RoutedEventArgs e)
    {
        StartEntranceAnimation();
        ViewModel.OnLoaded(XamlRoot);
    }

    private void StartEntranceAnimation()
    {
        if (GridContent is null) return;

        var visual = ElementCompositionPreview.GetElementVisual(GridContent);
        visual.Opacity = 0;
        visual.Offset = new System.Numerics.Vector3(0, 400, 0);
        GridContent.Visibility = Visibility.Visible;

        var compositor = visual.Compositor;

        var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
        fadeAnimation.Duration = TimeSpan.FromSeconds(FadeAnimationDuration);
        fadeAnimation.InsertKeyFrame(0, 0);
        fadeAnimation.InsertKeyFrame(1, 1);

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Duration = TimeSpan.FromSeconds(OffsetAnimationDuration);
        offsetAnimation.InsertKeyFrame(0, new System.Numerics.Vector3(0, 400, 0));
        offsetAnimation.InsertKeyFrame(1, new System.Numerics.Vector3(0, 0, 0));

        visual.StartAnimation("Opacity", fadeAnimation);
        visual.StartAnimation("Offset", offsetAnimation);
    }

    public void OnClipboardWriteClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        ViewModel.OnClipboardWriteCommand.Execute(button.Tag);
    }

    public void OnClipboardSilentWriteClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        ViewModel.OnClipboardSilentWriteCommand.Execute(button.Tag);
    }

    #endregion
}

namespace OpenTgResearcherDesktop.Common;

public abstract class TgPageBase : Page
{
    #region Fields, properties, constructor

    public virtual ITgPageViewModel ViewModel => default!;
    public Grid? GridContent { get; set; }

    #endregion

    #region Methods

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        await TgDesktopUtils.InvokeOnUIThreadAsync(() => ViewModel.OnNavigatedToAsync(e));
    }

    protected void PageLoaded(object sender, RoutedEventArgs e) => ViewModel.OnLoaded(XamlRoot);

    protected void PageLoadedWithAnimation(object sender, RoutedEventArgs e)
    {
        if (GridContent is not null)
        {
            // Get the visual representation of the element
            var visual = ElementCompositionPreview.GetElementVisual(GridContent);
            visual.Opacity = 0;
            visual.Offset = new System.Numerics.Vector3(0, 400, 0);
            GridContent.Visibility = Visibility.Visible;

            var compositor = visual.Compositor;

            // Create easing function for fast start and smooth end
            var easeOut = compositor.CreateCubicBezierEasingFunction(
                new System.Numerics.Vector2(0.0f, 0.0f), // Start point of curve
                new System.Numerics.Vector2(0.2f, 1.0f)  // End point of curve
            );

            // Create fade animation with easing
            var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.Duration = TimeSpan.FromSeconds(5.0); // Slower fade
            fadeAnimation.InsertKeyFrame(0, 0);
            fadeAnimation.InsertKeyFrame(1, 1, easeOut);

            // Create offset animation with easing
            var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Duration = TimeSpan.FromSeconds(3.0); // Slower movement
            offsetAnimation.InsertKeyFrame(0, new System.Numerics.Vector3(0, 400, 0));
            offsetAnimation.InsertKeyFrame(1, new System.Numerics.Vector3(0, 0, 0), easeOut);

            // Start both animations
            visual.StartAnimation("Opacity", fadeAnimation);
            visual.StartAnimation("Offset", offsetAnimation);
        }

        // Notify ViewModel that the page is loaded
        ViewModel.OnLoaded(XamlRoot);
    }

    /// <summary> Clipboard silent write with animation </summary>
    public async void OnClipboardSilentWriteClick(object sender, RoutedEventArgs e)
    {
        if (sender is not TgAnimatedClipboardButton button) return;
        
        if (button.Tag.ToString() is string tag && !string.IsNullOrEmpty(tag))
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(tag);
            Clipboard.SetContent(dataPackage);

            try
            {
                VisualStateManager.GoToState(button, "CheckedState", useTransitions: true);
                await Task.Delay(TimeSpan.FromSeconds(button.ResetDelay));
                VisualStateManager.GoToState(button, "NormalState", useTransitions: true);
            }
            catch (Exception ex)
            {
                ViewModel.LogError(ex);
            }
        }
    }

    /// <summary> Open url </summary>
    public void OnOpenHyperlink(object sender, RoutedEventArgs e)
    {
        if (sender is not HyperlinkButton hyperlinkButton) return;
        if (hyperlinkButton.Tag is not string tag) return;

        var url = TgDesktopUtils.ExtractUrl(tag);
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }

    #endregion
}

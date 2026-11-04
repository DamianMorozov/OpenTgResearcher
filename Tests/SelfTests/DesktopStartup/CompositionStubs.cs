namespace Microsoft.UI.Composition;

// Фиксируем команды исходного контроллера вместо подмены его поведения готовыми результатами
public class Visual
{
    public Compositor Compositor { get; } = new();
    public System.Numerics.Vector2 Size { get; set; } = new(80, 18);
    public System.Numerics.Vector3 CenterPoint { get; set; }
    public System.Numerics.Vector3 Scale { get; set; } = System.Numerics.Vector3.One;
    public System.Numerics.Vector3 Offset { get; set; }
    public float RotationAngleInDegrees { get; set; }
    public float Opacity { get; set; } = 1;
    public Dictionary<string, CompositionAnimation> Animations { get; } = [];
    /// <summary> Сохраняет настоящее описание запрошенной анимации </summary>
    public void StartAnimation(string property, CompositionAnimation animation) => Animations[property] = animation;
    /// <summary> Убирает только явно остановленную анимацию </summary>
    public void StopAnimation(string property) => Animations.Remove(property);
}

public enum AnimationIterationBehavior { Count, Forever }
public abstract class CompositionAnimation
{
    public TimeSpan Duration { get; set; }
    public TimeSpan DelayTime { get; set; }
    public AnimationIterationBehavior IterationBehavior { get; set; }
}
public sealed class ScalarKeyFrameAnimation : CompositionAnimation
{
    public List<(float Progress, float Value)> Frames { get; } = [];
    /// <summary> Запоминает ключевой кадр, включая его фактическое значение </summary>
    public void InsertKeyFrame(float progress, float value, CompositionEasingFunction? easing = null) => Frames.Add((progress, value));
}
public sealed class Vector3KeyFrameAnimation : CompositionAnimation
{
    public List<(float Progress, System.Numerics.Vector3 Value)> Frames { get; } = [];
    /// <summary> Запоминает масштаб исходного визуального слоя </summary>
    public void InsertKeyFrame(float progress, System.Numerics.Vector3 value, CompositionEasingFunction? easing = null) => Frames.Add((progress, value));
}
public abstract class CompositionEasingFunction;
public sealed class CubicBezierEasingFunction : CompositionEasingFunction;
public sealed class LinearEasingFunction : CompositionEasingFunction;
public enum CompositionBatchTypes { Animation }
public sealed class CompositionBatchCompletedEventArgs : EventArgs;
public sealed class CompositionScopedBatch : IDisposable
{
    public bool IsEnded { get; private set; }
    public bool IsDisposed { get; private set; }
    public event Action<object, CompositionBatchCompletedEventArgs>? Completed;
    /// <summary> Заканчивает сбор команд без преждевременного завершения анимаций </summary>
    public void End() => IsEnded = true;
    /// <summary> Имитирует окончание кадров только по явному действию теста </summary>
    public void Complete() { if (!IsDisposed) Completed?.Invoke(this, new()); }
    /// <summary> Обнаруживает освобождение пакета при закрытии страницы </summary>
    public void Dispose() { IsDisposed = true; Completed = null; }
}
public sealed class Compositor
{
    public List<CompositionScopedBatch> Batches { get; } = [];
    /// <summary> Запоминает пакет для проверки фактического ожидания готовности </summary>
    public CompositionScopedBatch CreateScopedBatch(CompositionBatchTypes type) { var batch = new CompositionScopedBatch(); Batches.Add(batch); return batch; }
    /// <summary> Заменяет платформенную линейную кривую </summary>
    public LinearEasingFunction CreateLinearEasingFunction() => new();
    /// <summary> Создаёт описание скалярной анимации без движка Windows </summary>
    public ScalarKeyFrameAnimation CreateScalarKeyFrameAnimation() => new();
    /// <summary> Создаёт описание изменения масштаба </summary>
    public Vector3KeyFrameAnimation CreateVector3KeyFrameAnimation() => new();
    /// <summary> Заменяет только платформенную кривую замедления </summary>
    public CubicBezierEasingFunction CreateCubicBezierEasingFunction(System.Numerics.Vector2 first, System.Numerics.Vector2 second) => new();
}

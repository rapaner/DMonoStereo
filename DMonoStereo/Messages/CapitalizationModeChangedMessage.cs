namespace DMonoStereo.Messages;

/// <summary>
/// Сообщение об изменении режима капитализации
/// </summary>
public class CapitalizationModeChangedMessage
{
    public KeyboardFlags Mode { get; }

    public CapitalizationModeChangedMessage(KeyboardFlags mode)
    {
        Mode = mode;
    }
}
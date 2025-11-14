namespace StaticWebForms.Services;

/// <summary>
/// Implementation of IErrorService for displaying messages to users
/// </summary>
public class ErrorService : IErrorService
{
    public event Action<MessageEventArgs>? OnMessage;

    public void ShowError(string message, string? title = null)
    {
        OnMessage?.Invoke(new MessageEventArgs
        {
            Message = message,
            Title = title ?? "Errore",
            Type = MessageType.Error
        });
    }

    public void ShowWarning(string message, string? title = null)
    {
        OnMessage?.Invoke(new MessageEventArgs
        {
            Message = message,
            Title = title ?? "Attenzione",
            Type = MessageType.Warning
        });
    }

    public void ShowInfo(string message, string? title = null)
    {
        OnMessage?.Invoke(new MessageEventArgs
        {
            Message = message,
            Title = title ?? "Informazione",
            Type = MessageType.Info
        });
    }

    public void ShowSuccess(string message, string? title = null)
    {
        OnMessage?.Invoke(new MessageEventArgs
        {
            Message = message,
            Title = title ?? "Successo",
            Type = MessageType.Success
        });
    }

    public void CloseMessage()
    {
        OnMessage?.Invoke(new MessageEventArgs
        {
            Message = string.Empty,
            Type = MessageType.Info
        });
    }
}


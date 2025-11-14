namespace StaticWebForms.Services;

/// <summary>
/// Service for managing and displaying error messages to users
/// </summary>
public interface IErrorService
{
    /// <summary>
    /// Shows an error message to the user
    /// </summary>
    void ShowError(string message, string? title = null);

    /// <summary>
    /// Shows a warning message to the user
    /// </summary>
    void ShowWarning(string message, string? title = null);

    /// <summary>
    /// Shows an information message to the user
    /// </summary>
    void ShowInfo(string message, string? title = null);

    /// <summary>
    /// Shows a success message to the user
    /// </summary>
    void ShowSuccess(string message, string? title = null);

    /// <summary>
    /// Event raised when a message should be displayed
    /// </summary>
    event Action<MessageEventArgs>? OnMessage;

    /// <summary>
    /// Closes the current message
    /// </summary>
    void CloseMessage();
}

/// <summary>
/// Event arguments for message display
/// </summary>
public class MessageEventArgs
{
    public string Message { get; set; } = string.Empty;
    public string? Title { get; set; }
    public MessageType Type { get; set; }
}

/// <summary>
/// Type of message to display
/// </summary>
public enum MessageType
{
    Error,
    Warning,
    Info,
    Success
}


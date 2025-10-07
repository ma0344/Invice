using System;

namespace Invoice.Classes
{
    public class DomainError
    {
        public string Message { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
    }

    public class DomainInfo
    {
        public string Message { get; set; } = string.Empty;
    }

    public static class DomainEvents
    {
        public static event Action<DomainError>? ErrorRaised;
        public static event Action<DomainInfo>? InfoRaised;
        public static event Action<string>? ClipboardCopyRequested;

        public static void RaiseError(string message, Exception? exception = null)
        {
            ErrorRaised?.Invoke(new DomainError { Message = message, Exception = exception });
        }

        public static void RaiseInfo(string message)
        {
            InfoRaised?.Invoke(new DomainInfo { Message = message });
        }

        public static void RaiseClipboardCopy(string text)
        {
            ClipboardCopyRequested?.Invoke(text);
        }
    }
}

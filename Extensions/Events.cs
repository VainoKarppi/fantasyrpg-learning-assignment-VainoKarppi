public static class EventExtensions {
    public static void InvokeFireAndForget(this Delegate? eventHandler, params object?[]? args) {
        if (eventHandler == null) return;

        // If args is null, assign an empty array
        args ??= []; 

        var invocationList = eventHandler.GetInvocationList();

        // Fire and forget each event handler asynchronously
        foreach (var handler in invocationList) {
            Task.Run(() => handler.DynamicInvoke(args));
        }
    }
}
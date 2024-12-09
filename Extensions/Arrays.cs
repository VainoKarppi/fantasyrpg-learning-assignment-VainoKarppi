public static class StringArrayExtensions {
    public static bool ContainsIgnoreCase(this string?[] array, string? value) {
        if (array == null) return false;

        return Array.Exists(array, item => string.Equals(item, value, StringComparison.InvariantCultureIgnoreCase));
    }
}
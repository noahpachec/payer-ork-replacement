using System.Text.Json;

namespace Ork.Helpers
{
    // Copied from Cryptide/Cryptide/Tools/Serialization.cs - only the extensions used by
    // PayerService are kept. The Cryptide AOT branch is dropped; the payer models are not AOT types.
    public static class Serialization
    {
        public static T DeserializeTo<T>(this string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return DeserializeTo<T>(json, options);
        }
        public static T DeserializeTo<T>(this string json, JsonSerializerOptions options)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("The string could not be deserialized to the type of " + typeof(T));
            }
        }
        public static string ToBase64String(this byte[] a)
        {
            return Convert.ToBase64String(a);
        }
    }
}

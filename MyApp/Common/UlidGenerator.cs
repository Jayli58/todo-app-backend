namespace MyApp.Common
{
    public static class UlidGenerator
    {
        public static string NewUlid()
        {
            return Ulid.NewUlid().ToString();
        }
    }
}

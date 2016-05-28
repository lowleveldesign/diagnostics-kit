namespace LowLevelDesign.Diagnostics.Commons
{
    public static class Constraints
    {
        public const int MaxApplicationPathLength = 2000;

        public const int MaxServerNameLength = 200;

        public const int MaxServerFqdnOrIpLength = 255;

        public const int MaxAppPoolNameLength = 500;

        public const int MaxBindingLength = 3000;

        public const int MaxServiceNameLength = 300;

        public const int MaxDisplayNameLength = 500;

        public const int MaxLoggerNameLength = 200;

        public const int MaxMessageLength = 7000;

        public const int MaxIdentityLength = 200;

        public const int MaxCorrelationIdLength = 1024;

        public const int MaxExceptionTypeLength = 100;

        public const int MaxExceptionMessageLength = 2000;

        public const int MaxExceptionAdditionalInfoLength = 5000;

        public const int MaxAdditionalFieldKeyLength = 256;

        public const int MaxAdditionalFieldValueLength = 5000;

        public const int MaxPerformanceDataKeyLength = 100;
    }
}

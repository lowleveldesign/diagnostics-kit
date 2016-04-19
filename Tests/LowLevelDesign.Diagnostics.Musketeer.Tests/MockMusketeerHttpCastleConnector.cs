namespace LowLevelDesign.Diagnostics.Musketeer.Tests
{
    class MockMusketeerHttpCastleConnectorFactory : IMusketeerHttpCastleConnectorFactory
    {
        private readonly IMusketeerHttpCastleConnector connector;

        public MockMusketeerHttpCastleConnectorFactory(IMusketeerHttpCastleConnector connectorToReturn)
        {
            connector = connectorToReturn;
        }

        public IMusketeerHttpCastleConnector CreateCastleConnector()
        {
            return connector;
        }
    }
}

namespace Entities.Exceptions
{
    public sealed class EmailCcConfigurationNotFoundException : NotFoundException
    {
        public EmailCcConfigurationNotFoundException(int id)
            : base($"Email CC configuration with id: {id} doesn't exist in the database.")
        {
        }

        public EmailCcConfigurationNotFoundException(string configKey)
            : base($"Email CC configuration with key: {configKey} doesn't exist in the database.")
        {
        }
    }
}
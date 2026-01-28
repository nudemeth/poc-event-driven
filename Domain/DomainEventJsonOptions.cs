using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Domain;
using Domain.Account;

public static class DomainEventJsonOptions
{
    public static readonly JsonSerializerOptions Instance = new()
    {
        TypeInfoResolver = new DomainEventTypeResolver(),
        AllowOutOfOrderMetadataProperties = true,
        WriteIndented = false
    };

    private class DomainEventTypeResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var jsonTypeInfo = base.GetTypeInfo(type, options);
            var domainEventType = typeof(DomainEvent);

            if (jsonTypeInfo.Type == domainEventType)
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "EventType",
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                    DerivedTypes =
                    {
                        new JsonDerivedType(typeof(AccountOpened), nameof(AccountOpened)),
                        new JsonDerivedType(typeof(AccountClosed), nameof(AccountClosed)),
                        new JsonDerivedType(typeof(MoneyDeposited), nameof(MoneyDeposited)),
                        new JsonDerivedType(typeof(MoneyTransferred), nameof(MoneyTransferred)),
                        new JsonDerivedType(typeof(MoneyWithdrawn), nameof(MoneyWithdrawn))
                    }
                };
            }

            return jsonTypeInfo;
        }
    }
}


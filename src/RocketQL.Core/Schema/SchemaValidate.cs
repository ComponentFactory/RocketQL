using System.ComponentModel.DataAnnotations;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private readonly List<ValidationException> _validationErrors = [];

    public void Validate()
    {
        Clean();
        SyntaxNodeList nodes = new(_syntaxNodes);

        try
        {

            while (nodes.Count > 0)
                if (ValidateNodes(nodes) == 0)
                    throw UnresolvedNodesException(nodes);

            ValidateSchemaOperations();
            IsValidated = true;
        }
        catch 
        {
            // Remove any partial results
            Clean();
            throw;
        }
    }

    private int ValidateNodes(SyntaxNodeList nodes, bool errors = false)
    {
        int processed = 0;

        for(int i=0; i<nodes.Count; i++)
        {
            if (nodes[i] switch
            {
                SyntaxSchemaDefinitionNode schemaNode => Validate(schemaNode, errors),
                _ => throw ValidationException.UnrecognizedType(nodes[i].Location, nodes[i].GetType().Name)
            })
            {
                nodes.RemoveAt(i--);
                processed++;
            }
        }

        return processed;
    }

    private bool Validate(SyntaxSchemaDefinitionNode schemaNode, bool errors = false) 
    { 
        // Only allowed a single schema definition
        if (!Definition.IsDefault)
            throw ValidationException.SchemaDefinitionAlreadyDefined(schemaNode.Location);

        // Cannot have an empty schema definition
        if (schemaNode.OperationTypes.Count == 0)
            throw ValidationException.SchemaDefinitionEmpty(schemaNode.Location);

        // Resolve the operations
        Dictionary<OperationType, OperationTypeDefinition> operations = [];
        foreach (var operation in schemaNode.OperationTypes)
        {
            // Cannot define the same operation twice
            if (operations.ContainsKey(operation.Operation))
                throw ValidationException.SchemaOperationAlreadyDefined(operation.Location, operation.Operation);

            // The object type might not have been defined yet
            if (!ObjectTypes.TryGetValue(operation.NamedType, out var typeDefinition))
            {
                if (errors)
                    _validationErrors.Add(ValidationException.TypeNotDefinedForSchemaOperation(operation.Location, operation.Operation, operation.NamedType));

                return false;
            }
        }

        // Must define the query operation
        if (!operations.TryGetValue(OperationType.QUERY, out var queryType))
            throw ValidationException.SchemaDefinitionMissingQuery(schemaNode.Location);

        // Other operations are optional
        operations.TryGetValue(OperationType.MUTATION, out var mutatationType);
        operations.TryGetValue(OperationType.SUBSCRIPTION, out var subscriptionType);

        // Resolve all the directives
        // TODO

        Definition = new()
        {
            Description = schemaNode.Description,
            Directives = [], // TODO
            Query = queryType,
            Mutation = mutatationType,
            Subscription = subscriptionType,
            Location = schemaNode.Location,
        };

        return true;
    }

    private void ValidateSchemaOperations()
    {
        // Schema definition can be omitted
        if (Definition.IsDefault)
        {
            // Look for types with names that match the operation, but the type cannot be referenced by any other types
            if (ObjectTypes.TryGetValue("Query", out var queryTypeDefinition) && (queryTypeDefinition.UsedByTypes.Count == 0))
            {
                Definition.Query = new OperationTypeDefinition()
                {
                    Operation = OperationType.QUERY,
                    ObjectTypeDefinition = queryTypeDefinition,
                    Location = queryTypeDefinition.Location,
                };
            }

            if (ObjectTypes.TryGetValue("Mutation", out var mutationTypeDefinition) && (mutationTypeDefinition.UsedByTypes.Count == 0))
            {
                Definition.Mutation = new OperationTypeDefinition()
                {
                    Operation = OperationType.MUTATION,
                    ObjectTypeDefinition = mutationTypeDefinition,
                    Location = mutationTypeDefinition.Location,
                };
            }

            if (ObjectTypes.TryGetValue("Subscription", out var subscriptionTypeDefinition) && (subscriptionTypeDefinition.UsedByTypes.Count == 0))
            {
                Definition.Subscription = new OperationTypeDefinition()
                {
                    Operation = OperationType.SUBSCRIPTION,
                    ObjectTypeDefinition = subscriptionTypeDefinition,
                    Location = subscriptionTypeDefinition.Location,
                };
            }

            // Schema must always define the Query root operation
            if (Definition.Query is null)
                throw ValidationException.SchemaDefinitionMissingQuery(Definition.Location);

            // Each operation must have a different type
            if (Definition.Mutation is not null)
            {
                if (Definition.Query.ObjectTypeDefinition == Definition.Mutation.ObjectTypeDefinition)
                    throw ValidationException.SchemaOperationsNotUnique(Definition.Location, "Query", "Mutation", Definition.Query.ObjectTypeDefinition.Name);
            }

            if (Definition.Subscription is not null)
            {
                if (Definition.Query.ObjectTypeDefinition == Definition.Subscription.ObjectTypeDefinition)
                    throw ValidationException.SchemaOperationsNotUnique(Definition.Location, "Query", "Subscription", Definition.Query.ObjectTypeDefinition.Name);

                if (Definition.Mutation is not null)
                {
                    if (Definition.Mutation.ObjectTypeDefinition == Definition.Subscription.ObjectTypeDefinition)
                        throw ValidationException.SchemaOperationsNotUnique(Definition.Location, "Mutation", "Subscription", Definition.Mutation.ObjectTypeDefinition.Name);
                }
            }
        }
    }
    private Exception UnresolvedNodesException(SyntaxNodeList nodes)
    {
        // Validate remaining nodes again, but this time generating errors
        ValidateNodes(nodes, true);

        if (_validationErrors.Count == 1)
            return _validationErrors[0];

        return new AggregateException(_validationErrors);
    }

    private void Clean()
    {
        _validationErrors.Clear();

        Definition = new();
        Directives.Clear();
        ScalarTypes.Clear();
        ObjectTypes.Clear();
        InterfaceTypes.Clear();
        UnionTypes.Clear();
        EnumTypes.Clear();
        InputObjectTypes.Clear();

        IsValidated = false;
    }
}

﻿using System.Linq;
using System;
using HotChocolate.Types.Descriptors.Definitions;
using HotChocolate.Configuration;
using HotChocolate.Language;

#nullable enable

namespace HotChocolate.Types
{
    public abstract class NamedTypeBase<TDefinition>
        : TypeSystemObjectBase<TDefinition>
        , INamedType
        , IHasDirectives
        , IHasClrType
        , IHasTypeIdentity
        where TDefinition : DefinitionBase, IHasDirectiveDefinition, IHasSyntaxNode
    {
        private IDirectiveCollection? _directives;
        private Type? _clrType;
        private ISyntaxNode? _syntaxNode;

        ISyntaxNode? IHasSyntaxNode.SyntaxNode => _syntaxNode;

        public abstract TypeKind Kind { get; }

        public IDirectiveCollection Directives
        {
            get
            {
                if (_directives is null)
                {
                    throw new TypeInitializationException();
                }
                return _directives;
            }
        }

        public Type ClrType
        {
            get
            {
                if (_clrType is null)
                {
                    throw new TypeInitializationException();
                }
                return _clrType;
            }
        }

        public Type? TypeIdentity { get; private set; }

        public virtual bool IsAssignableFrom(INamedType type) =>
            ReferenceEquals(type, this);

        protected override void OnRegisterDependencies(
            IInitializationContext context,
            TDefinition definition)
        {
            base.OnRegisterDependencies(context, definition);

            _clrType = definition is IHasClrType clr && clr.ClrType != GetType()
                ? clr.ClrType
                : typeof(object);

            context.RegisterDependencyRange(
                definition.Directives.Select(t => t.Reference));
        }

        protected override void OnCompleteType(
            ICompletionContext context,
            TDefinition definition)
        {
            base.OnCompleteType(context, definition);

            _syntaxNode = definition.SyntaxNode;

            var directives = new DirectiveCollection(this, definition.Directives);
            directives.CompleteCollection(context);
            _directives = directives;
        }

        protected void SetTypeIdentity(Type typeDefinition)
        {
            if (typeDefinition is null)
            {
                throw new ArgumentNullException(nameof(typeDefinition));
            }

            if (!typeDefinition.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    "The type definition must be a generic type definition.",
                    nameof(typeDefinition));
            }

            if (ClrType != typeof(object))
            {
                TypeIdentity = typeDefinition.MakeGenericType(ClrType);
            }
        }
    }
}

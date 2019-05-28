﻿namespace ProtoBuf
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=false), ImmutableObject(true)]
    public sealed class ProtoBeforeDeserializationAttribute : Attribute
    {
    }
}


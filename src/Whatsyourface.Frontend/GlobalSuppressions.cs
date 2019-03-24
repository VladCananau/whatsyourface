// <copyright file="GlobalSuppressions.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Reliability",
    "CA2007:Do not directly await a Task",
    Justification = "ASP.NET Core does not use SynchronizationContext anymore")]
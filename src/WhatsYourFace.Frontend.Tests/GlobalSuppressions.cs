﻿// <copyright file="GlobalSuppressions.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Redundancies in Symbol Declarations",
    "RECS0154:Parameter is never used",
    Justification = "Incorrectly runs on auto-generated unit-test entry point",
    Scope = "type",
    Target = "~T:AutoGeneratedProgram")]

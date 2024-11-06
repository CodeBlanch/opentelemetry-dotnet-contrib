// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

#if NET
using System.Diagnostics.CodeAnalysis;
#endif
using OpenTelemetry.Instrumentation.SqlClient.Implementation;

namespace OpenTelemetry.Instrumentation.SqlClient;

/// <summary>
/// SqlClient instrumentation.
/// </summary>
internal sealed class SqlClientInstrumentation : IDisposable
{
    internal const string SqlClientDiagnosticListenerName = "SqlClientDiagnosticListener";
#if NET
    internal const string SqlClientTrimmingUnsupportedMessage = "Trimming is not yet supported with SqlClient instrumentation.";
#endif
#if NETFRAMEWORK
    private static readonly SqlEventSourceListener SqlEventSourceListener;
#else
    private static readonly HashSet<string> DiagnosticSourceEvents =
    [
        "System.Data.SqlClient.WriteCommandBefore",
        "Microsoft.Data.SqlClient.WriteCommandBefore",
        "System.Data.SqlClient.WriteCommandAfter",
        "Microsoft.Data.SqlClient.WriteCommandAfter",
        "System.Data.SqlClient.WriteCommandError",
        "Microsoft.Data.SqlClient.WriteCommandError"
    ];

    private static readonly Func<string, object?, object?, bool> IsEnabled = (eventName, _, _)
        => DiagnosticSourceEvents.Contains(eventName);

    private static readonly DiagnosticSourceSubscriber DiagnosticSourceSubscriber;
#endif

    private readonly IDisposable? unapply;

    static SqlClientInstrumentation()
    {
#if NETFRAMEWORK
        SqlEventSourceListener = new SqlEventSourceListener();
#else
        DiagnosticSourceSubscriber = new DiagnosticSourceSubscriber(
           name => new SqlClientDiagnosticListener(name),
           listener => listener.Name == SqlClientDiagnosticListenerName,
           IsEnabled,
           SqlClientInstrumentationEventSource.Log.UnknownErrorProcessingEvent);
        DiagnosticSourceSubscriber.Subscribe();
#endif
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlClientInstrumentation"/> class.
    /// </summary>
    /// <param name="options">Configuration options for sql instrumentation.</param>
#if NET
    [RequiresUnreferencedCode(SqlClientTrimmingUnsupportedMessage)]
#endif
    public SqlClientInstrumentation(
        SqlClientTraceInstrumentationOptions? options = null)
    {
        this.unapply = options?.Apply();
    }

    public void Dispose()
    {
        this.unapply?.Dispose();
    }
}

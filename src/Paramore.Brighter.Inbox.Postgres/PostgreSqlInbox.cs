﻿#region Licence

/* The MIT License (MIT)
Copyright © 2020 Ian Cooper <ian.cooper@yahoo.co.uk>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

#endregion

using System;
using System.Data;
using Npgsql;
using Paramore.Brighter.Logging;
using Paramore.Brighter.Observability;
using Paramore.Brighter.PostgreSql;

namespace Paramore.Brighter.Inbox.Postgres;

public class PostgreSqlInbox : RelationalDatabaseInbox
{
    public PostgreSqlInbox(IAmARelationalDatabaseConfiguration configuration, IAmARelationalDbConnectionProvider connectionProvider)
        : base(DbSystem.Postgresql, configuration, connectionProvider, 
            new PostgreSqlQueries(), ApplicationLogging.CreateLogger<PostgreSqlInbox>())
    {
    }

    public PostgreSqlInbox(IAmARelationalDatabaseConfiguration configuration)
        : this(configuration, new PostgreSqlConnectionProvider(configuration))
    {
    }


    /// <inheritdoc />
    protected override bool IsExceptionUniqueOrDuplicateIssue(Exception ex)
    {
        return ex is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }

    /// <inheritdoc />
    protected override IDbDataParameter CreateSqlParameter(string parameterName, object? value)
    {
        return new NpgsqlParameter { ParameterName = parameterName, Value = value ?? DBNull.Value };
    }
}

﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SeqCli.Cli.Features;
using SeqCli.Connection;
using Serilog;

namespace SeqCli.Cli.Commands.ApiKey
{
    [Command("apikey", "list", "List of API Keys", Example =
        "seqcli apikey list")]
    class ListCommand : Command
    {
        private readonly SeqConnectionFactory _connectionFactory;
        private readonly ConnectionFeature _connection;

        public ListCommand(SeqConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _connection = Enable<ConnectionFeature>();
        }

        protected override async Task<int> Run()
        {
            var connection = _connectionFactory.Connect(_connection);

            var apiKeys = await connection.ApiKeys.ListAsync();
            Log.Debug("Retrieved ApiKeys {@ApiKeys}", apiKeys);
            var data = apiKeys.Select(a => new
            {
                a.Title,
                a.Id,
                a.Token,
                a.MinimumLevel,
                a.AppliedProperties,
                a.CanActAsPrincipal,
                a.InputFilter,
                a.UseServerTimestamps,
                a.IsDefault
            });

            foreach (var apiKey in data)
            {
                var apiKeyString = JsonConvert.SerializeObject(apiKey);

                Console.WriteLine(apiKeyString);
            }
            return 0;
        }
    }
}
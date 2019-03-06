﻿using Aggregates.Extensions;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aggregates.Sagas
{
    
    public class CommandSaga
    {
        private IMessageHandlerContext _context;
        private string _sagaId;
        private Messages.IMessage _originating;
        private List<Messages.ICommand> _commands;
        private List<Messages.ICommand> _abortCommands;

        internal CommandSaga(IMessageHandlerContext context, string sagaId, Messages.IMessage originating)
        {
            _context = context;
            _sagaId = sagaId;
            _originating = originating;
            _commands = new List<Messages.ICommand>();
            _abortCommands = new List<Messages.ICommand>();

            if (string.IsNullOrEmpty(Configuration.Settings.CommandDestination))
                throw new ArgumentException($"Usage of SAGA depends on Configuration.SetCommandDestination");
        }

        public CommandSaga Command(Messages.ICommand command)
        {
            _commands.Add(command);
            return this;
        }

        public CommandSaga OnAbort(Messages.ICommand command)
        {
            _abortCommands.Add(command);
            return this;
        }

        public Task Start()
        {
            var message = new StartCommandSaga
            {
                SagaId = _sagaId,
                Originating = _originating,
                Commands = _commands.ToArray(),
                AbortCommands = _abortCommands.ToArray(),
            };


            var options = new SendOptions();
            options.SetDestination(Configuration.Settings.CommandDestination);
            options.SetHeader(Defaults.RequestResponse, "0");
            options.SetHeader(Defaults.SagaHeader, message.SagaId);

            return _context.Send(message, options);
        }

    }
    
}

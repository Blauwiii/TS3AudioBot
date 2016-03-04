﻿namespace TS3AudioBot.CommandSystem
{
	using System.Collections.Generic;
	using System.Linq;

	public class CommandGroup : ICommand
	{
		readonly IDictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

		public void AddCommand(string name, ICommand command) => commands.Add(name, command);
		public void RemoveCommand(string name) => commands.Remove(name);
		public void RemoveCommand(ICommand command)
		{
			var commandPair = commands.Single(kvp => kvp.Value == command);
			commands.Remove(commandPair);
		}
		public bool ContainsCommand(string name) => commands.ContainsKey(name);

		public virtual ICommandResult Execute(ExecutionInformation info, IEnumerable<ICommand> arguments, IEnumerable<CommandResultType> returnTypes)
		{
			if (!arguments.Any())
			{
				if (returnTypes.Contains(CommandResultType.Command))
					return new CommandCommandResult(this);
				throw new CommandException("Expected a string");
			}

			var result = arguments.First().Execute(info, new ICommand[] {}, new CommandResultType[] { CommandResultType.String });

			var commandResults = XCommandSystem.FilterList(commands.Keys, ((StringCommandResult)result).Content);
			if (commandResults.Skip(1).Any())
				throw new CommandException("Ambiguous command, possible names: " + string.Join(", ", commandResults));

			return commands[commandResults.First()].Execute(info, arguments.Skip(1), returnTypes);
		}
	}
}

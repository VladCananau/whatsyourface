// <copyright file="CmdBase.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd.Commands
{
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Dawn;

    public abstract class CmdBase
    {
        protected CmdBase(Wyfcmd.IConsole console, IFileSystem fileSystem)
        {
            Guard.Argument(console, nameof(console)).NotNull();
            Guard.Argument(fileSystem, nameof(fileSystem)).NotNull();

            this.Console = console;
            this.FileSystem = fileSystem;
        }

        protected CmdBase()
        {
        }

        protected Wyfcmd.IConsole Console { get; }

        protected IFileSystem FileSystem { get; }
    }

    public abstract class CmdBase<TArgs> : CmdBase
    {
        protected CmdBase(Wyfcmd.IConsole console, IFileSystem fileSystem)
            : base(console, fileSystem)
        {
        }

        protected CmdBase()
        {
        }

        public virtual void Execute()
        {
            this.ExecuteAsync().GetAwaiter().GetResult();
        }

        public abstract Task ExecuteAsync();

        public virtual void Execute(TArgs args)
        {
            this.ExecuteAsync(args).GetAwaiter().GetResult();
        }

        public abstract Task ExecuteAsync(TArgs args);
    }

    public abstract class CmdBase<TArgs, TResult> : CmdBase
    {
        protected CmdBase(Wyfcmd.IConsole console, IFileSystem fileSystem)
            : base(console, fileSystem)
        {
        }

        protected CmdBase()
        {
        }

        public virtual TResult Execute()
        {
            return this.ExecuteAsync().GetAwaiter().GetResult();
        }

        public abstract Task<TResult> ExecuteAsync();

        public virtual TResult Execute(TArgs args)
        {
            return this.ExecuteAsync(args).GetAwaiter().GetResult();
        }

        public abstract Task<TResult> ExecuteAsync(TArgs args);
    }
}

﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// A non-buildable <see cref="IHostBuilder"/> for <see cref="MauiAppBuilder"/>.
	/// Use <see cref="MauiAppBuilder.Build"/> to build the <see cref="MauiAppBuilder"/>.
	/// </summary>
	internal sealed class ConfigureHostBuilder : IHostBuilder
	{
		private readonly ConfigurationManager _configuration;
		private readonly IServiceCollection _services;
		private readonly HostBuilderContext _context;

		private readonly List<Action<IHostBuilder>> _operations = new();

		internal ConfigureHostBuilder(HostBuilderContext context, ConfigurationManager configuration, IServiceCollection services)
		{
			_configuration = configuration;
			_services = services;
			_context = context;
		}

		/// <inheritdoc />
		public IDictionary<object, object> Properties => _context.Properties;

		IHost IHostBuilder.Build()
		{
			throw new NotSupportedException($"Call {nameof(MauiAppBuilder)}.{nameof(MauiAppBuilder.Build)}() instead.");
		}

		/// <inheritdoc />
		public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
		{
			// Run these immediately so that they are observable by the imperative code
			configureDelegate(_context, _configuration);
			return this;
		}

		/// <inheritdoc />
		public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
		{
			if (configureDelegate is null)
			{
				throw new ArgumentNullException(nameof(configureDelegate));
			}

			_operations.Add(b => b.ConfigureContainer(configureDelegate));
			return this;
		}

		/// <inheritdoc />
		public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
		{
			var previousApplicationName = _configuration[HostDefaults.ApplicationKey];
			var previousContentRoot = _configuration[HostDefaults.ContentRootKey];
			var previousEnvironment = _configuration[HostDefaults.EnvironmentKey];

			// Run these immediately so that they are observable by the imperative code
			configureDelegate(_configuration);

			// Disallow changing any host settings this late in the cycle, the reasoning is that we've already loaded the default configuration
			// and done other things based on environment name, application name or content root.
			if (!string.Equals(previousApplicationName, _configuration[HostDefaults.ApplicationKey], StringComparison.OrdinalIgnoreCase))
			{
				throw new NotSupportedException("The application name changed. Changing the host configuration is not supported");
			}

			if (!string.Equals(previousContentRoot, _configuration[HostDefaults.ContentRootKey], StringComparison.OrdinalIgnoreCase))
			{
				throw new NotSupportedException("The content root changed. Changing the host configuration is not supported");
			}

			if (!string.Equals(previousEnvironment, _configuration[HostDefaults.EnvironmentKey], StringComparison.OrdinalIgnoreCase))
			{
				throw new NotSupportedException("The environment changed. Changing the host configuration is not supported");
			}

			return this;
		}

		/// <inheritdoc />
		public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
		{
			// Run these immediately so that they are observable by the imperative code
			configureDelegate(_context, _services);
			return this;
		}

		/// <inheritdoc />
		public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull
		{
			if (factory is null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			_operations.Add(b => b.UseServiceProviderFactory(factory));
			return this;
		}

		/// <inheritdoc />
		public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) where TContainerBuilder : notnull
		{
			if (factory is null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			_operations.Add(b => b.UseServiceProviderFactory(factory));
			return this;
		}

		internal void RunDeferredCallbacks(IHostBuilder hostBuilder)
		{
			foreach (var operation in _operations)
			{
				operation(hostBuilder);
			}
		}
	}
}

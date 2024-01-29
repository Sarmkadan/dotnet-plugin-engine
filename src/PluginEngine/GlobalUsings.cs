// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

global using System.Reflection;
global using System.Text;
global using System.Xml;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using PluginEngine.Caching;
global using PluginEngine.Configuration;
global using PluginEngine.Data.Repositories;
global using PluginEngine.Domain.Entities;
global using PluginEngine.Events;
global using PluginEngine.Exceptions;
global using PluginEngine.Execution;
global using PluginEngine.Formatters;
global using PluginEngine.Integration;
global using PluginEngine.Middleware;
global using PluginEngine.Results;
global using PluginEngine.Services.Abstractions;
global using PluginEngine.Utils.Helpers;
global using PluginEngine.Utils.Validators;

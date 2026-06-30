# Pulse

Pulse is a project, roadmap, task, and collaboration management platform. It supports project registration, execution tracking, task updates, roadmap governance, template setup, reporting, notifications, attachments, chat, and role-based visibility.

The solution is primarily a classic ASP.NET MVC / ASP.NET Web API application backed by Oracle, with a React migration workspace and a Python operational console for notification/email workflows.

## Repository Structure

```text
docs/                     Documentation and UAT scenarios
scripts/                  Utility scripts
src/
  Pulse.sln               Main Visual Studio solution
  Pulse.Web/              ASP.NET MVC web application
  Pulse.Api/              ASP.NET Web API host
  Pulse.Auth.Identity/    Authentication and identity components
  Pulse.Core/             Core domain logic
  Pulse.Services/         Application services
  Pulse.Infrastructure/   Oracle/Dapper data access and infrastructure
  Pulse.SharedUtilities/  Shared utility code
  Pulse.ViewModels/       MVC/view model types
  Pulse.DataTransformationObjects/
                           DTOs used across application boundaries
  Pulse.EventHandlers/    Event handling components
  Pulse.MediaWiki/        MediaWiki integration components
  Pulse.Prompt/           Prompt-related components
  pulse.react/            Vite + React + TypeScript migration workspace
  pulse.console/          Python operational console for email tasks
  JwtKeyGenerator/        Utility for generating JWT signing keys
  TableSchema/            Oracle schema artifacts
tests/                    Reserved for tests

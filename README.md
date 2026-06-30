# Pulse

Pulse is a project, roadmap, task, and collaboration management platform. It supports project registration, execution tracking, task updates, milestone management, roadmap governance, template setup, reporting, notifications, attachments, chat, and role-based access.

The application is primarily a classic ASP.NET MVC / ASP.NET Web API solution backed by Oracle. The repository also includes a Vite + React migration workspace, a Python operational console for notification/email workflows, database schema artifacts, and supporting utilities.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Repository Structure](#repository-structure)
- [Applications and Components](#applications-and-components)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Database](#database)
- [Build and Run](#build-and-run)
- [React Workspace](#react-workspace)
- [Python Console](#python-console)
- [Testing and Validation](#testing-and-validation)
- [Documentation](#documentation)
- [Security Notes](#security-notes)
- [Development Workflow](#development-workflow)
- [Troubleshooting](#troubleshooting)
- [License](#license)

## Overview

Pulse is designed for managing project execution and visibility across multiple user roles, including:

- Daily users / task owners
- Project owners
- Project members
- Template managers
- Administrators
- Report viewers

The system includes workflows for:

- Creating and maintaining projects
- Managing project roadmaps and milestones
- Assigning and updating tasks
- Tracking project execution
- Managing project members
- Maintaining templates and forms
- Uploading and downloading attachments
- Sending project and task notifications
- Supporting chat/collaboration flows
- Reporting on project and task status
- Enforcing role-based access and visibility

## Features

- Project registration and maintenance
- Roadmap and milestone tracking
- Project task management
- Project member management
- Template and form configuration
- Field and metadata management
- File attachments
- Notification delivery
- Chat and collaboration features
- Admin user/module/access management
- Oracle-backed persistence
- ASP.NET MVC web interface
- ASP.NET Web API backend
- React migration workspace
- Python-based notification console
- JWT key generation utility

## Technology Stack

### Backend and Web

- .NET Framework 4.7.2
- ASP.NET MVC 5
- ASP.NET Web API
- ASP.NET SignalR
- Autofac
- OWIN
- JWT bearer authentication
- Cookie authentication
- log4net

### Data

- Oracle Database
- Oracle Managed Data Access
- Dapper
- Dapper.Oracle

### Frontend

- Razor views
- jQuery
- Bootstrap
- DataTables
- Vite
- React
- TypeScript

### Operations / Utilities

- Python 3.11+
- python-oracledb
- SMTP email delivery
- Local SQLite state for selected console workflows

## Repository Structure

```text
docs/
  github-guide.html
  Pulse_User_Acceptance_Test_Scenarios.md
  uat.html

scripts/

src/
  Pulse.sln

  Pulse.Web/
    ASP.NET MVC web application

  Pulse.Api/
    ASP.NET Web API application

  Pulse.Auth.Identity/
    Authentication and identity components

  Pulse.Core/
    Core application/domain logic

  Pulse.Services/
    Application service layer

  Pulse.Infrastructure/
    Data access, infrastructure, and Oracle/Dapper integration

  Pulse.SharedUtilities/
    Shared helper and utility code

  Pulse.ViewModels/
    View model types used by the web layer

  Pulse.DataTransformationObjects/
    DTOs used across application boundaries

  Pulse.EventHandlers/
    Event handling components

  Pulse.MediaWiki/
    MediaWiki integration components

  Pulse.Prompt/
    Prompt-related components

  pulse.react/
    Vite + React + TypeScript migration workspace

  pulse.console/
    Python operational console for notification/email workflows

  JwtKeyGenerator/
    Utility for generating JWT signing keys

  TableSchema/
    Oracle schema artifacts

tests/
  Reserved for tests

## Repository Structure



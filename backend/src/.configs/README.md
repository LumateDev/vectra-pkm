# Vectra Configurations

## File structure

- `appsettings.API.json' - The main configuration for the API (Production)
- `appsettings.API.Development.json` - Configuration for the API (Development)
- `appsettings.Secrets.example.json` is an example of a file with secrets.
- `appsettings.Secrets.json` - A file with secrets

## Setup for development

1. Copy the `appsettings.Secrets.example.json` to `appsettings.Secrets.json`
2. Fill in the real secret values in `appsettings.Secrets.json`
3. Make sure that `appsettings.Secrets.json` is added to `.gitignore`
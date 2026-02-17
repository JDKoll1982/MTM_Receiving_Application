# Serilog in MTM Receiving Application

Last Updated: 2026-02-10

## What it is (in plain words)
Serilog is the app's built-in note taker. It writes a running diary of what the app is doing so issues can be found later.

## Where the notes are saved
- Logs are saved in a folder named logs next to the app.
- A new file is created each day.
- The app keeps about 30 days of log files and then replaces the older ones.

## What shows up in the logs
- Normal activity (info)
- Warnings when something seems off
- Errors when something fails
- Each entry includes date and time, and where it came from in the app

## How the app decides how much to write
The app reads its logging settings when it starts:
- appsettings.json is the main file for normal use.
- appsettings.Development.json is for development and writes more detail.

In appsettings.json, the Default level controls how chatty the logs are:
- Debug = very detailed
- Information = normal day-to-day detail
- Warning = only issues and important events
- Error = only failures

The Microsoft and System settings are there to keep noise down from Windows and .NET components.

## How to use the logs
- If the app acts strange, open the latest file in the logs folder and read the last few lines.
- When reporting a problem, share the latest log file so the issue can be traced.

## If you need more detail
- Switch the Default level to Debug in appsettings.Development.json for test machines.
- Switch it back to Information for normal daily use so files do not grow too fast.

## Quick checklist
- Logs folder exists next to the app
- Latest file has today’s date
- Errors are captured in the file when an issue happens

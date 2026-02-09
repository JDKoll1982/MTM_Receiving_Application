# How It Works at a Glance

Last Updated: 2025-01-19

## Overview
- Core services expose shared capabilities to other modules
- Behaviors run before and after requests to add logging and validation
- Models and converters standardize data and UI binding

## Typical Flow
- ViewModel calls a service
- Behaviors run around the request pipeline
- Service uses helpers and models to complete the action

# Settable Objects Inventory (Template)

Use this template in every feature module to document settings.

| Setting Key | Data Type | Scope | UI Control | Description | Permission | Default | Notes |
| ----------- | --------- | ----- | ---------- | ----------- | ---------- | ------- | ----- |
| Feature:ExampleSetting | String | User | TextBox | Example user setting | User | "" | Replace with actual settings |

## Notes

- Keys must be unique within the module.
- Scope values: System or User.
- Permission levels: User, Supervisor, Admin, Developer.
- Sensitive values must be masked in UI and changed via dedicated dialog.

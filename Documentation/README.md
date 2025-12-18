# MTM Receiving Application Documentation

Welcome to the comprehensive documentation for the MTM Receiving Label Application.

## 📂 Directory Structure

This documentation is organized into the following categories:

### 🔧 [Features/](Features/)
Documentation for implemented features, organized into three guide types:
- **UserGuide**: End-user instructions and troubleshooting
- **DeveloperGuide**: Technical implementation details and architecture  
- **CopilotGuide**: AI assistant context and code generation hints

**Available Features**:
- [Authentication](Features/Authentication/) - User login, PIN authentication, session management
- [ConfigurableSettings](Features/ConfigurableSettings/) - System and user settings configuration
- [DatabaseAdmin](Features/DatabaseAdmin/) - Database administration and maintenance
- [WindowSizing](Features/WindowSizing/) - Window and dialog sizing strategy
- [ReusableServices](Features/ReusableServices/) - Shared services for MTM projects

### 🔮 [FuturePlans/](FuturePlans/)
Planned features and design specifications:
- [ReceivingLabels](FuturePlans/ReceivingLabels/) - Receiving label workflow and data models
- [DunnageLabels](FuturePlans/DunnageLabels/) - Dunnage label generation
- [RoutingLabels](FuturePlans/RoutingLabels/) - Internal routing label functionality
- [GoogleSheetsReplacement](FuturePlans/GoogleSheetsReplacement/) - Migration from Google Sheets

### 📡 [API/](API/)
API documentation (placeholder - will be added when REST APIs are implemented)

### 🚀 [Deployment/](Deployment/)
Deployment guides (placeholder - will be added for production rollout)

### 🔍 [Troubleshooting/](Troubleshooting/)
Common issues and solutions (placeholder - will be documented as issues arise)

### 🏗️ [Architecture/](Architecture/)
System architecture diagrams and design documents (placeholder)

### 🔗 [InforVisual/](InforVisual/)
Infor Visual ERP database reference files and transaction macros

---

## 📖 Guide Types Explained

Each implemented feature includes three types of guides:

### 1. **UserGuide** 📘
- **Audience**: End users, operators, administrators
- **Content**: How to use the feature, step-by-step instructions, troubleshooting
- **Format**: Screenshots, examples, FAQs
- **Example**: How to log in, what to do if you can't access the system

### 2. **DeveloperGuide** 📗
- **Audience**: Software developers, system architects
- **Content**: Technical implementation, code architecture, database schemas, APIs
- **Format**: Code examples, SQL scripts, class diagrams
- **Example**: Authentication flow implementation, database stored procedures

### 3. **CopilotGuide** 📙
- **Audience**: AI assistants (GitHub Copilot, ChatGPT, etc.)
- **Content**: Key classes/methods, common tasks, code generation patterns, testing strategies
- **Format**: Concise reference, code templates, best practices
- **Example**: How to authenticate a user programmatically, test patterns

---

## 🚀 Quick Links

### For End Users
- [How to Log In](Features/Authentication/Authentication-UserGuide.md)
- [Troubleshooting Login Issues](Features/Authentication/Authentication-UserGuide.md#troubleshooting)

### For Developers
- [Authentication Architecture](Features/Authentication/Authentication-DeveloperGuide.md)
- [Database Administration](Features/DatabaseAdmin/DatabaseAdmin-DeveloperGuide.md)
- [Reusable Services Setup](Features/ReusableServices/ReusableServices-DeveloperGuide.md)

### For Database Administrators
- [Managing Users and Workstations](Features/DatabaseAdmin/DatabaseAdmin-UserGuide.md)
- [Database Schemas and Queries](Features/DatabaseAdmin/DatabaseAdmin-DeveloperGuide.md)

---

## 🤝 Contributing to Documentation

### Adding New Documentation
1. Place documentation in the appropriate directory (Features/, FuturePlans/, etc.)
2. Follow the three-guide pattern for implemented features (UserGuide, DeveloperGuide, CopilotGuide)
3. Use naming convention: `FeatureName-GuideType.md`
4. Update this README with links to new documentation

### Documentation Standards
- Use clear, descriptive headings
- Include code examples with syntax highlighting
- Add screenshots for UI-related documentation
- Keep technical details in DeveloperGuide, not UserGuide
- Update documentation when features change

### Style Guidelines
- Write in clear, concise English
- Use bullet points for lists
- Include table of contents for long documents
- Add "Last Updated" date at bottom of documents
- Use markdown formatting consistently

---

## 📝 Document Versions

| Document Category | Last Updated | Version |
|-------------------|--------------|---------|
| Authentication | December 2025 | 1.0.0 |
| ConfigurableSettings | December 2025 | 1.0.0 |
| DatabaseAdmin | December 2025 | 1.0.0 |
| WindowSizing | January 2025 | 1.0.0 |
| ReusableServices | December 2025 | 1.0.0 |

---

## 🆘 Support & Contact

For questions about this documentation or the MTM Receiving Application:
- **Technical Issues**: Contact IT Support
- **Documentation Updates**: Submit pull request or contact development team
- **Feature Requests**: Create issue in project repository

---

**Documentation Structure Version**: 1.0.0  
**Last Updated**: December 2025  
**Maintained By**: MTM Development Team

# Static Web Forms Project

A complete static web forms system built with Blazor WASM, optimized for GitHub Pages deployment. This project allows non-technical users to create and manage dynamic forms with document generation capabilities.

## Features

- **Dynamic Form Generation**: Forms are generated from JSON configuration files
- **Document Templates**: Markdown templates with placeholder replacement
- **Multiple Field Types**: Text, email, textarea, number, date, and choice fields
- **Form Validation**: Client-side validation for required fields and email format
- **Document Preview**: Real-time preview of generated documents
- **Download Support**: Download completed documents as Markdown files
- **Responsive Design**: Modern, mobile-friendly UI
- **Extensible Architecture**: Ready for backend integration

## Project Structure

```
/modules          - Markdown document templates with {{placeholders}}
/json             - JSON configuration files for each module
/wwwroot          - Blazor WASM static files (HTML, CSS, JS)
/backend          - Backend API documentation (for future implementation)
/docs             - User guide for non-technical users
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- A code editor (Visual Studio, VS Code, or Rider)

### Building the Project

1. Clone or download this repository
2. Open a terminal in the project directory
3. Run:
   ```bash
   dotnet restore
   dotnet build
   ```

### Running Locally

```bash
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`

### Deploying to GitHub Pages

1. Build the project for release:
   ```bash
   dotnet publish -c Release
   ```

2. The output will be in `bin/Release/net8.0/publish/wwwroot/`

3. Copy the contents of `wwwroot` to your GitHub Pages repository

4. Configure GitHub Pages to serve from the root directory

5. Update the `base` tag in `index.html` if your site is in a subdirectory:
   ```html
   <base href="/your-repo-name/" />
   ```

## Creating Modules

See the [User Guide](docs/USER_GUIDE.md) for detailed instructions on creating and modifying form modules.

### Quick Start

1. Create a JSON file in `/json` (e.g., `myForm.json`)
2. Create a Markdown template in `/modules` (e.g., `myForm.md`)
3. Use `{{fieldName}}` placeholders in the template
4. The form will automatically appear in the module selector

## Example Module

The project includes an example module (`exampleModule`) that demonstrates all field types and features. Check:
- `/json/exampleModule.json` - Form configuration
- `/modules/exampleModule.md` - Document template

## Backend Integration

The frontend is designed to work with a backend service running on Raspberry Pi. See [Backend Documentation](backend/README.md) for API contract and implementation examples.

The backend should:
- Receive form submissions via POST `/api/submit`
- Generate documents (especially for Word format)
- Send emails with attachments via SMTP

## Technology Stack

- **Frontend**: Blazor WebAssembly (.NET 8)
- **Styling**: Custom CSS with modern design
- **Markdown Processing**: Markdig library
- **Deployment**: GitHub Pages (static hosting)

## Development

### Adding New Field Types

1. Add the question type to `Question.QuestionType` in the JSON schema
2. Add a case in `FormField.razor` to handle the new type
3. Update the user guide with the new field type

### Extending Functionality

The codebase is structured for easy extension:
- **Services**: Add new services in `/Services`
- **Components**: Add reusable components in `/Components`
- **Models**: Add data models in `/Models`

## File Organization

- `/Models` - Data models (ModuleConfig, Question, FormSubmission)
- `/Services` - Business logic (ModuleService, DocumentService, FormSubmissionService)
- `/Components` - Reusable Blazor components
- `/Pages` - Page components (Index, Form)
- `/Shared` - Shared layout components

## License

This project is provided as-is for use in your organization.

## Support

For questions about creating modules, see the [User Guide](docs/USER_GUIDE.md).

For technical issues or feature requests, contact your development team.

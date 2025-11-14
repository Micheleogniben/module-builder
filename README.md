# Module Builder

**Module Builder** is a static web system for creating and managing dynamic form modules. It allows non-technical users to create custom forms simply by creating JSON files and Markdown templates, without needing to write code.

## 🚀 Key Features

- ✅ **Dynamic Forms**: Automatically generates forms from JSON files
- ✅ **Customizable Templates**: Use Markdown templates with placeholders to generate documents
- ✅ **Easy to Use**: Intuitive interface for non-technical users
- ✅ **Multiple Field Types**: Text, email, numbers, dates, multiple choice, and more
- ✅ **Automatic Validation**: Client-side validation for required fields and email format
- ✅ **Document Preview**: Real-time preview of generated documents
- ✅ **Download Documents**: Download completed documents as Markdown files
- ✅ **Responsive Design**: Works perfectly on desktop and mobile
- ✅ **Backend Ready**: Extensible architecture for future API integration

## 📋 Requirements

- .NET 8 SDK
- A modern browser
- (Optional) A web server for deployment (e.g., GitHub Pages)

## 🎯 Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/module-builder.git
   cd module-builder
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Open your browser** to the URL shown in the terminal (usually `https://localhost:5001`)

## 📚 Documentation

- **[User Guide](docs/USER_GUIDE.md)** - Complete guide for creating and modifying modules (for non-technical users)
- **[Technical Setup](SETUP.md)** - Technical information for developers
- **[Next Steps](NEXT_STEPS.md)** - Future steps and improvements
- **[Backend API](backend/README.md)** - API documentation for backend integration

## 🎨 How It Works

1. **Create a Module**: Add a JSON file in `/json` and a Markdown template in `/modules`
2. **Configure Questions**: Define form fields in the JSON file
3. **Create the Template**: Use `{{fieldName}}` as placeholders in the Markdown template
4. **Users Fill Forms**: The form is automatically generated from the JSON
5. **Document Generated**: Answers replace placeholders in the template

## 📁 Project Structure

```
module-builder/
├── modules/          # Markdown templates with {{placeholders}}
├── json/             # JSON module configurations
├── wwwroot/          # Static files (HTML, CSS, JS)
├── backend/          # Backend API documentation
├── docs/             # User guides
└── Components/       # Blazor components
```

## 🛠️ Technologies Used

- **Blazor WebAssembly** (.NET 8) - Frontend framework
- **Markdig** - Markdown processing
- **Custom CSS** - Modern and responsive design

## 📝 Example

The project includes an example module (`exampleModule`) that demonstrates all features. Check:
- `/json/exampleModule.json` - Form configuration
- `/modules/exampleModule.md` - Document template

## 🤝 Contributing

Contributions are welcome! For more information, see the [User Guide](docs/USER_GUIDE.md) to create new modules.

## 📄 License

This project is released under the MIT License. See the [LICENSE](LICENSE) file for details.

## 🆘 Support

For questions or issues:
- Check the [User Guide](docs/USER_GUIDE.md) for creating modules
- See [Technical Setup](SETUP.md) for technical issues
- Open an issue on GitHub for bugs or feature requests

---

**Built with ❤️ using Blazor WebAssembly**

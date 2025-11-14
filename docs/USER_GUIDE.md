# User Guide: Creating and Modifying Form Modules

This guide will help you create new form modules or modify existing ones. No programming knowledge is required!

## Table of Contents

1. [Understanding the System](#understanding-the-system)
2. [Creating a New Module](#creating-a-new-module)
3. [Modifying an Existing Module](#modifying-an-existing-module)
4. [Creating Document Templates](#creating-document-templates)
5. [File Naming Conventions](#file-naming-conventions)
6. [Troubleshooting](#troubleshooting)

## Understanding the System

Each form module consists of two files:

1. **JSON Configuration File** (`/json/moduleName.json`) - Defines the questions and form fields
2. **Document Template** (`/modules/moduleName.md`) - The template that gets filled with answers

### How It Works

1. Users fill out the form based on questions in the JSON file
2. Their answers replace placeholders (like `{{firstName}}`) in the template
3. A completed document is generated and can be downloaded

## Creating a New Module

### Step 0: Add Module to Index (Optional but Recommended)

1. Open `/json/modules.json`
2. Add your new module ID to the "modules" array:
   ```json
   {
     "modules": [
       "exampleModule",
       "yourNewModule"
     ]
   }
   ```
   
   **Note:** If you don't add it here, you'll need to modify the code. Adding it to modules.json is the easiest way!

### Step 1: Create the JSON Configuration File

1. Navigate to the `/json` folder
2. Create a new file named `yourModuleName.json` (use lowercase, no spaces)
3. Copy the structure below and fill it in:

```json
{
  "moduleId": "yourModuleName",
  "title": "Your Module Title",
  "questions": [
    {
      "fieldName": "field1",
      "questionType": "text",
      "questionText": "What is your question?",
      "required": true,
      "placeholder": "Optional placeholder text"
    }
  ]
}
```

### Step 2: Add Questions

For each question, you need to specify:

- **fieldName**: A unique identifier (use lowercase, no spaces, e.g., "firstName", "emailAddress")
- **questionType**: One of the following:
  - `text` - Single line text input
  - `email` - Email address input (with validation)
  - `textarea` - Multi-line text input
  - `number` - Number input
  - `date` - Date picker
  - `choice` - Radio button selection (requires "options" field)
- **questionText**: The question text shown to users
- **required**: `true` or `false` - Whether the field must be filled
- **placeholder**: (Optional) Hint text shown in the field
- **helpText**: (Optional) Additional help text below the field
- **options**: (Required for "choice" type) Array of options like `["Option 1", "Option 2", "Option 3"]`

#### Example: Complete JSON with Different Question Types

```json
{
  "moduleId": "registrationForm",
  "title": "Event Registration Form",
  "questions": [
    {
      "fieldName": "fullName",
      "questionType": "text",
      "questionText": "What is your full name?",
      "required": true,
      "placeholder": "John Doe"
    },
    {
      "fieldName": "email",
      "questionType": "email",
      "questionText": "What is your email address?",
      "required": true,
      "placeholder": "john@example.com",
      "helpText": "We'll send you a confirmation email"
    },
    {
      "fieldName": "age",
      "questionType": "number",
      "questionText": "How old are you?",
      "required": false
    },
    {
      "fieldName": "eventDate",
      "questionType": "date",
      "questionText": "Which date would you prefer?",
      "required": true
    },
    {
      "fieldName": "mealPreference",
      "questionType": "choice",
      "questionText": "What is your meal preference?",
      "required": true,
      "options": ["Vegetarian", "Vegan", "Gluten-Free", "No Preference"]
    },
    {
      "fieldName": "specialRequests",
      "questionType": "textarea",
      "questionText": "Any special requests or notes?",
      "required": false,
      "placeholder": "Enter any special requirements...",
      "helpText": "This field is optional"
    }
  ]
}
```

### Step 3: Create the Document Template

1. Navigate to the `/modules` folder
2. Create a new file named `yourModuleName.md` (must match the moduleId from JSON)
3. Write your document template using Markdown format
4. Use `{{fieldName}}` placeholders where you want answers inserted

#### Example Template

```markdown
# Event Registration Confirmation

## Participant Information

**Name:** {{fullName}}

**Email:** {{email}}

**Age:** {{age}}

**Preferred Date:** {{eventDate}}

## Preferences

**Meal Preference:** {{mealPreference}}

## Additional Information

{{specialRequests}}

---

*Registration submitted on {{date}}*
```

**Important Notes:**
- Use double curly braces: `{{fieldName}}`
- The fieldName must exactly match the "fieldName" in your JSON
- You can use any Markdown formatting (headers, bold, lists, etc.)
- Placeholders can appear anywhere in the document

## Modifying an Existing Module

### Adding a New Question

1. Open the JSON file for your module (`/json/moduleName.json`)
2. Add a new question object to the "questions" array
3. Update the template file (`/modules/moduleName.md`) to include the new placeholder if needed

### Removing a Question

1. Remove the question object from the JSON file
2. Remove or leave the placeholder in the template (it will show as empty if not filled)

### Changing Question Text

Simply edit the "questionText" field in the JSON file.

### Making a Field Required/Optional

Change the "required" field from `true` to `false` or vice versa.

## Creating Document Templates

### Markdown Basics

Markdown is a simple text format. Here are the basics:

- **Headers**: Use `#` for main title, `##` for section headers, `###` for sub-sections
- **Bold**: Wrap text in `**bold**` → **bold**
- **Italic**: Wrap text in `*italic*` → *italic*
- **Lists**: Use `-` or `*` for bullet points, numbers for numbered lists
- **Line breaks**: Leave a blank line between paragraphs

### Template Examples

#### Simple Form Letter

```markdown
# Application Letter

Dear Sir/Madam,

My name is {{firstName}} {{lastName}} and I am writing to apply for the position.

I can be reached at {{email}} or by phone at {{phoneNumber}}.

Thank you for your consideration.

Sincerely,
{{firstName}} {{lastName}}
```

#### Structured Document

```markdown
# Registration Form

## Personal Details

- **Name:** {{firstName}} {{lastName}}
- **Email:** {{email}}
- **Phone:** {{phone}}
- **Date of Birth:** {{birthDate}}

## Preferences

**Selected Option:** {{preference}}

**Additional Notes:**
{{comments}}

---

*Submitted on {{date}}*
```

### Tips for Templates

1. **Test your placeholders**: Make sure fieldName matches exactly (case-sensitive)
2. **Use descriptive text**: Add labels and context around placeholders
3. **Format nicely**: Use headers, bold text, and lists to organize information
4. **Handle empty fields**: If a field is optional, consider adding conditional text or leaving it blank

## File Naming Conventions

### Important Rules

1. **Module ID**: Use lowercase letters and numbers only, no spaces (e.g., `registrationForm`, `contactForm2024`)
2. **JSON files**: Must be named exactly as `moduleId.json` in the `/json` folder
3. **Template files**: Must be named exactly as `moduleId.md` in the `/modules` folder
4. **Consistency**: The moduleId in JSON must match the filename (without extension)

### Examples

| Module ID | JSON File | Template File |
|-----------|-----------|---------------|
| `exampleModule` | `json/exampleModule.json` | `modules/exampleModule.md` |
| `registration` | `json/registration.json` | `modules/registration.md` |
| `contactForm` | `json/contactForm.json` | `modules/contactForm.md` |

## Troubleshooting

### Form Not Appearing

- **Check file names**: JSON and template filenames must match the moduleId exactly
- **Check JSON syntax**: Use a JSON validator to ensure your JSON is valid
- **Check file location**: JSON files go in `/json`, templates go in `/modules`

### Placeholders Not Replacing

- **Check fieldName spelling**: Must match exactly (case-sensitive) between JSON and template
- **Check placeholder syntax**: Must be `{{fieldName}}` with double curly braces
- **Check if field is filled**: Optional fields that aren't filled will show as empty

### Validation Errors

- **Required fields**: Make sure all required fields are filled
- **Email format**: Email fields must be valid email addresses
- **Number fields**: Number fields only accept numeric values

### JSON Errors

Common JSON mistakes:
- Missing commas between items
- Using single quotes instead of double quotes
- Trailing commas in arrays/objects
- Missing closing brackets or braces

Use an online JSON validator to check your file.

## Quick Reference: Question Types

| Type | Description | Example |
|------|-------------|---------|
| `text` | Single line text | Name, address |
| `email` | Email with validation | Email address |
| `textarea` | Multi-line text | Comments, notes |
| `number` | Numeric input | Age, quantity |
| `date` | Date picker | Birth date, event date |
| `choice` | Radio buttons | Meal preference, options |

## Need Help?

If you encounter issues:

1. Check the example module files (`exampleModule.json` and `exampleModule.md`) for reference
2. Validate your JSON using an online JSON validator
3. Test with simple examples first before creating complex forms
4. Make sure all file names and fieldNames match exactly

Happy form building!


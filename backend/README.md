# Backend API Documentation

This document describes the API contract that the frontend expects from the backend service running on Raspberry Pi.

## Overview

The backend service should be accessible via a Cloudflare tunnel and handle form submissions, document generation, and email sending.

## API Endpoints

### POST /api/submit

Submits a form with all answers and the generated document.

#### Request Body

```json
{
  "moduleId": "string",
  "moduleTitle": "string",
  "answers": {
    "fieldName1": "value1",
    "fieldName2": "value2"
  },
  "generatedDocument": "string (Markdown content)",
  "submittedAt": "2024-01-01T12:00:00Z"
}
```

#### Response

**Success (200 OK):**
```json
{
  "success": true,
  "message": "Form submitted successfully",
  "documentUrl": "optional-url-to-download-document"
}
```

**Error (400 Bad Request):**
```json
{
  "success": false,
  "error": "Validation error message"
}
```

## Implementation Notes

### C# .NET 8 Minimal API Example

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://your-github-pages-url.github.io")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure email service (SMTP)
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.UseCors("AllowFrontend");

app.MapPost("/api/submit", async (FormSubmission submission, IEmailService emailService) =>
{
    // Validate submission
    if (string.IsNullOrEmpty(submission.ModuleId))
    {
        return Results.BadRequest(new { success = false, error = "Module ID is required" });
    }

    // Generate document (if Word processing needed)
    // For Markdown, the frontend already provides generatedDocument
    
    // Send email
    var emailSent = await emailService.SendEmailAsync(
        to: submission.Answers.ContainsKey("email") ? submission.Answers["email"].ToString() : null,
        subject: $"Form Submission: {submission.ModuleTitle}",
        body: "Please find attached the completed document.",
        attachment: submission.GeneratedDocument,
        attachmentName: $"{submission.ModuleId}.md"
    );

    if (emailSent)
    {
        return Results.Ok(new { success = true, message = "Form submitted and email sent successfully" });
    }
    else
    {
        return Results.StatusCode(500);
    }
})
.WithName("SubmitForm")
.Produces(200)
.Produces(400)
.Produces(500);

app.Run();
```

### Python FastAPI Example

```python
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Dict, Any
from datetime import datetime
import smtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart

app = FastAPI()

# CORS configuration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["https://your-github-pages-url.github.io"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class FormSubmission(BaseModel):
    moduleId: str
    moduleTitle: str
    answers: Dict[str, Any]
    generatedDocument: str
    submittedAt: datetime

@app.post("/api/submit")
async def submit_form(submission: FormSubmission):
    # Validate submission
    if not submission.moduleId:
        raise HTTPException(status_code=400, detail="Module ID is required")
    
    # Send email
    email_sent = send_email(
        to=submission.answers.get("email"),
        subject=f"Form Submission: {submission.moduleTitle}",
        body="Please find attached the completed document.",
        attachment=submission.generatedDocument,
        attachment_name=f"{submission.moduleId}.md"
    )
    
    if email_sent:
        return {"success": True, "message": "Form submitted and email sent successfully"}
    else:
        raise HTTPException(status_code=500, detail="Failed to send email")

def send_email(to: str, subject: str, body: str, attachment: str, attachment_name: str) -> bool:
    # Configure SMTP settings
    smtp_server = "smtp.gmail.com"  # or your SMTP server
    smtp_port = 587
    smtp_user = "your-email@gmail.com"
    smtp_password = "your-app-password"
    
    try:
        msg = MIMEMultipart()
        msg['From'] = smtp_user
        msg['To'] = to
        msg['Subject'] = subject
        msg.attach(MIMEText(body, 'plain'))
        
        # Attach document
        attachment_part = MIMEText(attachment, 'plain')
        attachment_part.add_header('Content-Disposition', f'attachment; filename={attachment_name}')
        msg.attach(attachment_part)
        
        server = smtplib.SMTP(smtp_server, smtp_port)
        server.starttls()
        server.login(smtp_user, smtp_password)
        server.send_message(msg)
        server.quit()
        
        return True
    except Exception as e:
        print(f"Error sending email: {e}")
        return False
```

## Configuration

### Environment Variables

- `SMTP_SERVER`: SMTP server address
- `SMTP_PORT`: SMTP port (usually 587 for TLS)
- `SMTP_USER`: SMTP username/email
- `SMTP_PASSWORD`: SMTP password or app password
- `EMAIL_FROM`: From email address
- `CORS_ORIGINS`: Comma-separated list of allowed origins

### Cloudflare Tunnel Setup

1. Install Cloudflare Tunnel on Raspberry Pi
2. Configure tunnel to expose backend service
3. Update frontend configuration with tunnel URL

## Security Considerations

1. **CORS**: Only allow requests from your GitHub Pages domain
2. **Rate Limiting**: Implement rate limiting to prevent abuse
3. **Input Validation**: Validate all input data
4. **Email Authentication**: Use secure SMTP credentials
5. **HTTPS**: Always use HTTPS for the tunnel

## Future Enhancements

- Support for Word (.docx) document generation
- Database storage for form submissions
- Admin dashboard for viewing submissions
- Conditional question logic
- Multi-step forms
- File upload support


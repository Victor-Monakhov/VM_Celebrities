## Ensure that you have the following installed on your system

.NET SDK 8.0 (https://dotnet.microsoft.com/en-us/download/dotnet/8.0); 
Node.js  20.14.0 version or later (https://nodejs.org/en/download);

## Backend Setup (ASP.NET WebApi)

If you are using Windows OS you can open PowerShall terminal from the current folder and run the command:

`powershell -ExecutionPolicy Bypass -File .\bin\Debug\net8.0\playwright.ps1 install chromium`

Otherwise, open a terminal in your IDE and run command:

`npx playwright install chromium`

Run the project using “http” configuration by clicking “Run” button in your IDE or run the command: 

`dotnet run`

Swagger UI can be accessed at (http://localhost:5263/index.html);
# FullPerla Music

This is a full-stack music streaming application built with F# from top to bottom. It showcases how to build a modern web application using the F# ecosystem, including Fable, Elmish, and Feliz for the client-side, and a standard F# web server for the back-end.

## Features

- Browse a collection of music albums.
- Play music tracks directly in your browser.
- A simple and intuitive user interface.

## Technology Stack

- **F#:** A functional-first, general purpose, and open-source programming language.
- **Fable:** An F# to JavaScript compiler, allowing you to write your client-side application in F#.
- **Elmish:** A functional architecture for building web applications, inspired by The Elm Architecture.
- **Feliz:** A library for building user interfaces in F# with a syntax similar to React.
- **Perla:** A CLI tool for developing Fable applications without requiring Node.js or any other JavaScript tooling.

## Perla Integration

This project uses [Perla](https://github.com/perla-build/perla) to manage the client-side development workflow. Perla simplifies the development experience by providing a complete toolchain for Fable projects, including:

- **Development Server:** A live-reloading development server that automatically recompiles your F# code and refreshes your browser.
- **Dependency Management:** Perla handles F# and JavaScript dependencies without requiring you to interact with `npm` or `yarn`.

The main advantage of using Perla is that you can build a full-stack F# application without ever leaving the .NET ecosystem. This means you don't need to install or configure Node.js, npm, or any other JavaScript-specific tools to build your client-side application.

### Perla Configuration Explained

The `perla.json` file in the `Perla.Client` directory configures Perla's behavior. Here's a breakdown of the features used in this project:

- **`fable`**: This section configures the Fable compiler
- **`mountDirectories`**: This feature maps directories to different paths, allowing for a flexible project structure.
- **`devServer.proxy`**: This proxies API requests from the client-side application to the back-end server, avoiding CORS issues during development. Any request to `/api/` is forwarded to `http://localhost:5000`.
- **`dependencies`**: This section lists the JavaScript dependencies for the project. Perla automatically downloads and manages these dependencies, so you don't need to use `npm` or `yarn`.

## Getting Started

To get started with this project, you'll need to have the .NET SDK installed.

1.  **Clone the repository:**

    ```bash
    git clone https://github.com/your-username/FullPerla.git
    cd FullPerla
    ```

2.  **Run the application:**

    ```bash
    dotnet run --project Perla.Server/Perla.Server.fsproj
    ```

3.  **In a separate terminal, run the Perla development server:**

    ```bash
    perla serve
    ```

You now have the application running locally. Open your browser and navigate to `http://localhost:7331` to see the application in action.

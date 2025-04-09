
# ⚙️ FloByte

**FloByte** is a modern workflow engine and orchestration server built entirely in .NET, designed with AI at its core. From drag-and-drop visual workflows to custom NuGet-based extensions, FloByte empowers developers to build intelligent, modular, and scalable automation solutions fast.

Whether you're connecting services, deploying agents, or scripting actions, FloByte gives you a full-featured, flexible platform to streamline your ideas into production workflows.

---

## ✨ Features

- 🧠 **AI Agent Support** – Define, schedule, and run AI-powered agents inside your workflows
- 🧩 **Extensible via NuGet** – Supports multiple NuGet feeds and custom package logic
- 🖱️ **Drag-and-Drop GUI** – Visual designer for building complex workflows without code
- 💻 **Built-in .NET Code Editor** – Customize logic directly within the platform
- 📦 **Pluggable Storage and Configuration** – Use local, cloud, or custom sources
- 🛠️ **Low-Footprint, Container-Ready** – Lightweight and easy to deploy anywhere

---

## 🚀 Getting Started

```bash
# Clone the repo
git clone https://github.com/[your-org-or-username]/flobyte.git
cd flobyte

# Build the solution
dotnet build

# Run the server (example)
dotnet run --project src/FloByte.Server
```

FloByte will launch locally and expose a web UI for workflow creation and management.

> You’ll need .NET 8+ installed to build and run this project.

---

## 🧱 Architecture

FloByte is designed with modularity and extensibility at its core:

- `FloByte.Server` – Host + API for workflow execution
- `FloByte.Designer` – Blazor-based visual interface
- `FloByte.SDK` – Interfaces and tools for building custom steps, agents, and integrations
- `FloByte.Agent` – Background and AI execution engine

---

## 📦 Custom Extensions

FloByte supports multiple NuGet sources and allows custom packages that define:

- Workflow steps
- AI agents
- Data connectors
- Notification handlers
- And more...

You can publish your own packages and install them through the UI or config.

---

## 🤝 Contributing

We welcome contributions and ideas from the community! If you're interested:

1. Fork the repo
2. Create your feature branch (`git checkout -b feature/awesome`)
3. Commit your changes
4. Push and open a PR

> All contributors must sign a Contributor License Agreement (CLA) before merging. A bot will guide you through it during your first PR.

---

## 📝 License

FloByte is licensed under the **Business Source License 1.1 (BUSL-1.1)**.

You are free to:
- View, fork, and modify the code
- Build your own versions
- Submit contributions

**However, commercial or production use requires a commercial license.**

> This code will convert to [Apache 2.0](https://www.apache.org/licenses/LICENSE-2.0) on **April 9, 2028**.

To inquire about a commercial license, contact: **[your-email@example.com]**

---

## 🌐 Links

- Website: [https://flobyte.io](https://flobyte.io) *(coming soon)*
- Discussions: [GitHub Discussions](https://github.com/devv-guru/FloByte/discussions)
- Issues: [GitHub Issues](https://github.com/devv-guru/FloByte/issues)
- License FAQ: [BUSL-1.1 FAQ](https://mariadb.com/busl/busl-faq/)

---

Built with 💜 by [Deon van Vuuren](https://github.com/deonvv)

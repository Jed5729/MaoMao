# MaoMao

**MaoMao** is a cross-platform streaming application that allows users to search, organize, download, and watch shows and movies from multiple sources through a modular provider system.

Instead of bundling specific content providers directly into the app, MaoMao uses **modules** that can be added by users. These modules act as web scrapers that retrieve metadata and stream sources from supported sites. This approach keeps the core of the application lightweight, and flexible - abstracting away the actual content sourcing.

---

## Features

* **Modular provider system**
  * Add or remove content providers without modifying the core application.

* **Cross-platform client**
  * The application client is built with Avalonia UI for consistent behavior across operating systems and devices.

* **Streaming support**
  * Stream your content directly from providers via the module system.

* **Offline downloads**
  * Download episodes or movies for offline viewing.

* **Watchlists**
  * Save shows or movies you want to watch later, and organize your favorites via a simple and clean watchlist system.

* **Continue watching**
  * Automatically track and save playback progress, so you never lose your spot!

* **Account synchronization**
  * Sign-in to sync watchlists, history, progress, modules, and preferences across devices.

* **Extensible architecture**
  * Providers are implemented as modules, allowing the community to expand supported sources.

---

## Modules

MaoMao does not ship with built-in providers.

Instead, the application loads **modules** that implement provider functionality such as:

* Searching for shows
* Retrieving show and episode metadata
* Fetching available stream and download sources

Modules can be installed by users and loaded at runtime, allowing the application to support many different sources without tightly coupling them to the core client.
Modules are all inter-compatible and will cleanly integrate into the same interface, handling duplicates effectively.

---

## Platforms

The current client targets:

* Windows
* macOS
* Linux
* iOS
* Android
* Web (via WebAssembly build)

Additional platforms may be supported in the future. I'm unsure of any TV operating systems at the moment - I'll cross that bridge when I get there, but I think Avalonia should support them.

---

## Project Status

MaoMao is currently in **active development**.
Core systems such as the module framework, API services, database structure, and client UI are still being made, so there are **no releases yet.**

---

## Architecture Overview

The project is structured into several core components:

* **Client** – Cross-platform UI application
  * Comprised of several projects within the solution, created with Avalonia.
* **Core** – Shared application logic and services
* **Modules** – Provider implementations loaded by the client
* **API** – Backend services for accounts and synchronization
* **Database** - MongoDB is used for the database.

---

## Disclaimer

MaoMao does **not host or distribute any media content**.

**All content** is retrieved from third-party sources through user-installed modules.

**Users are responsible for ensuring they comply with the laws and terms of service applicable in their region.**

---

## Contributing

Contributions are welcome.
If you'd like to help improve MaoMao, feel free to open issues or submit pull requests.

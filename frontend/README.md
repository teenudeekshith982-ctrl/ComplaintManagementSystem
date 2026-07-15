# Complaint Management System - Frontend App

This is the Angular frontend application for the Complaint Management System. It is styled with a modern glassmorphism slate dark theme and utilizes reactive state handling via Angular Signals.

## Getting Started

### Prerequisites
Make sure you have [Node.js](https://nodejs.org/) installed (v18+ recommended).

### Install Dependencies
Run the following command in this directory to install the npm packages:
```bash
npm install
```

### Start Development Server
Run the Angular CLI local server:
```bash
ng serve
```
Navigate to `http://localhost:4200/` in your browser. The application will automatically reload if you change any source files.

---

## Tech Stack & Architecture

- **Core**: Angular 21.2.0 (using reactive Signals)
- **Styling**: Premium custom CSS system with HSL status colors, glowing borders, card gradients, and hover transitions.
- **Icons**: Google Material Symbols Outlined loaded from Google Fonts.
- **Real-Time updates**: SignalR Client for real-time ticket alerts.
- **Build Tool**: Angular CLI builder

---

## Code Scaffolding

To generate a new Angular element:
```bash
# Component
ng generate component Components/component-name

# Service
ng generate service Services/service-name
```

## Production Build

To build the optimized static assets in `/dist/frontend`:
```bash
ng build
```

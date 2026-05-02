---
version: alpha
name: Azure
description: Azure’s design system blends Microsoft’s Fluent 2.0 design language with the Azure-specific Ibiza framework used across the Azure Portal, Active Directory admin center, and cloud management surfaces. The system emphasizes clarity, density, accessibility, and operational trust. It pairs a neutral, information-first canvas with high-contrast Fluent tokens, modular panels, command bars, and resource blades. Rounded corners (4–8px), strong typographic hierarchy, and a consistent left-nav + blade pattern define the experience across dashboards, resource pages, and configuration flows.

colors:
  primary: "#019C01"        # App Green
  primary-dark: "#006300"
  primary-light: "#E8F5E8"
  accent-green: "#107C10"
  accent-red: "#D13438"
  accent-yellow: "#FCE100"
  canvas: "#FFFFFF"
  surface: "#F3F2F1"
  surface-alt: "#FAF9F8"
  border: "#E1DFDD"
  border-strong: "#C8C6C4"
  text-primary: "#201F1E"
  text-secondary: "#605E5C"
  text-disabled: "#A19F9D"
  nav-bg: "#1F1F1F"
  nav-text: "#FFFFFF"
  nav-text-secondary: "#C8C8C8"
  focus-ring: "#006300"

typography:
  display-xl:
    fontFamily: "Segoe UI Variable"
    fontSize: 56px
    fontWeight: 600
    lineHeight: 1.16
  display-lg:
    fontFamily: "Segoe UI Variable"
    fontSize: 40px
    fontWeight: 600
    lineHeight: 1.20
  heading-lg:
    fontFamily: "Segoe UI Variable"
    fontSize: 32px
    fontWeight: 600
    lineHeight: 1.25
  heading-md:
    fontFamily: "Segoe UI Variable"
    fontSize: 24px
    fontWeight: 600
    lineHeight: 1.30
  heading-sm:
    fontFamily: "Segoe UI Variable"
    fontSize: 20px
    fontWeight: 600
    lineHeight: 1.30
  body-md:
    fontFamily: "Segoe UI Variable"
    fontSize: 14px
    fontWeight: 400
    lineHeight: 1.43
  body-md-strong:
    fontFamily: "Segoe UI Variable"
    fontSize: 14px
    fontWeight: 600
    lineHeight: 1.43
  body-sm:
    fontFamily: "Segoe UI Variable"
    fontSize: 12px
    fontWeight: 400
    lineHeight: 1.33
  code:
    fontFamily: "Cascadia Code"
    fontSize: 12px
    fontWeight: 400
    lineHeight: 1.33

rounded:
  sm: 2px
  md: 4px
  lg: 6px
  xl: 8px
  full: 9999px

spacing:
  xxs: 4px
  xs: 8px
  sm: 12px
  md: 16px
  lg: 20px
  xl: 24px
  xxl: 32px
  section: 48px

components:
  button-primary:
    backgroundColor: "{colors.primary}"
    textColor: "{colors.canvas}"
    typography: "{typography.body-md-strong}"
    rounded: "{rounded.md}"
    padding: "8px 16px"
  button-primary-pressed:
    backgroundColor: "{colors.primary-dark}"
    textColor: "{colors.canvas}"
  button-secondary:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text-primary}"
    border: "1px solid {colors.border}"
    rounded: "{rounded.md}"
    padding: "8px 16px"
  button-command:
    backgroundColor: "transparent"
    textColor: "{colors.text-secondary}"
    typography: "{typography.body-sm}"
    padding: "6px 10px"
  panel:
    backgroundColor: "{colors.canvas}"
    rounded: "{rounded.lg}"
    padding: "{spacing.lg}"
    border: "1px solid {colors.border}"
  blade:
    backgroundColor: "{colors.canvas}"
    rounded: "{rounded.sm}"
    padding: "{spacing.md}"
    border: "1px solid {colors.border-strong}"
  nav-left:
    backgroundColor: "{colors.nav-bg}"
    textColor: "{colors.nav-text}"
    width: 260px
  card-resource:
    backgroundColor: "{colors.surface-alt}"
    rounded: "{rounded.lg}"
    padding: "{spacing.lg}"
    border: "1px solid {colors.border}"
  input-text:
    backgroundColor: "{colors.canvas}"
    textColor: "{colors.text-primary}"
    border: "1px solid {colors.border}"
    rounded: "{rounded.md}"
    height: 32px
    padding: "0 8px"
  input-text-focused:
    border: "2px solid {colors.focus-ring}"
  table:
    backgroundColor: "{colors.canvas}"
    textColor: "{colors.text-primary}"
    border: "1px solid {colors.border}"
    rowHeight: 32px
  badge-success:
    backgroundColor: "{colors.accent-green}"
    textColor: "{colors.canvas}"
    rounded: "{rounded.full}"
    padding: "2px 8px"
  badge-critical:
    backgroundColor: "{colors.accent-red}"
    textColor: "{colors.canvas}"
    rounded: "{rounded.full}"
    padding: "2px 8px"
---

# Overview

Azure’s design system is a hybrid of **Fluent 2.0** (Microsoft’s cross-platform design language) and **Ibiza**, the internal framework powering the Azure Portal. The result is a system optimized for **dense information environments**, **operational clarity**, and **enterprise-scale workflows**.

Azure’s UI is defined by:

- A **left navigation rail** with persistent global categories  
- **Blade-based navigation**, where panels slide horizontally as users drill into resources  
- **Command bars** for contextual actions  
- **Modular cards** for dashboards and resource summaries  
- **High-contrast Fluent tokens** for accessibility and clarity  
- **4–8px rounding** and **neutral surfaces** for a calm, operational feel  

The system prioritizes **legibility**, **efficiency**, and **predictability**, reflecting Azure’s role as a mission-critical cloud platform.

---

# Colors

Azure uses Fluent’s blue as its primary action color (`{colors.primary}`), paired with neutral grays and high-contrast text tokens. Surfaces are intentionally quiet to foreground data tables, metrics, and configuration forms.

### Brand & Accent
- **Green** (`{colors.primary}`): Primary action buttons, links, and focus states.
- **Dark Green** (`{colors.primary-dark}`): Pressed states and dark-surface variants.
- **Light Green** (`{colors.primary-light}`): Info callouts and selected-table-row backgrounds.
- **Green** (`{colors.accent-green}`): Success states (“Running”, “Healthy”).
- **Red** (`{colors.accent-red}`): Errors, failed deployments.
- **Yellow** (`{colors.accent-yellow}`): Warnings, cost alerts.

### Surface
- **Canvas White** (`{colors.canvas}`): Primary panel and blade surface.
- **Surface Gray** (`{colors.surface}`): Secondary surfaces and command bars.
- **Surface Alt** (`{colors.surface-alt}`): Dashboard cards and resource summaries.

### Text
- **Primary Text** (`{colors.text-primary}`): Headings, labels, table content.
- **Secondary Text** (`{colors.text-secondary}`): Helper text, metadata.
- **Disabled Text** (`{colors.text-disabled}`): Disabled controls and inactive states.

---

# Typography

Azure uses **Segoe UI Variable**, optimized for clarity at small sizes and dense data environments.

| Token | Size | Weight | Use |
|---|---|---|---|
| `{typography.display-xl}` | 56px | 600 | Marketing hero inside Azure.com |
| `{typography.heading-lg}` | 32px | 600 | Resource overview titles |
| `{typography.heading-md}` | 24px | 600 | Blade titles |
| `{typography.heading-sm}` | 20px | 600 | Section headers |
| `{typography.body-md}` | 14px | 400 | Primary body text |
| `{typography.body-md-strong}` | 14px | 600 | Table headers, labels |
| `{typography.body-sm}` | 12px | 400 | Metadata, timestamps |
| `{typography.code}` | 12px | 400 | CLI commands, logs |

---

# Layout

### Spacing System
Azure uses a **4px base grid**, with 8–16px as the dominant rhythm.

- `{spacing.xs}` (8px): Inline spacing  
- `{spacing.md}` (16px): Standard padding  
- `{spacing.lg}` (20px): Panel gutters  
- `{spacing.section}` (48px): Page-level section spacing  

### Grid & Structure
- **Left nav**: 260px fixed  
- **Blade width**: 680–720px  
- **Dashboard cards**: 2–4 column responsive grid  
- **Tables**: Full-width, dense, 32px row height  

### Whitespace Philosophy
Whitespace is functional, not expressive. Azure prioritizes:

- Dense data tables  
- Clear separation between blades  
- Predictable spacing between form groups  

---

# Elevation & Depth

Azure uses minimal elevation. Shadows appear only for:

| Level | Treatment | Use |
|---|---|---|
| 0 | Flat, no shadow | Default panels, blades |
| 1 | Subtle 1–2px shadow | Command bars, dropdowns |
| 2 | Stronger shadow | Modals, flyouts |

---

# Shapes

Azure uses **2–8px rounding**, reflecting Fluent’s geometry.

| Token | Value | Use |
|---|---|---|
| `{rounded.sm}` | 2px | Inputs, small controls |
| `{rounded.md}` | 4px | Buttons, cards |
| `{rounded.lg}` | 6px | Panels |
| `{rounded.xl}` | 8px | Large surfaces |
| `{rounded.full}` | 9999px | Pills, status badges |

---

# Components

### Buttons

**`button-primary`** — Azure Blue action button  
- Background `{colors.primary}`, text `{colors.canvas}`, rounded `{rounded.md}`  
- Pressed: `{colors.primary-dark}`  

**`button-secondary`** — Neutral secondary action  
- Background `{colors.surface}`, border `{colors.border}`  

**`button-command`** — Command bar action  
- Transparent background, small footprint  

---

### Navigation

**Left Navigation Rail**  
- Background `{colors.nav-bg}`  
- Text `{colors.nav-text}`  
- Persistent global categories  

**Blade Navigation**  
- Horizontal sliding panels  
- Each blade is a self-contained resource context  

---

### Cards & Panels

**`card-resource`**  
- Used for resource summaries, metrics, quick actions  

**`panel`**  
- Standard configuration container  

**`blade`**  
- Azure’s signature navigation unit  

---

### Inputs & Forms

**`input-text`**  
- 32px height, `{rounded.md}`, 1px border  

**`input-text-focused`**  
- 2px `{colors.focus-ring}` border  

**Tables**  
- Dense, 32px rows, strong header contrast  

---

### Status & Badges

**`badge-success`** — Running, Healthy  
**`badge-critical`** — Failed, Error  

---

# Signature Patterns

### Blade Navigation
Azure’s defining pattern: each drill-down opens a new blade to the right, preserving context.

### Command Bar
Contextual actions appear at the top of each blade.

### Resource Overview Cards
Summaries of metrics, logs, and quick actions.

### Dense Tables
Azure’s most common component — optimized for scanning and filtering.

---

# Summary

Azure’s design system is a **functional, enterprise-grade adaptation of Fluent 2.0**, shaped by the operational needs of cloud administrators. It is:

- **Data-dense**  
- **Predictable**  
- **Accessible**  
- **Modular**  
- **Navigation-rich**  

The Ibiza framework provides the structure; Fluent 2.0 provides the visual language.


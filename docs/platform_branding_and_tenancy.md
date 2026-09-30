# StoreOS Branding & Tenancy Guidelines

This document outlines the official naming conventions, branding guidelines, and multi-tenant URL structures for the platform.

## 1. Platform Name
The official commercial name of the platform is **StoreOS** (or *Store OS*). 
All UI elements, documentation, and external-facing SaaS marketing should refer to the platform as StoreOS, acting as the underlying "operating system" for a tenant's retail/ERP operations.

## 2. Dynamic Tenant Relationship Convention
StoreOS uses a dynamic `@` (at) convention to describe the relationship between the tenant and the platform. The UI branding dynamically shifts based on how the tenant accesses the application (SaaS Subdomain vs. Custom BYOD).

### A. Standard SaaS Tier (Platform Subdomain)
If the tenant uses the default StoreOS infrastructure, the platform brand takes precedence as the host.
* **Domain:** `acme.storeos.com`
* **UI Branding:** `[Tenant Name] @ StoreOS` (e.g., *Acme @ StoreOS*)
* **Psychology:** The tenant is renting a workspace on the StoreOS platform.

### B. Enterprise / Premium Tier (Bring Your Own Domain - BYOD)
If the tenant maps a custom domain (white-labeling), the platform fades into the background and "Store" becomes an internal department/tool name.
* **Domain:** `store.acme.com` or `pos.acme.com`
* **UI Branding:** `Store @ [Tenant Name]` (e.g., *Store @ Acme*)
* **Psychology:** The software is perceived as Acme's proprietary internal tool.

## 3. Seed / Demo Tenant (Clexan Foods)
**"Clexan Foods"** is strictly the name of our internal seed/demo tenant. 
* It is **not** the name of the software platform. 
* It is the default tenant used in `appsettings.Development.json` for local debugging and is the first tenant provisioned when standing up a local environment.

## 4. Workspaces and Routing
StoreOS leverages a workspace-based routing approach, reflecting the dynamic branding rules above.

* **Subdomain Routing:** The primary way a standard tenant accesses their instance in production is via a dedicated subdomain (e.g., `acme.storeos.com`).
* **BYOD Routing:** Enterprise tenants point their CNAME records to the Control Plane, which resolves the custom domain (e.g., `store.acme.com`) to their isolated container stack.
* **Login Portal:** When logging in via the central Tenant Portal, users will be prompted for their **Workspace ID** (e.g., `acme`). Upon successful authentication, they are routed to the appropriate domain, and the UI dynamically adapts its branding.

